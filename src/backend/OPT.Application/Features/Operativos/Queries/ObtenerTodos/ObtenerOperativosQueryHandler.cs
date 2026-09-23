using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Queries.ObtenerTodos;

public sealed class ObtenerOperativosQueryHandler(
    IOperativoRepositorio      operativoRepo,
    IEmpresaRepositorio        empresaRepo,
    ISucursalRepositorio       sucursalRepo,
    IEstadoOperativoRepositorio estadoRepo,
    ICurrentUserService        currentUser)
    : IRequestHandler<ObtenerOperativosQuery, PagedResult<OperativoResumenDto>>
{
    public async Task<PagedResult<OperativoResumenDto>> Handle(
        ObtenerOperativosQuery request, CancellationToken ct)
    {
        // BOLA/IDOR: mismo criterio que el listado de OT — quien no tiene alcance nacional
        // solo puede listar Operativos de una sucursal suya.
        var sucursalIdFiltro = request.SucursalId;
        if (!RolesOPT.AccesoTotalSucursales.Contains(currentUser.RolId))
        {
            if (sucursalIdFiltro is not null)
                AutorizacionSucursal.ValidarAcceso(currentUser, sucursalIdFiltro.Value);
            else
                sucursalIdFiltro = currentUser.SucursalId
                    ?? throw new ForbiddenAccessException(
                        "El usuario no tiene una sucursal activa asignada.");
        }

        int? empresaId = null;
        if (request.EmpresaPublicId is not null)
        {
            var empresa = await empresaRepo.ObtenerPorPublicIdAsync(request.EmpresaPublicId.Value, ct)
                ?? throw new NotFoundException("Empresa", request.EmpresaPublicId.Value);
            empresaId = empresa.Id;
        }

        var (items, total) = await operativoRepo.BuscarPaginadoAsync(
            request, empresaId, sucursalIdFiltro, request.EstadoOperativoId, ct);

        var empresas   = (await empresaRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id);
        var sucursales = (await sucursalRepo.ObtenerTodosAsync(ct)).ToDictionary(s => s.Id, s => s.Nombre);
        var estados    = (await estadoRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id, e => e.Nombre);

        return PagedResultFactory.Crear(items, total, request, o =>
        {
            empresas.TryGetValue(o.EmpresaId, out var empresa);
            var gananciaPagado  = o.MontoTotalPagado  - o.MontoTotalGastos;
            var gananciaVendido = o.MontoTotalVendido - o.MontoTotalGastos;

            return new OperativoResumenDto(
                o.PublicId, o.Correlativo, o.Nombre,
                empresa?.PublicId ?? Guid.Empty, empresa?.Nombre ?? string.Empty,
                o.SucursalId, sucursales.TryGetValue(o.SucursalId, out var s) ? s : string.Empty,
                o.EstadoOperativoId, estados.TryGetValue(o.EstadoOperativoId, out var e) ? e : string.Empty,
                o.Fecha, o.MontoTotalVendido, o.MontoTotalPagado, o.MontoTotalGastos,
                gananciaPagado, gananciaVendido, o.CreadoEn);
        });
    }
}
