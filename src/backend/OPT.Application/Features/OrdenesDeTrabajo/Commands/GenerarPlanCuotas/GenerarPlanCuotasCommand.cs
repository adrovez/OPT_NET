using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.GenerarPlanCuotas;

/// <summary>
/// Genera el plan de cuotas de una OT ya creada: N cuotas de valor parejo (la diferencia por
/// redondeo va a la última) con vencimiento mensual desde <see cref="PrimerVencimiento"/>.
/// Solo puede haber un plan vigente: para rehacerlo hay que anular antes las cuotas pendientes.
/// </summary>
public record GenerarPlanCuotasCommand(
    Guid OrdenPublicId, int NumeroCuotas, DateOnly PrimerVencimiento) : IRequest<OrdenDeTrabajoDto>;
