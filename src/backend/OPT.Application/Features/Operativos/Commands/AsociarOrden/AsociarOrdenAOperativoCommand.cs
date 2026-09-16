using MediatR;

namespace OPT.Application.Features.Operativos.Commands.AsociarOrden;

/// <summary>
/// Asocia una OT ya existente a este Operativo, con el precio/abonado vigentes de la OT al
/// momento de asociarla (ver <c>OperativoOT</c>). Regla del requerimiento: la OT debe tener
/// una Empresa asociada, y no puede estar ya asociada a otro Operativo.
/// </summary>
public record AsociarOrdenAOperativoCommand(Guid PublicId, Guid OrdenPublicId) : IRequest<OperativoDto>;
