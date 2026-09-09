using MediatR;

namespace OPT.Application.Features.FormasPago.Queries.ObtenerTodos;

/// <summary>Catálogo completo de medios de pago — sembrado, sin paginar (6 filas).</summary>
public record ObtenerFormasPagoQuery : IRequest<IReadOnlyList<FormaPagoDto>>;
