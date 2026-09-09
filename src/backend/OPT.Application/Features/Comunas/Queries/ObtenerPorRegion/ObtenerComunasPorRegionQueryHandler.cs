using MediatR;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Comunas.Queries.ObtenerPorRegion;

public sealed class ObtenerComunasPorRegionQueryHandler(IComunaRepositorio comunaRepo)
    : IRequestHandler<ObtenerComunasPorRegionQuery, IReadOnlyList<ComunaDto>>
{
    public async Task<IReadOnlyList<ComunaDto>> Handle(ObtenerComunasPorRegionQuery request, CancellationToken ct)
    {
        var comunas = await comunaRepo.ObtenerPorRegionAsync(request.RegionId, ct);
        return comunas.Select(c => new ComunaDto(c.Id, c.Nombre, c.RegionId)).ToList();
    }
}
