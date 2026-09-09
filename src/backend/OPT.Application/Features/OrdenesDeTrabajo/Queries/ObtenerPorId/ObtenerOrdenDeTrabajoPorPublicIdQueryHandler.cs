using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Queries.ObtenerPorId;

public sealed class ObtenerOrdenDeTrabajoPorPublicIdQueryHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory)
    : IRequestHandler<ObtenerOrdenDeTrabajoPorPublicIdQuery, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(
        ObtenerOrdenDeTrabajoPorPublicIdQuery request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.PublicId);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
