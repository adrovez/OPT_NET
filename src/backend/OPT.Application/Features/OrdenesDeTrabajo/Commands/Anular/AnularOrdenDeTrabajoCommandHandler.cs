using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Anular;

public sealed class AnularOrdenDeTrabajoCommandHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<AnularOrdenDeTrabajoCommand, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(AnularOrdenDeTrabajoCommand request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, orden.SucursalId);

        orden.Anular(request.Motivo, currentUser.UsuarioId);

        ordenRepo.Actualizar(orden);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
