using MediatR;
using OPT.Application.Features.Clientes;

namespace OPT.Application.Features.Clientes.Queries.ObtenerPorId;

public record ObtenerClientePorPublicIdQuery(Guid PublicId) : IRequest<ClienteDto>;
