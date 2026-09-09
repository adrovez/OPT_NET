using MediatR;
using OPT.Application.Features.Comunas;

namespace OPT.Application.Features.Comunas.Queries.ObtenerPorRegion;

public record ObtenerComunasPorRegionQuery(int RegionId) : IRequest<IReadOnlyList<ComunaDto>>;
