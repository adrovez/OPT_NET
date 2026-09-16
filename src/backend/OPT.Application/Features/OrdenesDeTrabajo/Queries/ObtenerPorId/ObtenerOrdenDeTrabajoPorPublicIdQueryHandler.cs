using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Queries.ObtenerPorId;

public sealed class ObtenerOrdenDeTrabajoPorPublicIdQueryHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser)
    : IRequestHandler<ObtenerOrdenDeTrabajoPorPublicIdQuery, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(
        ObtenerOrdenDeTrabajoPorPublicIdQuery request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, orden.SucursalId);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
