using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Entities.Comercial;
using OPT.Domain.Entities.Operativo;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Persistence.Extensions;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class OrdenDeTrabajoRepositorio(AppDbContext context)
    : RepositorioBase<OrdenDeTrabajo>(context), IOrdenDeTrabajoRepositorio
{
    /// <summary>Columnas por las que el listado permite ordenar (lista blanca — el resto se ignora).</summary>
    private static readonly IReadOnlyDictionary<string, Expression<Func<OrdenDeTrabajo, object>>> ColumnasOrden =
        new Dictionary<string, Expression<Func<OrdenDeTrabajo, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["numeroOT"]     = o => o.NumeroOT,
            ["fechaEntrega"] = o => o.FechaEntrega,
            ["precio"]       = o => o.Precio,
            ["saldo"]        = o => o.Saldo,
            ["estado"]       = o => o.EstadoOTId,
            ["creadoEn"]     = o => o.CreadoEn,
        };

    public async Task<OrdenDeTrabajo?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(o => o.PublicId == publicId, ct);

    public async Task<OrdenDeTrabajo?> ObtenerCompletaPorPublicIdAsync(
        Guid publicId, CancellationToken ct = default)
        => await Activos
            .Include(o => o.Detalles)
            .Include(o => o.Abonos)
            .Include(o => o.Pagos)
            .Include(o => o.Cuotas)
            .Include(o => o.Bitacora)
            .FirstOrDefaultAsync(o => o.PublicId == publicId, ct);

    public async Task<OrdenDeTrabajo?> ObtenerConDetallesAsync(int id, CancellationToken ct = default)
        => await Activos
            .Include(o => o.Detalles)
            .Include(o => o.Abonos)
            .Include(o => o.Pagos)
            .Include(o => o.Cuotas)
            .Include(o => o.Bitacora)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<OrdenDeTrabajo>> ObtenerPorClienteAsync(
        int clienteId, CancellationToken ct = default)
        => await Activos
            .Where(o => o.ClienteId == clienteId)
            .OrderByDescending(o => o.CreadoEn)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<OrdenDeTrabajo>> ObtenerPorSucursalAsync(
        int sucursalId, int? estadoId = null, CancellationToken ct = default)
    {
        var q = Activos.Where(o => o.SucursalId == sucursalId);
        if (estadoId.HasValue) q = q.Where(o => o.EstadoOTId == estadoId.Value);
        return await q.OrderByDescending(o => o.CreadoEn).ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<OrdenDeTrabajo> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros,
        int? clienteId = null,
        int? sucursalId = null,
        int? estadoOTId = null,
        bool? soloConSaldo = null,
        int? empresaId = null,
        int? operativoId = null,
        bool? soloSucursal = null,
        CancellationToken ct = default)
    {
        var query = Activos;

        if (clienteId.HasValue)  query = query.Where(o => o.ClienteId  == clienteId.Value);
        if (sucursalId.HasValue) query = query.Where(o => o.SucursalId == sucursalId.Value);
        if (estadoOTId.HasValue) query = query.Where(o => o.EstadoOTId == estadoOTId.Value);
        if (soloConSaldo == true) query = query.Where(o => o.Saldo > 0);
        if (empresaId.HasValue)  query = query.Where(o => o.EmpresaId == empresaId.Value);

        if (operativoId.HasValue)
        {
            var ordenesDelOperativo = Contexto.Set<OperativoOT>()
                .Where(r => r.OperativoId == operativoId.Value)
                .Select(r => r.OrdenDeTrabajoId);
            query = query.Where(o => ordenesDelOperativo.Contains(o.Id));
        }

        if (soloSucursal == true)
        {
            var ordenesConOperativo = Contexto.Set<OperativoOT>().Select(r => r.OrdenDeTrabajoId);
            query = query.Where(o => !ordenesConOperativo.Contains(o.Id));
        }

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            // El término se prueba como número de OT y, en paralelo, contra los datos del
            // cliente: es el mismo campo de búsqueda único que usan los demás listados.
            var numeroOT = int.TryParse(busqueda, out var n) ? n : (int?)null;

            var clientesQueCalzan = Contexto.Set<Cliente>()
                .Where(c => !c.Eliminado &&
                            (c.Rut.Contains(busqueda) ||
                             c.Nombre.Contains(busqueda) ||
                             c.Apellido.Contains(busqueda)))
                .Select(c => c.Id);

            query = query.Where(o =>
                (numeroOT != null && o.NumeroOT == numeroOT) ||
                (o.Beneficiario != null && o.Beneficiario.Contains(busqueda)) ||
                clientesQueCalzan.Contains(o.ClienteId));
        }

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, o => o.NumeroOT);

        return await query.PaginarAsync(parametros, ct);
    }

    public async Task<bool> ExisteNumeroOTVigenteAsync(
        int numeroOT, int anio, CancellationToken ct = default)
        => await Activos.AnyAsync(o =>
            o.NumeroOT == numeroOT &&
            o.CreadoEn.Year == anio &&
            o.EstadoOTId != EstadosOT.Anulado, ct);

    public async Task<IReadOnlyList<ResumenDeudaEmpresa>> ObtenerDeudaPorEmpresaAsync(
        CancellationToken ct = default)
        => await Activos
            .Where(o => o.Saldo > 0 && o.EstadoOTId != EstadosOT.Anulado)
            .GroupBy(o => o.EmpresaId)
            .Select(g => new ResumenDeudaEmpresa(
                g.Key,
                g.Count(),
                g.Sum(o => o.Precio),
                g.Sum(o => o.TotalAbonado),
                g.Sum(o => o.Saldo)))
            .ToListAsync(ct);
}
