using MediatR;

namespace OPT.Application.Features.Usuarios.Commands.Activar;

public record ActivarUsuarioCommand(Guid PublicId) : IRequest;
