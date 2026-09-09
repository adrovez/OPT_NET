using MediatR;
using OPT.Application.Features.RecetaCristales;

namespace OPT.Application.Features.RecetaCristales.Queries.ObtenerPorId;

public record ObtenerRecetaCristalesPorPublicIdQuery(Guid PublicId) : IRequest<RecetaCristalesDto>;
