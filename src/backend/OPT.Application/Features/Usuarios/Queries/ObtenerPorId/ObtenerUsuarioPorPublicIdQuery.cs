using MediatR;
using OPT.Application.Features.Usuarios;

namespace OPT.Application.Features.Usuarios.Queries.ObtenerPorId;

public record ObtenerUsuarioPorPublicIdQuery(Guid PublicId) : IRequest<UsuarioDto>;
