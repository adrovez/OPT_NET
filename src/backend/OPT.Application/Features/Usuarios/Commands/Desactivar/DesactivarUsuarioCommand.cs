using MediatR;

namespace OPT.Application.Features.Usuarios.Commands.Desactivar;

public record DesactivarUsuarioCommand(Guid PublicId) : IRequest;
