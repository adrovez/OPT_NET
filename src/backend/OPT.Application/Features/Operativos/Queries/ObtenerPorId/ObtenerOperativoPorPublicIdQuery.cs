using MediatR;

namespace OPT.Application.Features.Operativos.Queries.ObtenerPorId;

public record ObtenerOperativoPorPublicIdQuery(Guid PublicId) : IRequest<OperativoDto>;
