using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Bodegas.Queries.ObtenerTodos;

public sealed class ObtenerBodegasQueryHandler(
    IBodegaRepositorio  bodegaRepo,
    ICurrentUserService currentUser)
    : IRequestHandler<ObtenerBodegasQuery, PagedResult<BodegaDto>>
{
    public async Task<PagedResult<BodegaDto>> Handle(ObtenerBodegasQuery request, CancellationToken ct)
    {
        // BOLA/IDOR: igual criterio que el listado de OT — sin alcance nacional solo se listan
        // bodegas de una sucursal propia; si no se filtra, se restringe a la sucursal activa.
        var sucursalId = request.SucursalId;
        if (!RolesOPT.AccesoTotalSucursales.Contains(currentUser.RolId))
        {
            if (sucursalId is not null)
                AutorizacionSucursal.ValidarAcceso(currentUser, sucursalId.Value);
            else
                sucursalId = currentUser.SucursalId
                    ?? throw new ForbiddenAccessException("El usuario no tiene una sucursal activa asignada.");
        }

        var (items, total) = await bodegaRepo.BuscarPaginadoAsync(request, sucursalId, request.SoloActivas, ct);
        return PagedResultFactory.Crear(items, total, request, BodegaDto.Desde);
    }
}
