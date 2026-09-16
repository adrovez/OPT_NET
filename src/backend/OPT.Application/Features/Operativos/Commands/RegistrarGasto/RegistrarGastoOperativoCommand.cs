using MediatR;

namespace OPT.Application.Features.Operativos.Commands.RegistrarGasto;

/// <summary>
/// Registra un gasto de la jornada (arriendo, movilización, insumos…) — solo Monto +
/// N° de documento + Observación, sin fecha propia ni categoría (requerimiento, punto 8.4).
/// Rechazado si el Operativo está ANULADO.
/// </summary>
public record RegistrarGastoOperativoCommand(
    Guid    PublicId,
    decimal Monto,
    string? NumeroDocumento = null,
    string? Observacion     = null) : IRequest<OperativoDto>;
