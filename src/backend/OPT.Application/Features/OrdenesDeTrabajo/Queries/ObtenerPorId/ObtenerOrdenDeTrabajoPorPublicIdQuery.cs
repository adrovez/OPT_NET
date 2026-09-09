using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Queries.ObtenerPorId;

public record ObtenerOrdenDeTrabajoPorPublicIdQuery(Guid PublicId) : IRequest<OrdenDeTrabajoDto>;
