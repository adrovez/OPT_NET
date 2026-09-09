using MediatR;
using OPT.Application.Features.RecetaCristales;

namespace OPT.Application.Features.RecetaCristales.Queries.ObtenerPorCliente;

public record ObtenerRecetaCristalesPorClienteQuery(Guid ClientePublicId) : IRequest<IReadOnlyList<RecetaCristalesDto>>;
