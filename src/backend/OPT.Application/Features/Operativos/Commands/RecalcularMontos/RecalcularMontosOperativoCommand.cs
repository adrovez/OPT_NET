using MediatR;

namespace OPT.Application.Features.Operativos.Commands.RecalcularMontos;

/// <summary>
/// Refresca <c>MontoTotalVendido</c>/<c>MontoTotalPagado</c> con el precio/abonado vigentes de
/// cada OT asociada. Necesario porque el snapshot de cada <c>OperativoOT</c> no se actualiza
/// solo cuando se registra un abono/pago posterior sobre una OT ya vinculada (punto abierto
/// 8.1 del requerimiento — ver comentario en <c>009_modulo_operativo.sql</c>).
/// </summary>
public record RecalcularMontosOperativoCommand(Guid PublicId) : IRequest<OperativoDto>;
