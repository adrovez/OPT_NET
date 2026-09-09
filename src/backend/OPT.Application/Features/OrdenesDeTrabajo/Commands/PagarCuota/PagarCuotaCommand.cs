using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.PagarCuota;

/// <summary>
/// Marca una cuota como PAGADA manualmente, sin registrar un <c>Pago</c> (regularización de
/// cobros hechos fuera del sistema). El flujo normal es registrar el pago: ese imputa las
/// cuotas solo y además mueve el saldo de la OT — este comando no lo hace.
/// </summary>
public record PagarCuotaCommand(
    Guid OrdenPublicId, int Numero, int? FormaPagoId = null, DateTimeOffset? FechaPago = null)
    : IRequest<OrdenDeTrabajoDto>;
