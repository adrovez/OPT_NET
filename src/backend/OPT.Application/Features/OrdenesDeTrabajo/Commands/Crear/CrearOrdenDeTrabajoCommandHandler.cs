using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;
using EntidadOT = OPT.Domain.Entities.Comercial.OrdenDeTrabajo;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Crear;

public sealed class CrearOrdenDeTrabajoCommandHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    IClienteRepositorio        clienteRepo,
    ISucursalRepositorio       sucursalRepo,
    IEmpresaRepositorio        empresaRepo,
    IProductoRepositorio       productoRepo,
    IFormaPagoRepositorio      formaPagoRepo,
    IRecetaCristalesRepositorio recetaRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<CrearOrdenDeTrabajoCommand, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(CrearOrdenDeTrabajoCommand request, CancellationToken ct)
    {
        var errores = new Dictionary<string, string[]>();

        var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.ClientePublicId, ct);
        if (cliente is null) errores["ClientePublicId"] = ["El cliente indicado no existe."];

        var sucursal = await sucursalRepo.ObtenerPorIdAsync(request.SucursalId, ct);
        if (sucursal is null) errores["SucursalId"] = ["La sucursal indicada no existe."];

        var empresaId = (int?)null;
        if (request.EmpresaPublicId is not null)
        {
            var empresa = await empresaRepo.ObtenerPorPublicIdAsync(request.EmpresaPublicId.Value, ct);
            if (empresa is null) errores["EmpresaPublicId"] = ["La empresa indicada no existe."];
            else empresaId = empresa.Id;
        }

        var productoIds = request.Detalles.Select(d => d.ProductoId).Distinct().ToList();
        var productos   = await productoRepo.BuscarAsync(p => productoIds.Contains(p.Id), ct);
        var faltantes   = productoIds.Except(productos.Select(p => p.Id)).ToList();
        if (faltantes.Count > 0)
            errores["Detalles"] = [$"Los siguientes productos no existen: {string.Join(", ", faltantes)}."];

        if (request.AbonoInicial is > 0 &&
            !await formaPagoRepo.ExisteAsync(request.FormaPagoAbono ?? -1, ct))
            errores["FormaPagoAbono"] = ["La forma de pago indicada no existe."];

        // La receta tiene que ser del mismo cliente: vincular la de otra persona sería
        // fabricar cristales con la graduación equivocada.
        var receta = request.RecetaPublicId is null
            ? null
            : await recetaRepo.ObtenerPorPublicIdAsync(request.RecetaPublicId.Value, ct);
        if (request.RecetaPublicId is not null && receta is null)
            errores["RecetaPublicId"] = ["La receta indicada no existe."];
        else if (receta is not null && cliente is not null && receta.ClienteId != cliente.Id)
            errores["RecetaPublicId"] = ["La receta indicada pertenece a otro cliente."];

        if (errores.Count > 0) throw new ValidationException(errores);

        // BOLA/IDOR: no se puede crear una OT en una sucursal a la que el usuario no
        // está asignado — recién acá, para que un SucursalId inexistente siga dando el
        // error de validación de arriba en vez de un 403 confuso.
        AutorizacionSucursal.ValidarAcceso(currentUser, request.SucursalId);

        // N° de OT manual (decisión 2026-09-11, como el legacy): único por año entre las OT que
        // no estén anuladas. Se valida aparte de `errores` para dar un mensaje específico —
        // "Errores de validación." genérico no le dice al usuario qué número está repetido.
        var anioActual = DateTimeOffset.UtcNow.Year;
        if (await ordenRepo.ExisteNumeroOTVigenteAsync(request.NumeroOT, anioActual, ct))
            throw new DomainException(
                $"Ya existe una Orden de Trabajo N° {request.NumeroOT} en el año {anioActual} " +
                "que no está anulada. Ingrese otro número o verifique el estado de esa orden.");

        var usuarioId = currentUser.UsuarioId;

        var orden = EntidadOT.Crear(request.NumeroOT, cliente!.Id, request.SucursalId,
            request.FechaEntrega, usuarioId, empresaId, request.Observaciones,
            request.Beneficiario, request.FechaAtencion, request.HoraEntrega);

        orden.ReemplazarDetalles(
            request.Detalles.Select(d => (d.ProductoId, d.Cantidad, d.ValorUnitario, d.Comentario)),
            usuarioId);

        if (request.NumeroCuotas is > 0)
            orden.GenerarPlanCuotas(request.NumeroCuotas.Value,
                request.PrimerVencimiento ?? DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1),
                usuarioId);

        // El abono va después del plan de cuotas para que el saldo y las cuotas queden
        // consistentes en la misma transacción (ADR 0003).
        if (request.AbonoInicial is > 0)
            orden.RegistrarAbono(request.AbonoInicial.Value, request.FormaPagoAbono!.Value,
                usuarioId, request.ReferenciaAbono);

        ordenRepo.Agregar(orden);
        await uow.CommitAsync(ct);

        // La receta se vincula después del primer commit: antes de él la OT todavía no tiene Id.
        if (receta is not null)
        {
            receta.AsociarAOrden(orden.Id, usuarioId);
            recetaRepo.Actualizar(receta);
            await uow.CommitAsync(ct);
        }

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
