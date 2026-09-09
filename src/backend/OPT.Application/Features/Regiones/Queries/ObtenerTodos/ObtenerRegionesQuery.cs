using MediatR;
using OPT.Application.Features.Regiones;

namespace OPT.Application.Features.Regiones.Queries.ObtenerTodos;

public record ObtenerRegionesQuery : IRequest<IReadOnlyList<RegionDto>>;
