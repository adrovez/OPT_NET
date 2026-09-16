using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.API.Authorization;
using OPT.Application.Features.Cobranza;
using OPT.Application.Features.Cobranza.Queries.ObtenerDeudores;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

/// <summary>
/// Cobranza — vista consolidada de la deuda vigente. Reemplaza al módulo "Deuda" del legacy
/// (<c>sp_ListaDeudores</c> + su reporte). El detalle de cada deudor no tiene endpoint propio:
/// es el listado de OT filtrado (<c>GET /api/ordenes-de-trabajo?empresaPublicId=…&amp;soloConSaldo=true</c>),
/// para no duplicar la proyección de la OT en dos lugares.
/// Reservado a supervisión (Administrador/Supervisor/Jefe Sucursal) — es información
/// financiera agregada de toda la cartera, no de una sola OT.
/// </summary>
[ApiController]
[Authorize]
[AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal)]
[Route("api/cobranza")]
public sealed class CobranzaController(IMediator mediator) : ControllerBase
{
    /// <summary>Deuda vigente agrupada por empresa convenio, de mayor a menor saldo.</summary>
    [HttpGet("deudores")]
    [ProducesResponseType(typeof(IReadOnlyList<DeudorEmpresaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerDeudores(CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerDeudoresQuery(), ct));
}
