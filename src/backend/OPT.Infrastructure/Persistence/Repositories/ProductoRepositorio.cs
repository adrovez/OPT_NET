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
        };

    public async Task<Producto?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(p => p.Codigo == codigo, ct);

    public async Task<ProductoSucursal?> ObtenerStockAsync(
        int productoId, int sucursalId, CancellationToken ct = default)
        => await Contexto.Set<ProductoSucursal>()
            .Where(ps => !ps.Eliminado)
            .FirstOrDefaultAsync(ps => ps.ProductoId == productoId && ps.SucursalId == sucursalId, ct);

    public async Task<IReadOnlyList<Producto>> ObtenerConStockBajoAsync(
        int sucursalId, CancellationToken ct = default)
    {
        var productoIds = Contexto.Set<ProductoSucursal>()
            .Where(ps => !ps.Eliminado
                         && ps.SucursalId == sucursalId
                         && ps.StockActual <= ps.StockMinimo)
            .Select(ps => ps.ProductoId);

        return await Activos
            .Where(p => p.ControlStock && productoIds.Contains(p.Id))
            .OrderBy(p => p.Codigo)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Producto> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default)
    {
        var query = Activos;

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p => p.Codigo.Contains(busqueda) || p.Descripcion.Contains(busqueda));

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, p => p.Descripcion);

        return await query.PaginarAsync(parametros, ct);
    }
}
