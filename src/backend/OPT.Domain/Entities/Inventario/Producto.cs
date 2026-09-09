using OPT.Domain.Common;

namespace OPT.Domain.Entities.Inventario;

/// <summary>
/// Ítem de catálogo (armazón, cristal, accesorio).
/// ControlStock indica si este producto lleva gestión de inventario
/// (posiblemente servicios no físicos no lo requieren — [Reconsiderar] con el negocio).
/// </summary>
public class Producto : AuditableEntity
{
    /// <summary>Código único de catálogo (distinto del Id interno).</summary>
    public string  Codigo       { get; private set; } = string.Empty;
    public string  Descripcion  { get; private set; } = string.Empty;
    public bool    ControlStock { get; private set; }
    public int     CategoriaId  { get; private set; }   // FK a catálogo de categorías

    private readonly List<ProductoSucursal> _sucursales = [];
    public IReadOnlyCollection<ProductoSucursal> Sucursales => _sucursales.AsReadOnly();

    protected Producto() { }

    public static Producto Crear(string codigo, string descripcion, bool controlStock,
                                  int categoriaId, int usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var p = new Producto
        {
            Codigo       = codigo.Trim().ToUpperInvariant(),
            Descripcion  = descripcion.Trim(),
            ControlStock = controlStock,
            CategoriaId  = categoriaId
        };
        p.SetCreacion(usuarioId);
        return p;
    }
}
