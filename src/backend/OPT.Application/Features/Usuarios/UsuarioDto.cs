namespace OPT.Application.Features.Usuarios;

public record UsuarioDto(
    Guid PublicId, string Rut, string Nombre, string Apellido, string? Email,
    int RolId, string RolNombre, int? SucursalActivaId, bool Activo);
