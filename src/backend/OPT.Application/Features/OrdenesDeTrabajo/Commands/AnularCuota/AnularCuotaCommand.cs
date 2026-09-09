using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.AnularCuota;

/// <summary>
/// Anula una cuota pendiente (estado ANULADA). Es el paso previo para rehacer un plan de
/// cuotas mal generado. Una cuota ya pagada no se puede anular.
/// </summary>
public record AnularCuotaCommand(Guid OrdenPublicId, int Numero) : IRequest<OrdenDeTrabajoDto>;
