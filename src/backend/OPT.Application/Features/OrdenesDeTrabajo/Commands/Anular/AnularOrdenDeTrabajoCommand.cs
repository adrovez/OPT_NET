using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Anular;

/// <summary>
/// Anula la OT llevándola al estado terminal ANULADO, con el motivo registrado en la bitácora.
/// Reemplaza al SP_OTEliminar del legacy: la OT no desaparece del listado ni se borra
/// lógicamente — queda visible con su historial completo.
/// </summary>
public record AnularOrdenDeTrabajoCommand(Guid PublicId, string Motivo) : IRequest<OrdenDeTrabajoDto>;
