using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Queries.ObtenerTodos;

public sealed class ObtenerOrdenesDeTrabajoQueryHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    IClienteRepositorio        clienteRepo,
    ISucursalRepositorio       sucursalRepo,
    IEmpresaRepositorio        empresaRepo,
    IEstadoOTRepositorio       estadoRepo)
    : IRequestHandler<ObtenerOrdenesDeTrabajoQuery, PagedResult<OrdenDeTrabajoResumenDto>>
{
    public async Task<PagedResult<OrdenDeTrabajoResumenDto>> Handle(
        ObtenerOrdenesDeTrabajoQuery request, CancellationToken ct)
    {
        int? clienteId = null;
        if (request.ClientePublicId is not null)
        {
            var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.ClientePublicId.Value, ct)
                ?? throw new NotFoundException("Cliente", request.ClientePublicId.Value);
            clienteId = cliente.Id;
        }

        int? empresaId = null;
        if (request.EmpresaPublicId is not null)
        {
            var empresa = await empresaRepo.ObtenerPorPublicIdAsync(request.EmpresaPublicId.Value, ct)
                ?? throw new NotFoundException("Empresa", request.EmpresaPublicId.Value);
            empresaId = empresa.Id;
        }

        var (items, total) = await ordenRepo.BuscarPaginadoAsync(
            request, clienteId, request.SucursalId, request.EstadoOTId, request.SoloConSaldo,
            empresaId, ct);

        // Un lookup por página, no uno por fila.
        var clientes  = (await clienteRepo.ObtenerPorIdsAsync(items.Select(o => o.ClienteId), ct))
                        .ToDictionary(c => c.Id);
        var sucursales = (await sucursalRepo.ObtenerTodosAsync(ct)).ToDictionary(s => s.Id, s => s.Nombre);
        var estados    = (await estadoRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id, e => e.Nombre);

        return PagedResultFactory.Crear(items, total, request, o =>
        {
            clientes.TryGetValue(o.ClienteId, out var cliente);

            return new OrdenDeTrabajoResumenDto(
                o.PublicId, o.NumeroOT,
                cliente?.PublicId ?? Guid.Empty,
                cliente?.Rut ?? string.Empty,
                cliente is null ? string.Empty : $"{cliente.Nombre} {cliente.Apellido}".Trim(),
                o.SucursalId, sucursales.TryGetValue(o.SucursalId, out var s) ? s : string.Empty,
                o.EstadoOTId, estados.TryGetValue(o.EstadoOTId, out var e) ? e : string.Empty,
                o.Precio, o.TotalAbonado, o.Saldo, o.FechaEntrega, o.CreadoEn);
        });
    }
}
