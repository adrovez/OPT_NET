using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Application.Features.RecetaCristales;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Entities.Comercial;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Queries.ObtenerReporteCristales;

public sealed class ObtenerReporteCristalesOperativoQueryHandler(
    IOperativoRepositorio       operativoRepo,
    IOrdenDeTrabajoRepositorio  ordenRepo,
    IEstadoOTRepositorio        estadoOTRepo,
    IClienteRepositorio         clienteRepo,
    IRecetaCristalesRepositorio recetaRepo,
    ICurrentUserService         currentUser)
    : IRequestHandler<ObtenerReporteCristalesOperativoQuery, IReadOnlyList<ReporteCristalesItemDto>>
{
    public async Task<IReadOnlyList<ReporteCristalesItemDto>> Handle(
        ObtenerReporteCristalesOperativoQuery request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        var ordenIds = operativo.Ordenes.Select(r => r.OrdenDeTrabajoId).ToList();
        if (ordenIds.Count == 0) return [];

        var ordenes = (await ordenRepo.BuscarAsync(o => ordenIds.Contains(o.Id), ct))
            .ToDictionary(o => o.Id);

        var estadosOT = (await estadoOTRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id, e => e.Nombre);

        var clienteIds = ordenes.Values.Select(o => o.ClienteId).Distinct().ToList();
        var clientes   = clienteIds.Count == 0
            ? new Dictionary<int, Cliente>()
            : (await clienteRepo.ObtenerPorIdsAsync(clienteIds, ct)).ToDictionary(c => c.Id);

        var recetas = await recetaRepo.ObtenerPorOrdenesAsync(ordenIds, ct);
        var recetasPorOrden = recetas
            .Where(r => r.OrdenDeTrabajoId.HasValue)
            .GroupBy(r => r.OrdenDeTrabajoId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        return ordenIds
            .Select(id =>
            {
                ordenes.TryGetValue(id, out var orden);
                var cliente = orden is null ? null : clientes.GetValueOrDefault(orden.ClienteId);
                var clientePublicId = cliente?.PublicId ?? Guid.Empty;

                var recetasOrden = recetasPorOrden.TryGetValue(id, out var lista)
                    ? lista.Select(r => RecetaCristalesMapper.Mapear(r, clientePublicId)).ToList()
                    : [];

                return new ReporteCristalesItemDto(
                    orden?.PublicId ?? Guid.Empty,
                    orden?.NumeroOT ?? 0,
                    clientePublicId,
                    cliente is null ? string.Empty : $"{cliente.Nombre} {cliente.Apellido}".Trim(),
                    orden?.EstadoOTId ?? 0,
                    orden is null ? string.Empty : Nombre(estadosOT, orden.EstadoOTId),
                    orden?.FechaAtencion,
                    (IReadOnlyList<RecetaCristalesDto>)recetasOrden);
            })
            .OrderBy(x => x.NumeroOT)
            .ToList();
    }

    private static string Nombre(IReadOnlyDictionary<int, string> catalogo, int id)
        => catalogo.TryGetValue(id, out var nombre) ? nombre : string.Empty;
}
