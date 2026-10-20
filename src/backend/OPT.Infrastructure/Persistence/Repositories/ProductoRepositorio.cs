using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Persistence.Extensions;

namespace OPT.Infrastructure.Persistence.Repositories;

/// <summary>
/// Acceso a Producto/ProductoSucursal. Se registra ahora porque el detalle de la Orden de
/// Trabajo necesita validar que el producto exista antes de agregar una línea; el módulo
/// Inventario completo (stock, traslados) todavía no tiene casos de uso propios.
/// </summary>
public sealed class ProductoRepositorio(AppDbContext context)
    : RepositorioBase<Producto>(context), IProductoRepositorio
{
    /// <summary>Columnas por las que el listado permite ordenar (lista blanca — el resto se ignora).</summary>
    private static readonly IReadOnlyDictionary<string, Expression<Func<Producto, object>>> ColumnasOrden =
        new Dictionary<string, Expression<Func<Producto, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["codigo"]      = p => p.Codigo,
            ["descripcion"] = p => p.Descripcion,
            ["marca"]       = p => p.Marca!,
            ["modelo"]      = p => p.Modelo!,
            ["color"]       = p => p.Color!,
        };

    public async Task<Producto?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(p => p.Codigo == codigo, ct);

    public async Task<ProductoBodega?> ObtenerStockAsync(
        int productoId, int bodegaId, CancellationToken ct = default)
        => await Contexto.Set<ProductoBodega>()
            .Where(pb => !pb.Eliminado)
            .FirstOrDefaultAsync(pb => pb.ProductoId == productoId && pb.BodegaId == bodegaId, ct);

    public async Task<IReadOnlyList<Producto>> ObtenerConStockBajoAsync(
        int sucursalId, CancellationToken ct = default)
    {
        var bodegaIds = Contexto.Set<Bodega>()
            .Where(b => !b.Eliminado && b.Activa && b.SucursalId == sucursalId)
            .Select(b => b.Id);

        var productoIds = Contexto.Set<ProductoBodega>()
            .Where(pb => !pb.Eliminado
                         && bodegaIds.Contains(pb.BodegaId)
                         && pb.StockActual <= pb.StockMinimo)
            .Select(pb => pb.ProductoId);

        return await Activos
            .Where(p => p.ControlStock && productoIds.Contains(p.Id))
            .OrderBy(p => p.Codigo)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Producto> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, int? categoriaId = null, CancellationToken ct = default)
    {
        var query = Activos;

        if (categoriaId is { } cat)
            query = query.Where(p => p.CategoriaId == cat);

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p => p.Codigo.Contains(busqueda) || p.Descripcion.Contains(busqueda)
                                     || (p.Marca != null && p.Marca.Contains(busqueda))
                                     || (p.Modelo != null && p.Modelo.Contains(busqueda))
                                     || (p.Color != null && p.Color.Contains(busqueda)));

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, p => p.Descripcion);

        return await query.PaginarAsync(parametros, ct);
    }
}
