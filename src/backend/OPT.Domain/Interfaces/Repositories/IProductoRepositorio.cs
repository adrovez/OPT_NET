using OPT.Domain.Common;
using OPT.Domain.Entities.Inventario;

namespace OPT.Domain.Interfaces.Repositories;

public interface IProductoRepositorio : IRepositorioBase<Producto>
{
    Task<Producto?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<ProductoSucursal?> ObtenerStockAsync(int productoId, int sucursalId, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> ObtenerConStockBajoAsync(int sucursalId, CancellationToken ct = default);

    /// <summary>
    /// Listado paginado del catálogo (4.018 productos migrados — nunca lista completa).
    /// <see cref="ParametrosPaginacion.Busqueda"/> hace match (Contains) contra Código y Descripción:
    /// es el selector de producto del detalle de una Orden de Trabajo.
    /// </summary>
    Task<(IReadOnlyList<Producto> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default);
}
