using MediatR;

namespace OPT.Application.Features.Usuarios.Commands.CambiarClave;

public record CambiarClaveUsuarioCommand(Guid PublicId, string ClaveActual, string ClaveNueva) : IRequest;
