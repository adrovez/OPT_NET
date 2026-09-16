namespace OPT.Application.Common.Interfaces;

/// <summary>
/// Abstracción del usuario autenticado en la sesión actual.
/// La implementación concreta vive en Infrastructure (lee el JWT del HttpContext).
/// </summary>
public interface ICurrentUserService
{
    int    UsuarioId    { get; }
    string Rut          { get; }
    int    RolId        { get; }
    int?   SucursalId   { get; }

    /// <summary>Todas las sucursales asignadas al usuario (tabla UsuarioSucursal), no solo la activa.</summary>
    IReadOnlyCollection<int> SucursalesAsignadas { get; }
    bool   EstaAutenticado { get; }
}
