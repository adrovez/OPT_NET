using OPT.Domain.Common;

namespace OPT.Domain.Entities.Inventario;

/// <summary>
/// Catálogo de categorías de Producto (armazón, cristal, accesorio, etc.).
/// Entidad de catálogo estática: no lleva auditoría ni borrado lógico.
/// </summary>
public sealed class CategoriaProducto : CatalogEntity
{
    private CategoriaProducto() { }

    public CategoriaProducto(int id, string nombre) : base(id, nombre) { }
}
