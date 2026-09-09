using MediatR;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Regiones.Queries.ObtenerTodos;

public sealed class ObtenerRegionesQueryHandler(IRegionRepositorio regionRepo)
    : IRequestHandler<ObtenerRegionesQuery, IReadOnlyList<RegionDto>>
{
    public async Task<IReadOnlyList<RegionDto>> Handle(ObtenerRegionesQuery request, CancellationToken ct)
    {
        var regiones = await regionRepo.ObtenerTodosAsync(ct);
        return regiones.Select(r => new RegionDto(r.Id, r.Nombre, r.CodigoOficial)).ToList();
    }
}
