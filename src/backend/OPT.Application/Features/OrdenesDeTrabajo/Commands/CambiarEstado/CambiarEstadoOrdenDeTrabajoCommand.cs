using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.CambiarEstado;

/// <summary>
/// Avanza la OT al estado siguiente o la retrocede una etapa (con observación obligatoria).
/// El dominio rechaza saltos de etapa y cualquier cambio sobre un estado terminal.
/// Para anular use el endpoint de anulación, no este comando.
/// </summary>
public record CambiarEstadoOrdenDeTrabajoCommand(
    Guid PublicId, int NuevoEstadoId, string? Observacion = null) : IRequest<OrdenDeTrabajoDto>;
