using MediatR;

namespace OPT.Application.Features.Bodegas.Queries.ObtenerPorId;

public record ObtenerBodegaPorPublicIdQuery(Guid PublicId) : IRequest<BodegaDto>;
