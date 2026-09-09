using MediatR;

namespace OPT.Application.Features.Usuarios.Commands.Eliminar;

public record EliminarUsuarioCommand(Guid PublicId) : IRequest;
