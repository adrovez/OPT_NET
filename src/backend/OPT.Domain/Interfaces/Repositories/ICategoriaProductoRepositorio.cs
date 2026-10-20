using OPT.Domain.Entities.Inventario;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>Catálogo de categorías de Producto (General, Marcos, Cristales, Lentes de Sol, Otros). Solo lectura.</summary>
public interface ICategoriaProductoRepositorio
{
    Task<IReadOnlyList<CategoriaProducto>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(int id, CancellationToken ct = default);
}
