namespace OPT.Domain.Entities.Operativo;

/// <summary>
/// Relación pura Operativo↔OrdenDeTrabajo (1 Operativo → N OT). Sin auditoría ni borrado
/// lógico — mismo patrón que <c>EmpresaSucursal</c>/<c>UsuarioSucursal</c>: se agrega o se
/// quita del todo, no se anula. Una OT pertenece a lo sumo un Operativo (índice único en
/// <see cref="OrdenDeTrabajoId"/>, script <c>009_modulo_operativo.sql</c>).
///
/// Al momento de asociar quedan grabados <see cref="MontoVendidoSnapshot"/> y
/// <see cref="MontoPagadoSnapshot"/> — el precio y lo abonado/pagado de la OT en ese instante.
/// <see cref="Entities.Operativo.Operativo"/> los usa para sus totales agregados; no se leen en
/// vivo desde <c>OrdenDeTrabajo</c> para no acoplar este agregado al de Comercial. Se actualizan
/// solo cuando se recalcula explícitamente (<c>Operativo.RecalcularMontosDesdeOT</c>).
/// </summary>
public class OperativoOT
{
    public int     Id                  { get; private set; }
    public int     OperativoId         { get; private set; }
    public int     OrdenDeTrabajoId    { get; private set; }
    public decimal MontoVendidoSnapshot { get; private set; }
    public decimal MontoPagadoSnapshot  { get; private set; }

    protected OperativoOT() { }

    public OperativoOT(int operativoId, int ordenDeTrabajoId,
                        decimal montoVendidoSnapshot, decimal montoPagadoSnapshot)
    {
        OperativoId          = operativoId;
        OrdenDeTrabajoId     = ordenDeTrabajoId;
        MontoVendidoSnapshot = montoVendidoSnapshot;
        MontoPagadoSnapshot  = montoPagadoSnapshot;
    }

    internal void ActualizarSnapshot(decimal montoVendido, decimal montoPagado)
    {
        MontoVendidoSnapshot = montoVendido;
        MontoPagadoSnapshot  = montoPagado;
    }
}
