using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Documento comercial central del sistema. Es la raíz del agregado Comercial:
/// detalles, abonos, pagos, cuotas y bitácora solo se crean/modifican a través de ella.
///
/// Reglas invariantes (del legacy — preservar y corregir):
///   - <see cref="Precio"/> es la <b>suma de los detalles</b>, no un dato digitado.
///   - <c>TotalAbonado = abonos + pagos</c> y <c>Saldo = Precio - TotalAbonado</c>, recalculados
///     dentro de la misma transacción del movimiento que los origina (ADR 0003 / ADR 0006).
///   - El número visible (<see cref="NumeroOT"/>) se ingresa manualmente, igual que en el legacy
///     (decisión 2026-09-11): único entre OT del mismo año que no estén anuladas — una OT
///     anulada libera su número para reutilizarse. La BD refuerza con un índice único filtrado
///     (no anuladas) como respaldo; la app valida además el año.
///   - Todo cambio de estado queda registrado en <see cref="BitacoraOT"/>.
///   - El estado avanza o retrocede de a un paso; <see cref="EstadosOT.Entregado"/> y
///     <see cref="EstadosOT.Anulado"/> son terminales.
/// </summary>
public class OrdenDeTrabajo : AuditableEntity
{
    /// <summary>
    /// Identificador no enumerable expuesto en API/URLs — nunca el Id interno
    /// (ADR 0004, Ley 21.719: la OT vincula paciente ↔ prestación).
    /// Generado por la base de datos (DEFAULT NEWID()).
    /// </summary>
    public Guid    PublicId     { get; private set; }

    /// <summary>
    /// Número legible comunicado al cliente (aparece en tickets/reportes).
    /// Se ingresa manualmente, igual que en el legacy — no lo genera la BD ni la aplicación.
    /// Es atributo visible distinto del Id interno.
    /// </summary>
    public int     NumeroOT     { get; private set; }
    public int     ClienteId    { get; private set; }
    public int     SucursalId   { get; private set; }
    public int     EstadoOTId   { get; private set; }
    public int?    EmpresaId    { get; private set; }   // Si viene derivado por empresa

    public decimal Precio       { get; private set; }
    public decimal TotalAbonado { get; private set; }

    /// <summary>
    /// Saldo = Precio - TotalAbonado. Se persiste por rendimiento pero solo se actualiza
    /// dentro de la misma transacción que registra un abono/pago (ADR 0003).
    /// Puede quedar negativo: el negocio acepta sobrepago (decisión 2026-08-27).
    /// </summary>
    public decimal Saldo        { get; private set; }

    public string? Observaciones { get; private set; }
    public DateTimeOffset FechaEntrega { get; private set; }

    // ── Campos heredados del legacy (004_comercial_pagos_cuotas.sql) ─────────────
    /// <summary>Persona que retira, cuando no es el propio cliente.</summary>
    public string?   Beneficiario  { get; private set; }
    public DateOnly? FechaAtencion { get; private set; }
    public TimeOnly? HoraEntrega   { get; private set; }

    /// <summary>Cantidad de cuotas del plan de pago vigente (null = pago al contado).</summary>
    public int?      NumeroCuotas  { get; private set; }

    private readonly List<DetalleOT>   _detalles = [];
    private readonly List<Abono>       _abonos   = [];
    private readonly List<Pago>        _pagos    = [];
    private readonly List<Cuota>       _cuotas   = [];
    private readonly List<BitacoraOT>  _bitacora = [];

    public IReadOnlyCollection<DetalleOT>  Detalles  => _detalles.AsReadOnly();
    public IReadOnlyCollection<Abono>      Abonos    => _abonos.AsReadOnly();
    public IReadOnlyCollection<Pago>       Pagos     => _pagos.AsReadOnly();
    public IReadOnlyCollection<Cuota>      Cuotas    => _cuotas.AsReadOnly();
    public IReadOnlyCollection<BitacoraOT> Bitacora  => _bitacora.AsReadOnly();

    public bool EstaAnulada => EstadoOTId == EstadosOT.Anulado;

    protected OrdenDeTrabajo() { }

    public static OrdenDeTrabajo Crear(int numeroOT, int clienteId, int sucursalId,
                                        DateTimeOffset fechaEntrega,
                                        int usuarioId, int? empresaId = null,
                                        string? observaciones = null, string? beneficiario = null,
                                        DateOnly? fechaAtencion = null, TimeOnly? horaEntrega = null)
    {
        if (numeroOT <= 0)
            throw new DomainException("El número de orden debe ser mayor a cero.");

        var ot = new OrdenDeTrabajo
        {
            NumeroOT      = numeroOT,
            ClienteId     = clienteId,
            SucursalId    = sucursalId,
            EstadoOTId    = EstadosOT.Inicial,
            EmpresaId     = empresaId,
            Precio        = 0,
            TotalAbonado  = 0,
            Saldo         = 0,
            FechaEntrega  = fechaEntrega,
            Observaciones = observaciones?.Trim(),
            Beneficiario  = beneficiario?.Trim(),
            FechaAtencion = fechaAtencion,
            HoraEntrega   = horaEntrega
        };
        ot.SetCreacion(usuarioId);
        return ot;
    }

    /// <summary>Datos de cabecera editables mientras la OT no esté anulada.</summary>
    public void Actualizar(DateTimeOffset fechaEntrega, int usuarioId, int? empresaId = null,
                            string? observaciones = null, string? beneficiario = null,
                            DateOnly? fechaAtencion = null, TimeOnly? horaEntrega = null)
    {
        GarantizarModificable();

        FechaEntrega  = fechaEntrega;
        EmpresaId     = empresaId;
        Observaciones = observaciones?.Trim();
        Beneficiario  = beneficiario?.Trim();
        FechaAtencion = fechaAtencion;
        HoraEntrega   = horaEntrega;
        SetModificacion(usuarioId);
    }

    // ── Detalles ─────────────────────────────────────────────────────────────────

    /// <summary>Agrega una línea y recalcula el precio en la misma operación.</summary>
    public DetalleOT AgregarDetalle(int productoId, int cantidad, decimal valorUnitario, int usuarioId)
    {
        GarantizarModificable();

        var detalle = DetalleOT.Crear(Id, productoId, cantidad, valorUnitario, usuarioId);
        _detalles.Add(detalle);
        RecalcularTotales();
        SetModificacion(usuarioId);
        return detalle;
    }

    /// <summary>
    /// Reemplaza el detalle completo (las líneas anteriores quedan con borrado lógico) y
    /// recalcula el precio. Es la operación que usa el PUT de la OT: el detalle se manipula
    /// como parte del agregado, nunca como recurso independiente.
    /// </summary>
    public void ReemplazarDetalles(
        IEnumerable<(int ProductoId, int Cantidad, decimal ValorUnitario, string? Comentario)> lineas,
        int usuarioId)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        GarantizarModificable();

        foreach (var vigente in _detalles.Where(d => !d.Eliminado).ToList())
            vigente.Eliminar(usuarioId);

        foreach (var (productoId, cantidad, valorUnitario, comentario) in lineas)
            _detalles.Add(DetalleOT.Crear(Id, productoId, cantidad, valorUnitario, usuarioId, comentario));

        RecalcularTotales();
        SetModificacion(usuarioId);
    }

    // ── Dinero ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Registra el abono inicial y recalcula el saldo en la misma operación
    /// (debe ejecutarse dentro de la misma transacción de BD — ADR 0003).
    /// El negocio acepta sobrepago: el monto puede superar el saldo pendiente.
    /// </summary>
    public Abono RegistrarAbono(decimal monto, int formaPagoId, int usuarioId,
                                 string? referencia = null)
    {
        GarantizarModificable();
        if (monto <= 0)
            throw new DomainException("El monto del abono debe ser mayor a cero.");

        var abono = Abono.Crear(Id, monto, formaPagoId, usuarioId, referencia);
        _abonos.Add(abono);

        RecalcularTotales();
        SetModificacion(usuarioId);
        return abono;
    }

    /// <summary>
    /// Registra un pago posterior, recalcula el saldo e <b>imputa el monto a las cuotas
    /// pendientes más antiguas</b> — cierra el ciclo que el legacy dejaba abierto
    /// (reglas-negocio-legado.md, Orden de Trabajo). Todo en la misma transacción.
    /// </summary>
    public Pago RegistrarPago(decimal monto, int formaPagoId, int usuarioId,
                               DateTimeOffset? fechaPago = null, string? referencia = null)
    {
        GarantizarModificable();
        if (monto <= 0)
            throw new DomainException("El monto del pago debe ser mayor a cero.");

        var pago = Pago.Crear(Id, monto, formaPagoId, usuarioId, fechaPago, referencia);
        _pagos.Add(pago);

        ImputarACuotas(monto, formaPagoId, usuarioId, pago.FechaPago);

        RecalcularTotales();
        SetModificacion(usuarioId);
        return pago;
    }

    /// <summary>
    /// Marca como PAGADA cada cuota pendiente, de la más antigua a la más nueva, mientras
    /// el monto restante alcance a cubrirla completa. Un pago parcial no marca la cuota
    /// (sí baja el saldo de la OT).
    /// </summary>
    private void ImputarACuotas(decimal monto, int formaPagoId, int usuarioId, DateTimeOffset fechaPago)
    {
        var restante = monto;

        foreach (var cuota in _cuotas.Where(c => !c.Eliminado && c.EstaPendiente)
                                     .OrderBy(c => c.FechaVencimiento)
                                     .ThenBy(c => c.Numero))
        {
            if (restante < cuota.ValorCuota) break;

            cuota.MarcarPagada(usuarioId, formaPagoId, fechaPago);
            restante -= cuota.ValorCuota;
        }
    }

    // ── Plan de cuotas ───────────────────────────────────────────────────────────

    /// <summary>
    /// Genera el plan de cuotas: N cuotas de valor parejo (la diferencia por redondeo se
    /// carga a la última), con vencimiento mensual a partir de
    /// <paramref name="primerVencimiento"/>. Solo se puede generar uno vigente a la vez:
    /// para rehacerlo hay que anular las cuotas pendientes primero.
    /// </summary>
    public IReadOnlyList<Cuota> GenerarPlanCuotas(int numeroCuotas, DateOnly primerVencimiento,
                                                   int usuarioId)
    {
        GarantizarModificable();

        if (numeroCuotas <= 0)
            throw new DomainException("El número de cuotas debe ser mayor a cero.");
        if (Precio <= 0)
            throw new DomainException("No se puede generar un plan de cuotas sobre una OT sin detalle (precio 0).");
        if (_cuotas.Any(c => !c.Eliminado && c.EstadoCuotaId != EstadosCuota.Anulada))
            throw new DomainException("La OT ya tiene un plan de cuotas vigente. Anule las cuotas pendientes antes de generar uno nuevo.");

        var valorCuota   = Math.Round(Precio / numeroCuotas, 2, MidpointRounding.ToZero);
        var acumulado    = valorCuota * (numeroCuotas - 1);
        var ultimoNumero = _cuotas.Count == 0 ? 0 : _cuotas.Max(c => c.Numero);

        var nuevas = new List<Cuota>(numeroCuotas);
        for (var i = 0; i < numeroCuotas; i++)
        {
            var valor = i == numeroCuotas - 1 ? Precio - acumulado : valorCuota;
            var cuota = Cuota.Crear(Id, ultimoNumero + i + 1, valor,
                                     primerVencimiento.AddMonths(i), usuarioId);
            _cuotas.Add(cuota);
            nuevas.Add(cuota);
        }

        NumeroCuotas = numeroCuotas;
        SetModificacion(usuarioId);
        return nuevas;
    }

    /// <summary>Marca una cuota como pagada manualmente (sin registrar un <see cref="Pago"/>).</summary>
    public Cuota PagarCuota(int numero, int usuarioId, int? formaPagoId = null,
                             DateTimeOffset? fechaPago = null)
    {
        GarantizarModificable();

        var cuota = BuscarCuota(numero);
        cuota.MarcarPagada(usuarioId, formaPagoId, fechaPago);
        SetModificacion(usuarioId);
        return cuota;
    }

    public Cuota AnularCuota(int numero, int usuarioId)
    {
        GarantizarModificable();

        var cuota = BuscarCuota(numero);
        cuota.Anular(usuarioId);
        SetModificacion(usuarioId);
        return cuota;
    }

    private Cuota BuscarCuota(int numero)
        => _cuotas.FirstOrDefault(c => !c.Eliminado && c.Numero == numero)
           ?? throw new DomainException($"La OT no tiene una cuota número {numero}.");

    // ── Estados ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Avanza al estado siguiente o retrocede uno solo, registrando la bitácora.
    /// Retroceder exige observación (queda como justificación en <see cref="BitacoraOT"/>).
    /// Para anular la OT use <see cref="Anular"/>.
    /// </summary>
    public BitacoraOT CambiarEstado(int nuevoEstadoId, int usuarioId, string? observacion = null)
    {
        if (nuevoEstadoId == EstadosOT.Anulado)
            throw new DomainException("La anulación de la OT se realiza con la acción Anular, no con un cambio de estado.");
        if (!EstadosOT.PerteneceAlFlujo(nuevoEstadoId))
            throw new DomainException($"El estado {nuevoEstadoId} no pertenece al flujo de una Orden de Trabajo.");
        if (EstadosOT.EsTerminal(EstadoOTId))
            throw new DomainException($"La OT está en un estado terminal ({NombreEstadoTerminal()}) y no admite cambios de estado.");
        if (nuevoEstadoId == EstadoOTId)
            throw new DomainException("La OT ya se encuentra en ese estado.");

        var salto = nuevoEstadoId - EstadoOTId;
        if (salto > 1)
            throw new DomainException("No se pueden saltar etapas: la OT solo avanza al estado siguiente.");
        if (salto < -1)
            throw new DomainException("La OT solo puede retroceder una etapa a la vez.");
        if (salto == -1 && string.IsNullOrWhiteSpace(observacion))
            throw new DomainException("Retroceder el estado de la OT exige una observación que lo justifique.");

        return RegistrarTransicion(nuevoEstadoId, usuarioId, observacion);
    }

    /// <summary>
    /// Anula la OT: estado terminal <see cref="EstadosOT.Anulado"/>, alcanzable desde cualquier
    /// estado excepto <see cref="EstadosOT.Entregado"/>. Exige motivo, que queda en la bitácora.
    /// Reemplaza al SP_OTEliminar del legacy — la OT no desaparece del listado.
    /// </summary>
    public BitacoraOT Anular(string motivo, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("Anular una OT exige indicar el motivo.");
        if (EstaAnulada)
            throw new DomainException("La OT ya está anulada.");
        if (EstadoOTId == EstadosOT.Entregado)
            throw new DomainException("Una OT ya entregada no puede anularse.");

        return RegistrarTransicion(EstadosOT.Anulado, usuarioId, motivo);
    }

    private BitacoraOT RegistrarTransicion(int nuevoEstadoId, int usuarioId, string? observacion)
    {
        var entrada = BitacoraOT.Registrar(Id, EstadoOTId, nuevoEstadoId, usuarioId, observacion);
        _bitacora.Add(entrada);
        EstadoOTId = nuevoEstadoId;
        SetModificacion(usuarioId);
        return entrada;
    }

    private string NombreEstadoTerminal()
        => EstadoOTId == EstadosOT.Anulado ? "ANULADO" : "ENTREGADO";

    // ── Totales ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Recalcula Precio, TotalAbonado y Saldo desde las colecciones vigentes.
    /// Se invoca desde cada operación que los afecta — nunca por separado (ADR 0003).
    /// </summary>
    private void RecalcularTotales()
    {
        Precio       = _detalles.Where(d => !d.Eliminado).Sum(d => d.Total);
        TotalAbonado = _abonos.Where(a => !a.Eliminado).Sum(a => a.Monto)
                     + _pagos .Where(p => !p.Eliminado).Sum(p => p.Monto);
        Saldo        = Precio - TotalAbonado;
    }

    private void GarantizarModificable()
    {
        if (EstaAnulada)
            throw new DomainException("La OT está anulada y no admite modificaciones.");
        if (EstadoOTId == EstadosOT.Entregado)
            throw new DomainException("La OT ya fue entregada y no admite modificaciones.");
    }
}
