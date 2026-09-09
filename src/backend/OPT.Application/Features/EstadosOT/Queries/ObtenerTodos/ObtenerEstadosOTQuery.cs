using MediatR;
using OPT.Application.Features.EstadosOT;

namespace OPT.Application.Features.EstadosOT.Queries.ObtenerTodos;

/// <summary>Catálogo completo de estados de OT — sembrado, sin paginar (8 filas).</summary>
public record ObtenerEstadosOTQuery : IRequest<IReadOnlyList<EstadoOTDto>>;
