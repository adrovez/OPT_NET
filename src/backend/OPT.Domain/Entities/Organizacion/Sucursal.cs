using OPT.Domain.Common;

namespace OPT.Domain.Entities.Organizacion;

/// <summary>
/// Punto de atención físico de la óptica.
/// Una sucursal puede ser "matriz". Los usuarios se asignan a una o más sucursales.
/// </summary>
public class Sucursal : AuditableEntity
{
    public string Nombre    { get; private set; } = string.Empty;
    public string? Direccion { get; private set; }
    public string? Telefono  { get; private set; }
    public bool   EsMatriz  { get; private set; }

    // Navegación
    private readonly List<UsuarioSucursal> _usuarios = [];
    public IReadOnlyCollection<UsuarioSucursal> Usuarios => _usuarios.AsReadOnly();

    protected Sucursal() { }

    public static Sucursal Crear(string nombre, bool esMatriz, int usuarioId,
                                  string? direccion = null, string? telefono = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        var s = new Sucursal
        {
            Nombre    = nombre.Trim(),
            EsMatriz  = esMatriz,
            Direccion = direccion?.Trim(),
            Telefono  = telefono?.Trim()
        };
        s.SetCreacion(usuarioId);
        return s;
    }

    public void Actualizar(string nombre, string? direccion, string? telefono, int usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        Nombre    = nombre.Trim();
        Direccion = direccion?.Trim();
        Telefono  = telefono?.Trim();
        SetModificacion(usuarioId);
    }
}
