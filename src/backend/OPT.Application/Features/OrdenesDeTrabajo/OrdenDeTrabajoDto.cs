using OPT.Application.Features.RecetaCristales;

namespace OPT.Application.Features.OrdenesDeTrabajo;

/// <summary>
/// Fila del listado paginado de Órdenes de Trabajo. La OT se direcciona por
/// <c>PublicId</c>, nunca por el Id interno (ADR 0004). <c>NumeroOT</c> es el correlativo
/// visible que se comunica al cliente — se muestra, pero no se usa como identificador de ruta.
/// </summary>
public record OrdenDeTrabajoResumenDto(
    Guid           PublicId,
    int            NumeroOT,
    Guid           ClientePublicId,
    string         ClienteRut,
    string         ClienteNombre,
    int            SucursalId,
    string         SucursalNombre,
    int            EstadoOTId,
    string         EstadoOT,
    decimal        Precio,
    decimal        TotalAbonado,
    decimal        Saldo,
    DateTimeOffset FechaEntrega,
    DateTimeOffset CreadoEn);

/// <summary>
/// Vista completa de la OT: cabecera + detalle + movimientos de dinero + plan de cuotas +
/// bitácora de estados. Es la respuesta de todo comando del agregado, para que el frontend
/// no tenga que releer después de cada acción.
///
/// Los hijos se identifican por su Id interno: solo son alcanzables anidados bajo esta OT,
/// que ya está protegida por <c>PublicId</c> + autorización (ADR 0004, alcance decidido 2026-08-27).
/// </summary>
public record OrdenDeTrabajoDto(
    Guid           PublicId,
    int            NumeroOT,
    Guid           ClientePublicId,
    string         ClienteRut,
    string         ClienteNombre,
    int            SucursalId,
    string         SucursalNombre,
    Guid?          EmpresaPublicId,
    string?        EmpresaNombre,
    int            EstadoOTId,
    string         EstadoOT,
    decimal        Precio,
    decimal        TotalAbonado,
    decimal        Saldo,
    string?        Observaciones,
    DateTimeOffset FechaEntrega,
    string?        Beneficiario,
    DateOnly?      FechaAtencion,
    TimeOnly?      HoraEntrega,
    int?           NumeroCuotas,
    ClienteOTDto   Cliente,
    IReadOnlyList<RecetaCristalesDto> Recetas,
    IReadOnlyList<DetalleOTDto>  Detalles,
    IReadOnlyList<AbonoDto>      Abonos,
    IReadOnlyList<PagoDto>       Pagos,
    IReadOnlyList<CuotaDto>      Cuotas,
    IReadOnlyList<BitacoraOTDto> Bitacora);

/// <summary>
/// Ficha del cliente tal como la muestra la pestaña Cliente de la OT (equivale a la pestaña
/// del mismo nombre del modal "Detalle Orden de Trabajo" del legacy). Va embebida en la OT y
/// no en una llamada aparte: es una vista de solo lectura del cliente al que pertenece la
/// orden — para editarlo se va a su ficha, que se direcciona por <c>PublicId</c> (ADR 0004).
/// </summary>
public record ClienteOTDto(
    Guid      PublicId,
    string    Rut,
    string    Nombre,
    string?   Email,
    string?   Telefono,
    string?   Direccion,
    int?      ComunaId,
    string?   ComunaNombre,
    string?   RegionNombre,
    DateOnly? FechaNacimiento,
    string?   TipoPrevision);

public record DetalleOTDto(
    int     Id,
    int     ProductoId,
    string? ProductoCodigo,
    string? ProductoDescripcion,
    int     Cantidad,
    decimal ValorUnitario,
    decimal Total,
    string? Comentario);

/// <summary>Abono inicial de la OT. <c>FechaRegistro</c> viene del audit <c>CreadoEn</c>.</summary>
public record AbonoDto(
    int            Id,
    decimal        Monto,
    int            FormaPagoId,
    string         FormaPago,
    string?        Referencia,
    DateTimeOffset FechaRegistro);

/// <summary>Cobro posterior al abono inicial (ADR 0006).</summary>
public record PagoDto(
    int            Id,
    decimal        Monto,
    int            FormaPagoId,
    string         FormaPago,
    string?        Referencia,
    DateTimeOffset FechaPago);

public record CuotaDto(
    int             Id,
    int             Numero,
    decimal         ValorCuota,
    DateOnly        FechaVencimiento,
    DateTimeOffset? FechaPago,
    int?            FormaPagoId,
    string?         FormaPago,
    int             EstadoCuotaId,
    string          EstadoCuota);

public record BitacoraOTDto(
    int            Id,
    int            EstadoAnteriorId,
    string         EstadoAnterior,
    int            EstadoNuevoId,
    string         EstadoNuevo,
    string?        Observacion,
    DateTimeOffset Fecha,
    int            UsuarioId);

/// <summary>Línea de detalle tal como la envía el cliente de la API (alta y edición de la OT).</summary>
public record LineaDetalleOTDto(int ProductoId, int Cantidad, decimal ValorUnitario,
                                 string? Comentario = null);
