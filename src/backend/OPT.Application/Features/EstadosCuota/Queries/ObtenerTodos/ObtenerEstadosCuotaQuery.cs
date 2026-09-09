using MediatR;

namespace OPT.Application.Features.EstadosCuota.Queries.ObtenerTodos;

/// <summary>Catálogo completo de estados de cuota — sembrado, sin paginar (3 filas).</summary>
public record ObtenerEstadosCuotaQuery : IRequest<IReadOnlyList<EstadoCuotaDto>>;
