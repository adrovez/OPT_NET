using MediatR;
using OPT.Application.Features.Usuarios;

namespace OPT.Application.Features.Usuarios.Commands.Actualizar;

public record ActualizarUsuarioCommand(
    Guid PublicId, string Rut, string Nombre, string Apellido, string? Email, int RolId) : IRequest<UsuarioDto>;
