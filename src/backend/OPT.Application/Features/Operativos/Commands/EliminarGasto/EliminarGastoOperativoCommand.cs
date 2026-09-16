using MediatR;

namespace OPT.Application.Features.Operativos.Commands.EliminarGasto;

/// <summary>Elimina (borrado lógico) un gasto del Operativo. Rechazado si está ANULADO.</summary>
public record EliminarGastoOperativoCommand(Guid PublicId, int GastoId) : IRequest<OperativoDto>;
