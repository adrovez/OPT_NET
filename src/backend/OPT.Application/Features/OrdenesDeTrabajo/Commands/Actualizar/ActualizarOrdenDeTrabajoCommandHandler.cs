using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Actualizar;

public sealed class ActualizarOrdenDeTrabajoCommandHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    IEmpresaRepositorio        empresaRepo,
    IProductoRepositorio       productoRepo,
    IRecetaCristalesRepositorio recetaRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<ActualizarOrdenDeTrabajoCommand, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(ActualizarOrdenDeTrabajoCommand request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.PublicId);

        var errores = new Dictionary<string, string[]>();

        var empresaId = (int?)null;
        if (request.EmpresaPublicId is not null)
        {
            var empresa = await empresaRepo.ObtenerPorPublicIdAsync(request.EmpresaPublicId.Value, ct);
            if (empresa is null) errores["EmpresaPublicId"] = ["La empresa indicada no existe."];
            else empresaId = empresa.Id;
        }

        if (request.Detalles is { Count: > 0 })
        {
            var productoIds = request.Detalles.Select(d => d.ProductoId).Distinct().ToList();
            var productos   = await productoRepo.BuscarAsync(p => productoIds.Contains(p.Id), ct);
            var faltantes   = productoIds.Except(productos.Select(p => p.Id)).ToList();
            if (faltantes.Count > 0)
                errores["Detalles"] = [$"Los siguientes productos no existen: {string.Join(", ", faltantes)}."];
        }

        var receta = request.RecetaPublicId is null
            ? null
            : await recetaRepo.ObtenerPorPublicIdAsync(request.RecetaPublicId.Value, ct);
        if (request.RecetaPublicId is not null && receta is null)
            errores["RecetaPublicId"] = ["La receta indicada no existe."];
        else if (receta is not null && receta.ClienteId != orden.ClienteId)
            errores["RecetaPublicId"] = ["La receta indicada pertenece a otro cliente."];

        if (errores.Count > 0) throw new ValidationException(errores);

        var usuarioId = currentUser.UsuarioId;

        orden.Actualizar(request.FechaEntrega, usuarioId, empresaId, request.Observaciones,
            request.Beneficiario, request.FechaAtencion, request.HoraEntrega);

        if (request.Detalles is not null)
            orden.ReemplazarDetalles(
                request.Detalles.Select(d => (d.ProductoId, d.Cantidad, d.ValorUnitario, d.Comentario)),
                usuarioId);

        // El vínculo receta↔OT vive en la receta, no en la orden: se desvincula lo que había
        // y se vincula lo recibido, para que la OT no quede con dos recetas por una edición.
        foreach (var vigente in await recetaRepo.ObtenerPorOrdenAsync(orden.Id, ct))
        {
            if (receta is null || vigente.Id != receta.Id)
            {
                vigente.AsociarAOrden(null, usuarioId);
                recetaRepo.Actualizar(vigente);
            }
        }

        if (receta is not null && receta.OrdenDeTrabajoId != orden.Id)
        {
            receta.AsociarAOrden(orden.Id, usuarioId);
            recetaRepo.Actualizar(receta);
        }

        ordenRepo.Actualizar(orden);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
