using OPT.Domain.Common;

namespace OPT.Domain.Entities.Inventario;

/// <summary>
/// Lugar físico donde se guarda el stock. Una sucursal tiene una o más bodegas y exactamente
/// una es la principal (ADR 0012). No referencia a Sucursal: solo guarda su Id.
/// </summary>
public class Bodega : AuditableEntity
{
    /// <summary>Identificador no enumerable para API/URLs; lo genera la BD.</summary>
    public Guid   PublicId    { get; private set; }
    public int    SucursalId  { get; private set; }
    public string Codigo      { get; private set; } = string.Empty;
    public string Nombre      { get; private set; } = string.Empty;
    public bool   EsPrincipal { get; private set; }
    public bool   Activa      { get; private set; } = true;

    protected Bodega() { }

    public static Bodega Crear(int sucursalId, string codigo, string nombre, bool esPrincipal, int usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        var b = new Bodega
        {
            SucursalId  = sucursalId,
            Codigo      = codigo.Trim().ToUpperInvariant(),
            Nombre      = nombre.Trim(),
            EsPrincipal = esPrincipal,
            Activa      = true
        };
        b.SetCreacion(usuarioId);
        return b;
    }

    public void Actualizar(string codigo, string nombre, int usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        Codigo = codigo.Trim().ToUpperInvariant();
        Nombre = nombre.Trim();
        SetModificacion(usuarioId);
    }

    /// <summary>Marca esta bodega como principal. Quien llama debe quitar la marca a la anterior.</summary>
    public void MarcarPrincipal(int usuarioId)
    {
        if (!Activa) throw new DomainException("Una bodega inactiva no puede ser la principal.");
        EsPrincipal = true;
        SetModificacion(usuarioId);
    }

    public void QuitarPrincipal(int usuarioId)
    {
        EsPrincipal = false;
        SetModificacion(usuarioId);
    }

    /// <param name="stockTotal">Stock acumulado de la bodega; con stock no se puede desactivar.</param>
    public void Desactivar(int stockTotal, int usuarioId)
    {
        if (EsPrincipal)
            throw new DomainException("No se puede desactivar la bodega principal: designe otra primero.");
        if (stockTotal > 0)
            throw new DomainException("No se puede desactivar una bodega con stock.");
        Activa = false;
        SetModificacion(usuarioId);
    }

    public void Activar(int usuarioId)
    {
        Activa = true;
        SetModificacion(usuarioId);
    }
}
