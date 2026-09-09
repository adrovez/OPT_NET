using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.OrdenesDeTrabajo;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.Actualizar;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.Anular;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.AnularCuota;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.CambiarEstado;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.Crear;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.GenerarPlanCuotas;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.PagarCuota;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.RegistrarAbono;
using OPT.Application.Features.OrdenesDeTrabajo.Commands.RegistrarPago;
using OPT.Application.Features.OrdenesDeTrabajo.Queries.ObtenerPorId;
using OPT.Application.Features.OrdenesDeTrabajo.Queries.ObtenerTodos;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

/// <summary>
/// Órdenes de Trabajo y su dinero (abonos, pagos, cuotas) y flujo de estados.
///
/// La OT se direcciona siempre por <c>PublicId</c>, nunca por el Id interno ni por el
/// NumeroOT (ADR 0004). Abonos, pagos, cuotas y bitácora son <b>subrecursos</b>: solo se
/// alcanzan bajo la ruta de su OT, que ya está protegida.
/// </summary>
[ApiController]
[Authorize]
[Route("api/ordenes-de-trabajo")]
public sealed class OrdenesDeTrabajoController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Listado paginado. Query params: pagina, tamanioPagina, busqueda (número de OT,
    /// beneficiario o RUT/nombre del cliente), ordenarPor
    /// (numeroOT|fechaEntrega|precio|saldo|estado|creadoEn), direccionOrden (asc|desc),
    /// y los filtros clientePublicId, sucursalId, estadoOTId, soloConSaldo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrdenDeTrabajoResumenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ObtenerOrdenesDeTrabajoQuery query,
                                                    CancellationToken ct = default)
        => Ok(await mediator.Send(query, ct));

    /// <summary>Vista completa: cabecera, detalle, abonos, pagos, cuotas y bitácora de estados.</summary>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerOrdenDeTrabajoPorPublicIdQuery(publicId), ct));

    /// <summary>
    /// Crea la OT con su detalle (el precio se calcula de las líneas) y, si vienen en el
    /// mismo cuerpo, el plan de cuotas y el abono inicial.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearOrdenDeTrabajoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid publicId,
        [FromBody] ActualizarOrdenDeTrabajoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    // ── Flujo de estados ─────────────────────────────────────────────────────────

    /// <summary>
    /// Avanza al estado siguiente o retrocede una etapa (observación obligatoria al retroceder).
    /// Saltar etapas o mover una OT terminal devuelve 422.
    /// </summary>
    [HttpPost("{publicId:guid}/estado")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CambiarEstado(Guid publicId,
        [FromBody] CambiarEstadoOrdenDeTrabajoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    /// <summary>
    /// Anula la OT (estado terminal ANULADO, motivo obligatorio). No borra ni oculta la OT:
    /// queda en el listado con su historial — reemplaza al SP_OTEliminar del legacy.
    /// </summary>
    [HttpPost("{publicId:guid}/anular")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Anular(Guid publicId,
        [FromBody] AnularOrdenDeTrabajoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    // ── Dinero ───────────────────────────────────────────────────────────────────

    /// <summary>Registra el abono inicial y recalcula el saldo en la misma transacción.</summary>
    [HttpPost("{publicId:guid}/abonos")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarAbono(Guid publicId,
        [FromBody] RegistrarAbonoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { OrdenPublicId = publicId }, ct));

    /// <summary>
    /// Registra un cobro posterior: recalcula el saldo e imputa las cuotas pendientes más
    /// antiguas que el monto alcance a cubrir completas.
    /// </summary>
    [HttpPost("{publicId:guid}/pagos")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarPago(Guid publicId,
        [FromBody] RegistrarPagoCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { OrdenPublicId = publicId }, ct));

    // ── Plan de cuotas ───────────────────────────────────────────────────────────

    [HttpPost("{publicId:guid}/cuotas")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GenerarPlanCuotas(Guid publicId,
        [FromBody] GenerarPlanCuotasCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { OrdenPublicId = publicId }, ct));

    /// <summary>
    /// Marca una cuota como pagada manualmente (regularización). El flujo normal es registrar
    /// el pago en <c>POST {publicId}/pagos</c>, que imputa las cuotas solo.
    /// </summary>
    [HttpPost("{publicId:guid}/cuotas/{numero:int}/pagar")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PagarCuota(Guid publicId, int numero,
        [FromBody] PagarCuotaCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { OrdenPublicId = publicId, Numero = numero }, ct));

    [HttpPost("{publicId:guid}/cuotas/{numero:int}/anular")]
    [ProducesResponseType(typeof(OrdenDeTrabajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AnularCuota(Guid publicId, int numero, CancellationToken ct)
        => Ok(await mediator.Send(new AnularCuotaCommand(publicId, numero), ct));
}
