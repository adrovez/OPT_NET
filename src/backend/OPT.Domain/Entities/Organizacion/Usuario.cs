using OPT.Domain.Common;

namespace OPT.Domain.Entities.Organizacion;

/// <summary>
/// Persona que opera el sistema. Se identifica con RUT (login), pero la PK es sintética (ADR 0003).
/// La contraseña siempre se almacena como hash — nunca en texto plano (CLAUDE.md / reglas seguridad).
/// </summary>
public class Usuario : AuditableEntity
{
    /// <summary>
    /// Identificador no enumerable expuesto en API/URLs — nunca el Id interno (ADR 0004, Ley 21.719).
    /// Generado por la base de datos (DEFAULT NEWID()).
    /// </summary>
    public Guid   PublicId   { get; private set; }

    /// <summary>
    /// RUT del operador (dato de negocio para login).
    /// Es atributo con unicidad, NO la clave primaria (ADR 0003).
    /// </summary>
    public string Rut        { get; private set; } = string.Empty;
    public string Nombre     { get; private set; } = string.Empty;
    public string Apellido   { get; private set; } = string.Empty;
    public string? Email     { get; private set; }

    /// <summary>Hash bcrypt/Argon2 — nunca texto plano.</summary>
    public string ClaveHash  { get; private set; } = string.Empty;

    public int    RolId      { get; private set; }
    public Rol?   Rol        { get; private set; }

    /// <summary>Sucursal activa durante la sesión actual (se fija al hacer login).</summary>
    public int?      SucursalActivaId { get; private set; }
    public Sucursal?  SucursalActiva  { get; private set; }

    public bool Activo { get; private set; } = true;

    private readonly List<UsuarioSucursal> _sucursales = [];
    public IReadOnlyCollection<UsuarioSucursal> Sucursales => _sucursales.AsReadOnly();

    protected Usuario() { }

    public static Usuario Crear(string rut, string nombre, string apellido,
                                 string claveHash, int rolId, int usuarioCreadorId,
                                 string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rut);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(claveHash);

        var u = new Usuario
        {
            Rut       = rut.Trim().ToUpperInvariant(),
            Nombre    = nombre.Trim(),
            Apellido  = apellido.Trim(),
            ClaveHash = claveHash,
            RolId     = rolId,
            Email     = email?.Trim().ToLowerInvariant()
        };
        u.SetCreacion(usuarioCreadorId);
        return u;
    }

    public void Actualizar(string rut, string nombre, string apellido, string? email,
                            int rolId, int usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rut);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(apellido);

        Rut      = rut.Trim().ToUpperInvariant();
        Nombre   = nombre.Trim();
        Apellido = apellido.Trim();
        Email    = email?.Trim().ToLowerInvariant();
        RolId    = rolId;
        SetModificacion(usuarioId);
    }

    /// <summary>Asigna la sucursal activa post-login.</summary>
    public void SetSucursalActiva(int sucursalId) => SucursalActivaId = sucursalId;

    /// <summary>Asigna una sucursal adicional al usuario (idempotente — no duplica si ya está asignada).</summary>
    public void AsignarSucursal(int sucursalId)
    {
        if (_sucursales.Any(us => us.SucursalId == sucursalId)) return;
        _sucursales.Add(new UsuarioSucursal(Id, sucursalId));
    }

    /// <summary>Quita una sucursal previamente asignada (no-op si no estaba asignada).</summary>
    public void QuitarSucursal(int sucursalId)
    {
        var asignacion = _sucursales.FirstOrDefault(us => us.SucursalId == sucursalId);
        if (asignacion is not null) _sucursales.Remove(asignacion);
    }

    /// <summary>Actualiza el hash de contraseña (flujo de restablecimiento, no recuperación en texto).</summary>
    public void ActualizarClave(string nuevoHash, int usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nuevoHash);
        ClaveHash = nuevoHash;
        SetModificacion(usuarioId);
    }

    public void Desactivar(int usuarioId)
    {
        Activo = false;
        SetModificacion(usuarioId);
    }

    public void Activar(int usuarioId)
    {
        Activo = true;
        SetModificacion(usuarioId);
    }
}
