using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.RegistrarPago;

/// <summary>
/// Registra un cobro posterior al abono inicial. Además de recalcular el saldo, <b>imputa el
/// monto a las cuotas pendientes más antiguas</b> que alcance a cubrir completas — cierra el
/// ciclo que el legacy dejaba abierto (sus 34.110 cuotas quedaron todas en PENDIENTE).
/// </summary>
public record RegistrarPagoCommand(
    Guid OrdenPublicId, decimal Monto, int FormaPagoId,
    DateTimeOffset? FechaPago = null, string? Referencia = null) : IRequest<OrdenDeTrabajoDto>;
