using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.API.Authorization;
using OPT.Application.Features.Operativos;
using OPT.Application.Features.Operativos.Commands.Actualizar;
using OPT.Application.Features.Operativos.Commands.Anular;
using OPT.Application.Features.Operativos.Commands.AsociarOrden;
using OPT.Application.Features.Operativos.Commands.CambiarEstado;
using OPT.Application.Features.Operativos.Commands.Crear;
using OPT.Application.Features.Operativos.Commands.EliminarGasto;
using OPT.Application.Features.Operativos.Commands.QuitarOrden;
using OPT.Application.Features.Operativos.Commands.RecalcularMontos;
using OPT.Application.Features.Operativos.Commands.RegistrarGasto;
using OPT.Application.Features.Operativos.Queries.ObtenerPorId;
using OPT.Application.Features.Operativos.Queries.ObtenerReporteCristales;
using OPT.Application.Features.Operativos.Queries.ObtenerTodos;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

/// <summary>
/// Operativos Oftalmológicos en terreno: agrupan las OT de una jornada, sus gastos, y calculan
/// ganancia/pérdida. Ver <c>src/documentos/OPT_Requerimiento_Modulo_Operativo.md</c>.
///
/// El Operativo se direcciona siempre por <c>PublicId</c>, nunca por el Id interno ni por el
/// Correlativo (mismo criterio que la OT, ADR 0004). Las OT asociadas y los gastos son
/// <b>subrecursos</b>: solo se alcanzan bajo la ruta de su Operativo.
///
/// Autorización por rol vía <see cref="AutorizarRolesAttribute"/> por acción: Anular queda
/// reservada a supervisión (mismo criterio que <c>OrdenesDeTrabajoController.Anular</c>) por su
/// impacto en la trazabilidad de la jornada. Control de acceso por sucursal (BOLA/IDOR) vive en
/// los handlers de Application (<c>AutorizacionSucursal</c>), no aquí.
/// </summary>
[ApiController]
[Authorize]
[Route("api/operativos")]
public sealed class OperativosController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Listado paginado. Query params: pagina, tamanioPagina, busqueda (correlativo, nombre u
    /// observación), ordenarPor (correlativo|nombre|fecha|estado|creadoEn), direccionOrden (asc|desc),
    /// y los filtros empresaPublicId, sucursalId, estadoOperativoId.
    /// </summary>
    [HttpGet]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador, RolesOPT.TecnicoMedico)]
    [ProducesResponseType(typeof(PagedResult<OperativoResumenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ObtenerOperativosQuery query,
                                                    CancellationToken ct = default)
        => Ok(await mediator.Send(query, ct));

    /// <summary>Vista completa: cabecera, OT asociadas y gastos.</summary>
    [HttpGet("{publicId:guid}")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador, RolesOPT.TecnicoMedico)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerOperativoPorPublicIdQuery(publicId), ct));

    /// <summary>
    /// HU-OP-10: graduación de las recetas vinculadas a cada OT del Operativo — insumo del
    /// Reporte de Cristales del submenú Recepción (envío a laboratorio).
    /// </summary>
    [HttpGet("{publicId:guid}/reporte-cristales")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador, RolesOPT.TecnicoMedico)]
    [ProducesResponseType(typeof(IReadOnlyList<ReporteCristalesItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReporteCristales(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerReporteCristalesOperativoQuery(publicId), ct));

    [HttpPost]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearOperativoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));

    [HttpPut("{publicId:guid}")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Actualizar(Guid publicId,
        [FromBody] ActualizarOperativoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    // ── Flujo de estados ─────────────────────────────────────────────────────────

    /// <summary>Avanza al estado siguiente (Prospecto → Ingresado → Cobranza → Cerrado). Sin retroceso.</summary>
    [HttpPost("{publicId:guid}/estado")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CambiarEstado(Guid publicId,
        [FromBody] CambiarEstadoOperativoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    /// <summary>Anula el Operativo (solo desde PROSPECTO o INGRESADO, motivo obligatorio).</summary>
    [HttpPost("{publicId:guid}/anular")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Anular(Guid publicId,
        [FromBody] AnularOperativoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    // ── OT asociadas ─────────────────────────────────────────────────────────────

    /// <summary>Asocia una OT existente a este Operativo (debe tener Empresa y no estar ya asociada a otro).</summary>
    [HttpPost("{publicId:guid}/ordenes")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AsociarOrden(Guid publicId,
        [FromBody] AsociarOrdenAOperativoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    /// <summary>Quita una OT del Operativo (no modifica la OT).</summary>
    [HttpDelete("{publicId:guid}/ordenes/{ordenPublicId:guid}")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> QuitarOrden(Guid publicId, Guid ordenPublicId, CancellationToken ct)
        => Ok(await mediator.Send(new QuitarOrdenDeOperativoCommand(publicId, ordenPublicId), ct));

    /// <summary>
    /// Refresca MontoTotalVendido/MontoTotalPagado con el precio/abonado vigentes de cada OT
    /// asociada — usar después de registrar un abono/pago en una OT ya vinculada.
    /// </summary>
    [HttpPost("{publicId:guid}/recalcular-montos")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecalcularMontos(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new RecalcularMontosOperativoCommand(publicId), ct));

    // ── Gastos ───────────────────────────────────────────────────────────────────

    [HttpPost("{publicId:guid}/gastos")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarGasto(Guid publicId,
        [FromBody] RegistrarGastoOperativoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    [HttpDelete("{publicId:guid}/gastos/{gastoId:int}")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                     RolesOPT.Vendedor, RolesOPT.Operador)]
    [ProducesResponseType(typeof(OperativoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> EliminarGasto(Guid publicId, int gastoId, CancellationToken ct)
        => Ok(await mediator.Send(new EliminarGastoOperativoCommand(publicId, gastoId), ct));
}
