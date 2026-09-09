using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.RegistrarAbono;

/// <summary>
/// Registra el abono inicial de la OT y recalcula TotalAbonado/Saldo en la misma transacción.
/// El negocio acepta sobrepago (decisión 2026-08-27): el monto puede superar el saldo.
/// </summary>
public record RegistrarAbonoCommand(
    Guid OrdenPublicId, decimal Monto, int FormaPagoId, string? Referencia = null)
    : IRequest<OrdenDeTrabajoDto>;
