using MediatR;

namespace OPT.Application.Features.Operativos.Commands.CambiarEstado;

/// <summary>
/// Avanza el Operativo al estado siguiente del flujo (Prospecto → Ingresado → Cobranza →
/// Cerrado). No admite retroceso ni saltos — el dominio rechaza ambos. Para anular use el
/// endpoint de anulación, no este comando.
/// </summary>
public record CambiarEstadoOperativoCommand(Guid PublicId, int NuevoEstadoId) : IRequest<OperativoDto>;
