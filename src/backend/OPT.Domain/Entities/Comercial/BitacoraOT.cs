using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Historial de cambios de estado de una Orden de Trabajo.
/// Es el único mecanismo de auditoría que el legacy implementaba correctamente
/// — se generaliza aquí y debe extenderse a otros módulos (reglas-negocio-legado.md).
/// </summary>
public class BitacoraOT : AuditableEntity
{
    public int    OrdenDeTrabajoId { get; private set; }
    public int    EstadoAnteriorId { get; private set; }
    public int    EstadoNuevoId    { get; private set; }
    public string? Observacion     { get; private set; }

    protected BitacoraOT() { }

    internal static BitacoraOT Registrar(int ordenId, int estadoAnteriorId,
                                          int estadoNuevoId, int usuarioId,
                                          string? observacion = null)
    {
        var b = new BitacoraOT
        {
            OrdenDeTrabajoId = ordenId,
            EstadoAnteriorId = estadoAnteriorId,
            EstadoNuevoId    = estadoNuevoId,
            Observacion      = observacion?.Trim()
        };
        b.SetCreacion(usuarioId);
        return b;
    }
}
