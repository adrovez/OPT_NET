using OPT.Domain.Common;
using OPT.Domain.Entities.Inventario;

namespace OPT.Domain.Interfaces.Repositories;

public interface IProductoRepositorio : IRepositorioBase<Producto>
{
    Task<Producto?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
    /// <summary>Stock de un producto en una bodega (ADR 0012).</summary>
    Task<ProductoBodega?> ObtenerStockAsync(int productoId, int bodegaId, CancellationToken ct = default);

    /// <summary>Productos con stock bajo el mínimo en alguna bodega activa de la sucursal.</summary>
    Task<IReadOnlyList<Producto>> ObtenerConStockBajoAsync(int sucursalId, CancellationToken ct = default);

    /// <summary>
    /// Listado paginado del catálogo (4.018 productos migrados — nunca lista completa).
    /// <see cref="ParametrosPaginacion.Busqueda"/> hace match (Contains) contra Código y Descripción:
    /// es el selector de producto del detalle de una Orden de Trabajo; también busca en Marca, Modelo y Color.
    /// <paramref name="categoriaId"/> filtra por categoría cuando viene informada.
    /// </summary>
    Task<(IReadOnlyList<Producto> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, int? categoriaId = null, CancellationToken ct = default);
}
