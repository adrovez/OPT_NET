using OPT.Domain.Common;

namespace OPT.Domain.Entities.Inventario;

/// <summary>
/// Ítem de catálogo (armazón, cristal, lente de sol, otros).
/// ControlStock indica si este producto lleva gestión de inventario
/// (posiblemente servicios no físicos no lo requieren — [Reconsiderar] con el negocio).
///
/// Marcos y Lentes de Sol se identifican por Marca + Modelo (Color opcional) y su Descripción
/// se CALCULA como "Marca-Modelo[-Color]" (ADR 0013); las demás categorías usan Descripción libre.
/// </summary>
public class Producto : AuditableEntity
{
    /// <summary>Código único de catálogo (distinto del Id interno).</summary>
    public string  Codigo       { get; private set; } = string.Empty;
    public string  Descripcion  { get; private set; } = string.Empty;
    public string? Marca        { get; private set; }
    public string? Modelo       { get; private set; }
    public string? Color        { get; private set; }
    public bool    ControlStock { get; private set; }
    public int     CategoriaId  { get; private set; }   // FK a catálogo de categorías

    private readonly List<ProductoSucursal> _sucursales = [];
    public IReadOnlyCollection<ProductoSucursal> Sucursales => _sucursales.AsReadOnly();

    protected Producto() { }

    public static Producto Crear(string codigo, string? descripcion, string? marca, string? modelo,
                                  string? color, bool controlStock, int categoriaId, int usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        var p = new Producto { Codigo = codigo.Trim().ToUpperInvariant() };
        p.AplicarDatos(descripcion, marca, modelo, color, controlStock, categoriaId);
        p.SetCreacion(usuarioId);
        return p;
    }

    public void Actualizar(string codigo, string? descripcion, string? marca, string? modelo,
                           string? color, bool controlStock, int categoriaId, int usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        Codigo = codigo.Trim().ToUpperInvariant();
        AplicarDatos(descripcion, marca, modelo, color, controlStock, categoriaId);
        SetModificacion(usuarioId);
    }

    /// <summary>"Marca-Modelo" o "Marca-Modelo-Color" — mismo formato de la descripción legacy.</summary>
    public static string ComponerDescripcion(string marca, string modelo, string? color)
        => string.IsNullOrWhiteSpace(color) ? $"{marca}-{modelo}" : $"{marca}-{modelo}-{color}";

    private void AplicarDatos(string? descripcion, string? marca, string? modelo, string? color,
                              bool controlStock, int categoriaId)
    {
        var marcaN  = Normalizar(marca);
        var modeloN = Normalizar(modelo);
        var colorN  = Normalizar(color);

        if (CategoriaProductoIds.ExigeMarcaYModelo(categoriaId))
        {
            if (marcaN is null || modeloN is null)
                throw new DomainException("La marca y el modelo son obligatorios para esta categoría.");

            Descripcion = ComponerDescripcion(marcaN, modeloN, colorN);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new DomainException("La descripción es obligatoria para esta categoría.");

            Descripcion = descripcion.Trim();
        }

        Marca        = marcaN;
        Modelo       = modeloN;
        Color        = colorN;
        ControlStock = controlStock;
        CategoriaId  = categoriaId;
    }

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim().ToUpperInvariant();
}
