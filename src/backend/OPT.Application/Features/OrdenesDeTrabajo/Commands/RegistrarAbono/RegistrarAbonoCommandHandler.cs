using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.RegistrarAbono;

public sealed class RegistrarAbonoCommandHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    IFormaPagoRepositorio      formaPagoRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<RegistrarAbonoCommand, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(RegistrarAbonoCommand request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.OrdenPublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.OrdenPublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, orden.SucursalId);

        if (!await formaPagoRepo.ExisteAsync(request.FormaPagoId, ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["FormaPagoId"] = ["La forma de pago indicada no existe."]
            });

        orden.RegistrarAbono(request.Monto, request.FormaPagoId, currentUser.UsuarioId,
            request.Referencia);

        ordenRepo.Actualizar(orden);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
