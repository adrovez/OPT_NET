namespace OPT.Application.Features.Operativos;

/// <summary>
/// Fila del listado paginado de Operativos. El Operativo se direcciona por <c>PublicId</c>,
/// nunca por el Id interno (mismo criterio que <c>OrdenDeTrabajo</c>, ADR 0004).
/// <see cref="GananciaPagado"/>/<see cref="GananciaVendido"/> son derivadas, no persistidas
/// (punto abierto 8.3 del requerimiento: se muestran ambas en vez de elegir una fórmula única).
/// </summary>
public record OperativoResumenDto(
    Guid    PublicId,
    int     Correlativo,
    string  Nombre,
    Guid    EmpresaPublicId,
    string  EmpresaNombre,
    int     SucursalId,
    string  SucursalNombre,
    int     EstadoOperativoId,
    string  EstadoOperativo,
    DateOnly Fecha,
    decimal MontoTotalVendido,
    decimal MontoTotalPagado,
    decimal MontoTotalGastos,
    decimal GananciaPagado,
    decimal GananciaVendido,
    DateTimeOffset CreadoEn);

/// <summary>
/// Vista completa del Operativo: cabecera + OT asociadas + gastos. Es la respuesta de todo
/// comando del agregado, para que el frontend no tenga que releer después de cada acción.
/// </summary>
public record OperativoDto(
    Guid    PublicId,
    int     Correlativo,
    string  Nombre,
    Guid    EmpresaPublicId,
    string  EmpresaNombre,
    int     SucursalId,
    string  SucursalNombre,
    int     EstadoOperativoId,
    string  EstadoOperativo,
    DateOnly Fecha,
    string? Observacion,
    string? NombreContacto,
    string? MailContacto,
    string? TelefonoContacto,
    decimal MontoTotalVendido,
    decimal MontoTotalPagado,
    decimal MontoTotalGastos,
    decimal GananciaPagado,
    decimal GananciaVendido,
    IReadOnlyList<OperativoOTDto>    Ordenes,
    IReadOnlyList<GastoOperativoDto> Gastos);

/// <summary>
/// OT asociada a un Operativo. Los montos son el snapshot guardado al asociar (o al último
/// refresco vía <c>POST /api/operativos/{publicId}/recalcular-montos</c>), no el valor en vivo
/// de la OT — ver <c>OperativoOT.MontoVendidoSnapshot</c>/<c>MontoPagadoSnapshot</c>.
/// </summary>
public record OperativoOTDto(
    Guid     OrdenPublicId,
    int      NumeroOT,
    Guid     ClientePublicId,
    string   ClienteNombre,
    int      EstadoOTId,
    string   EstadoOT,
    DateOnly? FechaAtencion,
    decimal  MontoVendido,
    decimal  MontoPagado);

public record GastoOperativoDto(
    int            Id,
    decimal        Monto,
    string?        NumeroDocumento,
    string?        Observacion,
    DateTimeOffset FechaRegistro);
