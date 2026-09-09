using MediatR;
using OPT.Application.Features.Usuarios;

namespace OPT.Application.Features.Usuarios.Commands.Crear;

public record CrearUsuarioCommand(
    string Rut, string Nombre, string Apellido, string Clave, int RolId, string? Email) : IRequest<UsuarioDto>;
