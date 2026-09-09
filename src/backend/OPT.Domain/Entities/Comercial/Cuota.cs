using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Cuota del plan de pago de una Orden de Trabajo (calendario de vencimientos).
///
/// El legacy generaba el plan al crear la OT y nunca lo actualizaba: sus 34.110 cuotas
/// quedaron todas en PENDIENTE mientras los cobros reales vivían en <see cref="Pago"/>.
/// El sistema nuevo <b>cierra el ciclo</b>: registrar un pago marca las cuotas
/// correspondientes como PAGADA dentro de la misma transacción
/// (ver <see cref="OrdenDeTrabajo.RegistrarPago"/>).
/// </summary>
public class Cuota : AuditableEntity
{
    public int              OrdenDeTrabajoId { get; private set; }

    /// <summary>Correlativo dentro de la OT (1..N). Único por OT.</summary>
    public int              Numero           { get; private set; }
    public decimal          ValorCuota       { get; private set; }
    public DateOnly         FechaVencimiento { get; private set; }
    public DateTimeOffset?  FechaPago        { get; private set; }
    public int?             FormaPagoId      { get; private set; }
    public int              EstadoCuotaId    { get; private set; }

    public bool EstaPendiente => EstadoCuotaId == EstadosCuota.Pendiente;

    protected Cuota() { }

    internal static Cuota Crear(int ordenId, int numero, decimal valor,
                                 DateOnly vencimiento, int usuarioId)
    {
        if (numero <= 0)  throw new DomainException("El número de cuota debe ser mayor a cero.");
        if (valor  <= 0)  throw new DomainException("El valor de la cuota debe ser mayor a cero.");

        var c = new Cuota
        {
            OrdenDeTrabajoId = ordenId,
            Numero           = numero,
            ValorCuota       = valor,
            FechaVencimiento = vencimiento,
            EstadoCuotaId    = EstadosCuota.Pendiente
        };
        c.SetCreacion(usuarioId);
        return c;
    }

    internal void MarcarPagada(int usuarioId, int? formaPagoId = null, DateTimeOffset? fechaPago = null)
    {
        if (EstadoCuotaId == EstadosCuota.Pagada)
            throw new DomainException($"La cuota {Numero} ya está pagada.");
        if (EstadoCuotaId == EstadosCuota.Anulada)
            throw new DomainException($"La cuota {Numero} está anulada y no puede pagarse.");

        EstadoCuotaId = EstadosCuota.Pagada;
        FechaPago     = fechaPago ?? DateTimeOffset.UtcNow;
        FormaPagoId   = formaPagoId;
        SetModificacion(usuarioId);
    }

    internal void Anular(int usuarioId)
    {
        if (EstadoCuotaId == EstadosCuota.Pagada)
            throw new DomainException($"La cuota {Numero} está pagada y no puede anularse.");
        if (EstadoCuotaId == EstadosCuota.Anulada)
            throw new DomainException($"La cuota {Numero} ya está anulada.");

        EstadoCuotaId = EstadosCuota.Anulada;
        SetModificacion(usuarioId);
    }
}
