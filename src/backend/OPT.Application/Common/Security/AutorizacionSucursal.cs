using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Common;

namespace OPT.Application.Common.Security;

/// <summary>
/// Control de acceso a recursos por sucursal (BOLA/IDOR) — un usuario no puede operar
/// recursos de una sucursal a la que no está asignado, salvo que su rol tenga alcance
/// nacional (<see cref="RolesOPT.AccesoTotalSucursales"/>). Se valida contra la sucursal
/// activa de la sesión (claim <c>sucursalId</c>) y contra todas las asignadas vía
/// UsuarioSucursal (claim <c>sucursales</c>), no solo la activa — un Jefe de Sucursal con
/// más de una sucursal asignada puede operar cualquiera de ellas.
/// </summary>
public static class AutorizacionSucursal
{
    public static void ValidarAcceso(ICurrentUserService currentUser, int sucursalId)
    {
        if (RolesOPT.AccesoTotalSucursales.Contains(currentUser.RolId)) return;
        if (currentUser.SucursalId == sucursalId) return;
        if (currentUser.SucursalesAsignadas.Contains(sucursalId)) return;

        throw new ForbiddenAccessException(
            "No tiene acceso a recursos de esta sucursal.");
    }
}
