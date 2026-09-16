using OPT.Domain.Interfaces.Repositories;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Entities.Comercial;
using EntidadOperativo = OPT.Domain.Entities.Operativo.Operativo;

namespace OPT.Application.Features.Operativos;

/// <summary>
/// Ensambla la vista completa de un Operativo (cabecera + OT asociadas + gastos + nombres de
/// catálogo). Mismo motivo que <c>OrdenDeTrabajoDtoFactory</c>: varios comandos devuelven
/// exactamente esta forma.
///
/// Se registra explícitamente en <c>AddApplication</c> (no lo alcanza el escaneo de MediatR).
/// </summary>
public sealed class OperativoDtoFactory(
    IEmpresaRepositorio         empresaRepo,
    ISucursalRepositorio        sucursalRepo,
    IEstadoOperativoRepositorio estadoOperativoRepo,
    IOrdenDeTrabajoRepositorio  ordenRepo,
    IEstadoOTRepositorio        estadoOTRepo,
    IClienteRepositorio         clienteRepo)
{
    public async Task<OperativoDto> CrearAsync(EntidadOperativo operativo, CancellationToken ct)
    {
        var empresa  = await empresaRepo.ObtenerPorIdAsync(operativo.EmpresaId, ct);
        var sucursal = await sucursalRepo.ObtenerPorIdAsync(operativo.SucursalId, ct);
        var estados  = (await estadoOperativoRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id, e => e.Nombre);

        var relaciones = operativo.Ordenes.ToList();
        var ordenIds   = relaciones.Select(r => r.OrdenDeTrabajoId).ToList();
        var ordenes    = ordenIds.Count == 0
            ? new Dictionary<int, OrdenDeTrabajo>()
            : (await ordenRepo.BuscarAsync(o => ordenIds.Contains(o.Id), ct)).ToDictionary(o => o.Id);

        var estadosOT = (await estadoOTRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id, e => e.Nombre);

        var clienteIds = ordenes.Values.Select(o => o.ClienteId).Distinct().ToList();
        var clientes   = clienteIds.Count == 0
            ? new Dictionary<int, Cliente>()
            : (await clienteRepo.ObtenerPorIdsAsync(clienteIds, ct)).ToDictionary(c => c.Id);

        var gastos = operativo.Gastos.Where(g => !g.Eliminado).OrderBy(g => g.CreadoEn).ToList();

        var gananciaPagado  = operativo.MontoTotalPagado  - operativo.MontoTotalGastos;
        var gananciaVendido = operativo.MontoTotalVendido - operativo.MontoTotalGastos;

        return new OperativoDto(
            operativo.PublicId, operativo.Correlativo,
            empresa?.PublicId ?? Guid.Empty, empresa?.Nombre ?? string.Empty,
            operativo.SucursalId, sucursal?.Nombre ?? string.Empty,
            operativo.EstadoOperativoId, Nombre(estados, operativo.EstadoOperativoId),
            operativo.Fecha, operativo.Observacion,
            operativo.MontoTotalVendido, operativo.MontoTotalPagado, operativo.MontoTotalGastos,
            gananciaPagado, gananciaVendido,

            relaciones.Select(r =>
            {
                ordenes.TryGetValue(r.OrdenDeTrabajoId, out var orden);
                var cliente = orden is null ? null : clientes.GetValueOrDefault(orden.ClienteId);

                return new OperativoOTDto(
                    orden?.PublicId ?? Guid.Empty,
                    orden?.NumeroOT ?? 0,
                    cliente?.PublicId ?? Guid.Empty,
                    cliente is null ? string.Empty : $"{cliente.Nombre} {cliente.Apellido}".Trim(),
                    orden?.EstadoOTId ?? 0,
                    orden is null ? string.Empty : Nombre(estadosOT, orden.EstadoOTId),
                    r.MontoVendidoSnapshot, r.MontoPagadoSnapshot);
            }).ToList(),

            gastos.Select(g => new GastoOperativoDto(g.Id, g.Monto, g.NumeroDocumento, g.Observacion, g.CreadoEn))
                  .ToList());
    }

    private static string Nombre(IReadOnlyDictionary<int, string> catalogo, int id)
        => catalogo.TryGetValue(id, out var nombre) ? nombre : string.Empty;
}
