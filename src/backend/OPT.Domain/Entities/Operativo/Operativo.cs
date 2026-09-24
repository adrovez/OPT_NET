using OPT.Domain.Common;

namespace OPT.Domain.Entities.Operativo;

/// <summary>
/// Operativo Oftalmológico en terreno — agrupa las OT generadas en una jornada de atención en
/// una empresa cliente, sus gastos, y permite calcular ganancia/pérdida de la jornada.
/// Ver <c>src/documentos/OPT_Requerimiento_Modulo_Operativo.md</c>.
///
/// Es la raíz de este agregado: las OT asociadas y los gastos solo se agregan/quitan a través
/// de él. No referencia la entidad <c>OrdenDeTrabajo</c> del módulo Comercial directamente
/// (evita acoplar agregados de módulos distintos) — <see cref="MontoTotalVendido"/> y
/// <see cref="MontoTotalPagado"/> se calculan a partir de los <em>snapshots</em> guardados en
/// cada <see cref="OperativoOT"/>, que la capa de Application refresca pasándole los montos
/// vigentes de la OT (<see cref="RecalcularMontosDesdeOT"/>).
///
/// Reglas invariantes (decididas con el usuario 2026-09-15, ver script <c>009_modulo_operativo.sql</c>):
///   - <see cref="Correlativo"/> es autogenerado por la base de datos (SEQUENCE), no se asigna acá.
///   - Los gastos no se pueden registrar si el Operativo está <see cref="EstadosOperativo.Anulado"/>.
///   - Solo se puede anular desde <see cref="EstadosOperativo.Prospecto"/> o
///     <see cref="EstadosOperativo.Ingresado"/> (punto abierto 8.6 del requerimiento).
///   - El flujo solo avanza de a un estado; no hay retroceso (a diferencia de <c>OrdenDeTrabajo</c>).
/// </summary>
public class Operativo : AuditableEntity
{
    /// <summary>
    /// Identificador no enumerable expuesto en API/URLs — nunca el Id interno (mismo criterio
    /// que <c>OrdenDeTrabajo</c>, ADR 0004). Generado por la base de datos (DEFAULT NEWID()).
    /// </summary>
    public Guid PublicId { get; private set; }

    /// <summary>Número correlativo visible, generado por la base de datos (SEQ_CorrelativoOperativo).</summary>
    public int Correlativo { get; private set; }

    public string   Nombre            { get; private set; } = string.Empty;
    public int    EmpresaId         { get; private set; }
    public int    SucursalId        { get; private set; }
    public int    EstadoOperativoId { get; private set; }
    public DateOnly Fecha           { get; private set; }
    public string?  Observacion     { get; private set; }

    /// <summary>
    /// Datos de contacto de la persona de la Empresa a cargo de esta jornada — propios de este
    /// Operativo, no de la Empresa (HU-OP-01/02: pueden cambiar de una jornada a otra aunque sea
    /// la misma Empresa). Opcionales al crear como Prospecto; se recomienda completarlos antes de
    /// pasar a Ingresado, pero no se fuerza a nivel de dominio.
    /// </summary>
    public string? NombreContacto   { get; private set; }
    public string? MailContacto     { get; private set; }
    public string? TelefonoContacto { get; private set; }

    public decimal MontoTotalVendido { get; private set; }
    public decimal MontoTotalPagado  { get; private set; }
    public decimal MontoTotalGastos  { get; private set; }

    private readonly List<OperativoOT>    _ordenes = [];
    private readonly List<GastoOperativo> _gastos  = [];

    public IReadOnlyCollection<OperativoOT>    Ordenes => _ordenes.AsReadOnly();
    public IReadOnlyCollection<GastoOperativo> Gastos  => _gastos.AsReadOnly();

    public bool EstaAnulado => EstadoOperativoId == EstadosOperativo.Anulado;

    protected Operativo() { }

    public static Operativo Crear(string nombre, int empresaId, int sucursalId, DateOnly fecha,
                                   int usuarioId, string? observacion = null,
                                   string? nombreContacto = null, string? mailContacto = null,
                                   string? telefonoContacto = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del Operativo es obligatorio.");

        var operativo = new Operativo
        {
            Nombre            = nombre.Trim(),
            EmpresaId         = empresaId,
            SucursalId        = sucursalId,
            EstadoOperativoId = EstadosOperativo.Inicial,
            Fecha             = fecha,
            Observacion       = observacion?.Trim(),
            NombreContacto    = nombreContacto?.Trim(),
            MailContacto      = mailContacto?.Trim(),
            TelefonoContacto  = telefonoContacto?.Trim(),
            MontoTotalVendido = 0,
            MontoTotalPagado  = 0,
            MontoTotalGastos  = 0
        };
        operativo.SetCreacion(usuarioId);
        return operativo;
    }

    /// <summary>Datos de cabecera editables mientras el Operativo no esté en un estado terminal.</summary>
    public void Actualizar(string nombre, DateOnly fecha, string? observacion, int usuarioId,
                            string? nombreContacto = null, string? mailContacto = null,
                            string? telefonoContacto = null)
    {
        GarantizarModificable();

        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del Operativo es obligatorio.");

        Nombre           = nombre.Trim();
        Fecha            = fecha;
        Observacion      = observacion?.Trim();
        NombreContacto   = nombreContacto?.Trim();
        MailContacto     = mailContacto?.Trim();
        TelefonoContacto = telefonoContacto?.Trim();
        SetModificacion(usuarioId);
    }

    // ── OT asociadas ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Asocia una OT a este Operativo con el precio/abonado vigentes de esa OT al momento de
    /// asociarla, y recalcula los totales. La unicidad (una OT pertenece a lo sumo un Operativo)
    /// la refuerza el índice de la base de datos — acá solo se evita duplicarla en memoria.
    /// </summary>
    public OperativoOT AsociarOrden(int ordenDeTrabajoId, decimal montoVendido, decimal montoPagado,
                                     int usuarioId)
    {
        GarantizarModificable();

        if (_ordenes.Any(o => o.OrdenDeTrabajoId == ordenDeTrabajoId))
            throw new DomainException("Esa Orden de Trabajo ya está asociada a este Operativo.");

        var relacion = new OperativoOT(Id, ordenDeTrabajoId, montoVendido, montoPagado);
        _ordenes.Add(relacion);

        // La primera OT asociada a un Prospecto lo pasa automáticamente a Ingresado.
        if (EstadoOperativoId == EstadosOperativo.Prospecto)
            EstadoOperativoId = EstadosOperativo.Ingresado;

        RecalcularTotales();
        SetModificacion(usuarioId);
        return relacion;
    }

    /// <summary>Quita una OT del Operativo (no soft-delete: la relación desaparece, la OT no se toca).</summary>
    public void QuitarOrden(int ordenDeTrabajoId, int usuarioId)
    {
        GarantizarModificable();

        var relacion = _ordenes.FirstOrDefault(o => o.OrdenDeTrabajoId == ordenDeTrabajoId)
            ?? throw new DomainException("Esa Orden de Trabajo no está asociada a este Operativo.");

        _ordenes.Remove(relacion);
        RecalcularTotales();
        SetModificacion(usuarioId);
    }

    /// <summary>
    /// Refresca el precio/abonado de cada OT asociada con los valores vigentes que le pasa la
    /// capa de Application (que sí conoce <c>OrdenDeTrabajo</c>) y recalcula los totales. Es la
    /// manera de reflejar abonos/pagos registrados en una OT <b>después</b> de asociarla —
    /// este agregado no los detecta solo, para no acoplarse al agregado Comercial.
    /// </summary>
    public void RecalcularMontosDesdeOT(
        IReadOnlyDictionary<int, (decimal Precio, decimal TotalAbonado)> montosPorOrden, int usuarioId)
    {
        if (EstaAnulado)
            throw new DomainException("El Operativo está anulado y no admite recalcular montos.");

        foreach (var relacion in _ordenes)
        {
            if (montosPorOrden.TryGetValue(relacion.OrdenDeTrabajoId, out var montos))
                relacion.ActualizarSnapshot(montos.Precio, montos.TotalAbonado);
        }

        RecalcularTotales();
        SetModificacion(usuarioId);
    }

    // ── Gastos ───────────────────────────────────────────────────────────────────

    /// <summary>Se puede registrar un gasto en cualquier estado del Operativo, excepto Anulado.</summary>
    public GastoOperativo RegistrarGasto(decimal monto, int usuarioId,
                                          string? numeroDocumento = null, string? observacion = null)
    {
        if (EstaAnulado)
            throw new DomainException("El Operativo está anulado y no admite registrar gastos.");
        if (monto <= 0)
            throw new DomainException("El monto del gasto debe ser mayor a cero.");

        var gasto = GastoOperativo.Crear(Id, monto, usuarioId, numeroDocumento, observacion);
        _gastos.Add(gasto);

        RecalcularTotales();
        SetModificacion(usuarioId);
        return gasto;
    }

    public void EliminarGasto(int gastoId, int usuarioId)
    {
        if (EstaAnulado)
            throw new DomainException("El Operativo está anulado y no admite modificar sus gastos.");

        var gasto = _gastos.FirstOrDefault(g => !g.Eliminado && g.Id == gastoId)
            ?? throw new DomainException("El Operativo no tiene un gasto con ese identificador.");

        gasto.Eliminar(usuarioId);
        RecalcularTotales();
        SetModificacion(usuarioId);
    }

    // ── Estados ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Avanza al estado siguiente del flujo (sin retroceso, a diferencia de la OT: el
    /// requerimiento no lo pide). Saltar etapas o mover un Operativo terminal es un error.
    /// Para anular use <see cref="Anular"/>.
    /// </summary>
    public void CambiarEstado(int nuevoEstadoId, int usuarioId)
    {
        if (nuevoEstadoId == EstadosOperativo.Anulado)
            throw new DomainException("La anulación del Operativo se realiza con la acción Anular, no con un cambio de estado.");
        if (!EstadosOperativo.PerteneceAlFlujo(nuevoEstadoId))
            throw new DomainException($"El estado {nuevoEstadoId} no pertenece al flujo de un Operativo.");
        if (EstadosOperativo.EsTerminal(EstadoOperativoId))
            throw new DomainException($"El Operativo está en un estado terminal ({NombreEstadoTerminal()}) y no admite cambios de estado.");
        if (nuevoEstadoId != EstadoOperativoId + 1)
            throw new DomainException("No se pueden saltar etapas: el Operativo solo avanza al estado siguiente.");

        EstadoOperativoId = nuevoEstadoId;
        SetModificacion(usuarioId);
    }

    /// <summary>
    /// Anula el Operativo (estado terminal). Solo alcanzable desde Prospecto o Ingresado
    /// (decisión 2026-09-15) — una vez en Cobranza ya no se puede anular, solo Cerrar.
    /// El motivo se deja registrado en <see cref="Observacion"/>: el módulo no tiene una
    /// bitácora propia (a diferencia de <c>BitacoraOT</c>), fuera de alcance del esquema actual.
    /// </summary>
    public void Anular(string motivo, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("Anular un Operativo exige indicar el motivo.");
        if (EstaAnulado)
            throw new DomainException("El Operativo ya está anulado.");
        if (!EstadosOperativo.PuedeAnularseDesde(EstadoOperativoId))
            throw new DomainException("El Operativo ya pasó a Cobranza y no puede anularse; solo puede Cerrarse.");

        Observacion = string.IsNullOrWhiteSpace(Observacion)
            ? $"ANULADO: {motivo.Trim()}"
            : $"{Observacion} | ANULADO: {motivo.Trim()}";
        EstadoOperativoId = EstadosOperativo.Anulado;
        SetModificacion(usuarioId);
    }

    private string NombreEstadoTerminal()
        => EstadoOperativoId == EstadosOperativo.Anulado ? "ANULADO" : "CERRADO";

    // ── Totales ──────────────────────────────────────────────────────────────────

    private void RecalcularTotales()
    {
        MontoTotalVendido = _ordenes.Sum(o => o.MontoVendidoSnapshot);
        MontoTotalPagado  = _ordenes.Sum(o => o.MontoPagadoSnapshot);
        MontoTotalGastos  = _gastos.Where(g => !g.Eliminado).Sum(g => g.Monto);
    }

    private void GarantizarModificable()
    {
        if (EstadosOperativo.EsTerminal(EstadoOperativoId))
            throw new DomainException($"El Operativo está {NombreEstadoTerminal()} y no admite modificaciones.");
    }
}
