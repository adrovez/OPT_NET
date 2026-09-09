using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Pago posterior al abono inicial de una Orden de Trabajo.
///
/// <b>Abono y Pago son dos flujos distintos</b>, no conceptos duplicados (ADR 0006, confirmado
/// con los datos reales del legacy): <see cref="Abono"/> es lo que se recibe al crear la OT;
/// <c>Pago</c> es cada cobro posterior. Ambos suman a <see cref="OrdenDeTrabajo.TotalAbonado"/>.
///
/// Solo se crea a través de <see cref="OrdenDeTrabajo.RegistrarPago"/>, que recalcula el saldo
/// e imputa las cuotas en la misma transacción.
/// </summary>
public class Pago : AuditableEntity
{
    public int             OrdenDeTrabajoId { get; private set; }
    public DateTimeOffset  FechaPago        { get; private set; }
    public decimal         Monto            { get; private set; }
    public int             FormaPagoId      { get; private set; }

    /// <summary>Número de transacción, voucher u otra referencia externa opcional.</summary>
    public string?         Referencia       { get; private set; }

    protected Pago() { }

    internal static Pago Crear(int ordenId, decimal monto, int formaPagoId, int usuarioId,
                                DateTimeOffset? fechaPago = null, string? referencia = null)
    {
        var p = new Pago
        {
            OrdenDeTrabajoId = ordenId,
            Monto            = monto,
            FormaPagoId      = formaPagoId,
            FechaPago        = fechaPago ?? DateTimeOffset.UtcNow,
            Referencia       = referencia?.Trim()
        };
        p.SetCreacion(usuarioId);
        return p;
    }
}
