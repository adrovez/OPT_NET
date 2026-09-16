using MediatR;

namespace OPT.Application.Features.Operativos.Commands.Anular;

/// <summary>
/// Anula el Operativo (estado terminal ANULADO). Solo alcanzable desde PROSPECTO o INGRESADO
/// (decisión 2026-09-15) — el dominio rechaza anular uno que ya pasó a COBRANZA. El motivo
/// queda registrado en <c>Observacion</c>: el módulo no tiene bitácora propia.
/// </summary>
public record AnularOperativoCommand(Guid PublicId, string Motivo) : IRequest<OperativoDto>;
