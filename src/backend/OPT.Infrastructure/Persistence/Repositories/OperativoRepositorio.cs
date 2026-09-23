using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;
using OPT.Domain.Entities.Operativo;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Persistence.Extensions;
using EntidadOperativo = OPT.Domain.Entities.Operativo.Operativo;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class OperativoRepositorio(AppDbContext context)
    : RepositorioBase<EntidadOperativo>(context), IOperativoRepositorio
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<EntidadOperativo, object>>> ColumnasOrden =
        new Dictionary<string, Expression<Func<EntidadOperativo, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["correlativo"] = o => o.Correlativo,
            ["nombre"]      = o => o.Nombre,
            ["fecha"]       = o => o.Fecha,
            ["estado"]      = o => o.EstadoOperativoId,
            ["creadoEn"]    = o => o.CreadoEn,
        };

    public async Task<EntidadOperativo?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(o => o.PublicId == publicId, ct);

    public async Task<EntidadOperativo?> ObtenerCompletaPorPublicIdAsync(
        Guid publicId, CancellationToken ct = default)
        => await Activos
            .Include(o => o.Ordenes)
            .Include(o => o.Gastos)
            .FirstOrDefaultAsync(o => o.PublicId == publicId, ct);

    public async Task<bool> OrdenYaAsociadaAsync(int ordenDeTrabajoId, CancellationToken ct = default)
        => await Contexto.Set<OperativoOT>().AnyAsync(r => r.OrdenDeTrabajoId == ordenDeTrabajoId, ct);

    public async Task<IReadOnlyDictionary<int, EntidadOperativo>> ObtenerPorOrdenesDeTrabajoIdsAsync(
        IEnumerable<int> ordenDeTrabajoIds, CancellationToken ct = default)
    {
        var ids = ordenDeTrabajoIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, EntidadOperativo>();

        var relaciones = await Contexto.Set<OperativoOT>()
            .Where(r => ids.Contains(r.OrdenDeTrabajoId))
            .ToListAsync(ct);

        var operativoIds = relaciones.Select(r => r.OperativoId).Distinct().ToList();
        var operativos = (await Activos.Where(o => operativoIds.Contains(o.Id)).ToListAsync(ct))
            .ToDictionary(o => o.Id);

        return relaciones
            .Where(r => operativos.ContainsKey(r.OperativoId))
            .ToDictionary(r => r.OrdenDeTrabajoId, r => operativos[r.OperativoId]);
    }

    public async Task<(IReadOnlyList<EntidadOperativo> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros,
        int? empresaId = null,
        int? sucursalId = null,
        int? estadoOperativoId = null,
        CancellationToken ct = default)
    {
        var query = Activos;

        if (empresaId.HasValue)         query = query.Where(o => o.EmpresaId == empresaId.Value);
        if (sucursalId.HasValue)        query = query.Where(o => o.SucursalId == sucursalId.Value);
        if (estadoOperativoId.HasValue) query = query.Where(o => o.EstadoOperativoId == estadoOperativoId.Value);

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var correlativo = int.TryParse(busqueda, out var n) ? n : (int?)null;

            query = query.Where(o =>
                (correlativo != null && o.Correlativo == correlativo) ||
                o.Nombre.Contains(busqueda) ||
                (o.Observacion != null && o.Observacion.Contains(busqueda)));
        }

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, o => o.Correlativo);

        return await query.PaginarAsync(parametros, ct);
    }
}
