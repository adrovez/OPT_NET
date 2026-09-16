using MediatR;
using OPT.Application.Features.EstadosOperativo;

namespace OPT.Application.Features.EstadosOperativo.Queries.ObtenerTodos;

/// <summary>Catálogo completo de estados de Operativo — sembrado, sin paginar (5 filas).</summary>
public record ObtenerEstadosOperativoQuery : IRequest<IReadOnlyList<EstadoOperativoDto>>;
