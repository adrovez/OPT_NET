using OPT.Domain.Common;

namespace OPT.Domain.Entities.Operativo;

/// <summary>
/// Gasto asociado a la jornada de un Operativo (arriendo, movilización, insumos, etc. — sin
/// categoría ni fecha propia, tal como lo pidió el requerimiento, punto 8.4). Solo se crea a
/// través de <see cref="Entities.Operativo.Operativo.RegistrarGasto"/>, que recalcula
/// <c>MontoTotalGastos</c> en la misma operación.
/// </summary>
public class GastoOperativo : AuditableEntity
{
    public int     OperativoId     { get; private set; }
    public decimal Monto           { get; private set; }

    /// <summary>N° de boleta o factura del gasto.</summary>
    public string? NumeroDocumento { get; private set; }
    public string? Observacion     { get; private set; }

    protected GastoOperativo() { }

    internal static GastoOperativo Crear(int operativoId, decimal monto, int usuarioId,
                                          string? numeroDocumento = null, string? observacion = null)
    {
        var gasto = new GastoOperativo
        {
            OperativoId     = operativoId,
            Monto           = monto,
            NumeroDocumento = numeroDocumento?.Trim(),
            Observacion     = observacion?.Trim()
        };
        gasto.SetCreacion(usuarioId);
        return gasto;
    }
}
