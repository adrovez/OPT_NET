using MediatR;

namespace OPT.Application.Features.Operativos.Commands.QuitarOrden;

/// <summary>Quita una OT del Operativo. No modifica la OT — solo elimina la relación.</summary>
public record QuitarOrdenDeOperativoCommand(Guid PublicId, Guid OrdenPublicId) : IRequest<OperativoDto>;
