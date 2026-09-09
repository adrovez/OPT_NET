using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Abono inicial recibido al crear una Orden de Trabajo.
///
/// <b>Abono y Pago son dos flujos distintos</b>, no conceptos duplicados: el legacy los
/// registraba en tablas separadas y el análisis de sus datos lo confirmó (ADR 0006, que
/// cierra el [Reconsiderar] de reglas-negocio-legado.md). <see cref="Pago"/> son los cobros
/// posteriores; ambos suman a <see cref="OrdenDeTrabajo.TotalAbonado"/>.
///
/// Solo se crea a través de <see cref="OrdenDeTrabajo.RegistrarAbono"/>, que recalcula el
/// saldo en la misma transacción.
/// </summary>
public class Abono : AuditableEntity
{
    public int     OrdenDeTrabajoId { get; private set; }
    public decimal Monto            { get; private set; }
    public int     FormaPagoId      { get; private set; }

    /// <summary>Número de transacción, voucher u otra referencia externa opcional.</summary>
    public string? Referencia       { get; private set; }

    protected Abono() { }

    internal static Abono Crear(int ordenId, decimal monto, int formaPagoId,
                                 int usuarioId, string? referencia = null)
    {
        var a = new Abono
        {
            OrdenDeTrabajoId = ordenId,
            Monto            = monto,
            FormaPagoId      = formaPagoId,
            Referencia       = referencia?.Trim()
        };
        a.SetCreacion(usuarioId);
        return a;
    }
}
