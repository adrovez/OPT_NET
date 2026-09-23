using MediatR;

namespace OPT.Application.Features.Operativos.Queries.ObtenerReporteCristales;

/// <summary>
/// HU-OP-10: detalle de graduación de las recetas vinculadas a cada OT de un Operativo, para el
/// submenú Recepción — se envía al laboratorio y sirve de seguimiento de lo que falta fabricar.
/// Reutiliza el vínculo Receta↔OT (script 006) y OT↔Operativo (script 009), ya existentes.
/// </summary>
public record ObtenerReporteCristalesOperativoQuery(Guid PublicId) : IRequest<IReadOnlyList<ReporteCristalesItemDto>>;

/// <summary>Una fila del reporte: la OT y las recetas materializadas en ella (normalmente una).</summary>
public record ReporteCristalesItemDto(
    Guid     OrdenPublicId,
    int      NumeroOT,
    Guid     ClientePublicId,
    string   ClienteNombre,
    int      EstadoOTId,
    string   EstadoOT,
    DateOnly? FechaAtencion,
    IReadOnlyList<RecetaCristales.RecetaCristalesDto> Recetas);
