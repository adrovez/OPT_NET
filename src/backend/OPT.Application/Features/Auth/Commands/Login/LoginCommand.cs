using MediatR;

namespace OPT.Application.Features.Auth.Commands.Login;

/// <summary>
/// Autenticación con RUT + clave (convención de negocio heredada — preservar, reglas-negocio-legado.md).
/// Retorna el token JWT y la sucursal activa asignada al usuario.
/// </summary>
public record LoginCommand(string Rut, string Clave) : IRequest<LoginResult>;

public record LoginResult(string Token, int UsuarioId, string NombreCompleto, int SucursalActivaId);
