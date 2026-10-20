using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Persistence.Extensions;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class BodegaRepositorio(AppDbContext context)
    : RepositorioBase<Bodega>(context), IBodegaRepositorio
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Bodega, object>>> ColumnasOrden =
        new Dictionary<string, Expression<Func<Bodega, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["codigo"] = b => b.Codigo,
            ["nombre"] = b => b.Nombre,
        };

    public async Task<Bodega?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(b => b.PublicId == publicId, ct);

    public async Task<Bodega?> ObtenerPrincipalAsync(int sucursalId, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(b => b.SucursalId == sucursalId && b.EsPrincipal, ct);

    public async Task<bool> ExisteCodigoAsync(
        int sucursalId, string codigo, int? excluirId = null, CancellationToken ct = default)
        => await Activos.AnyAsync(
            b => b.SucursalId == sucursalId && b.Codigo == codigo && (excluirId == null || b.Id != excluirId), ct);

    public async Task<bool> SucursalTieneBodegasAsync(int sucursalId, CancellationToken ct = default)
        => await Activos.AnyAsync(b => b.SucursalId == sucursalId, ct);

    public async Task<int> ObtenerStockTotalAsync(int bodegaId, CancellationToken ct = default)
        => await Contexto.Set<ProductoBodega>()
            .Where(pb => !pb.Eliminado && pb.BodegaId == bodegaId)
            .SumAsync(pb => (int?)pb.StockActual, ct) ?? 0;

    public async Task<(IReadOnlyList<Bodega> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, int? sucursalId, bool soloActivas, CancellationToken ct = default)
    {
        var query = Activos;

        if (sucursalId is not null) query = query.Where(b => b.SucursalId == sucursalId);
        if (soloActivas)            query = query.Where(b => b.Activa);

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(b => b.Codigo.Contains(busqueda) || b.Nombre.Contains(busqueda));

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, b => b.Nombre);

        return await query.PaginarAsync(parametros, ct);
    }
}
