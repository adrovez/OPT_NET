using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>Línea de producto dentro de una Orden de Trabajo.</summary>
public class DetalleOT : AuditableEntity
{
    public int     OrdenDeTrabajoId { get; private set; }
    public int     ProductoId       { get; private set; }
    public int     Cantidad         { get; private set; }
    public decimal ValorUnitario    { get; private set; }

    /// <summary>
    /// Anotación libre de la línea (modelo/color del armazón, p. ej. "FORMOSA F4 C2").
    /// Equivale a <c>OPT_OrdenDeTrabajoDetalle.Comentario</c> del legacy, poblado en 11.168
    /// de las 20.573 líneas migradas.
    /// </summary>
    public string? Comentario       { get; private set; }

    public decimal Total            => Cantidad * ValorUnitario;

    protected DetalleOT() { }

    public static DetalleOT Crear(int ordenId, int productoId, int cantidad,
                                   decimal valorUnitario, int usuarioId,
                                   string? comentario = null)
    {
        if (cantidad <= 0)        throw new DomainException("La cantidad debe ser mayor a cero.");
        if (valorUnitario < 0)    throw new DomainException("El valor unitario no puede ser negativo.");

        var d = new DetalleOT
        {
            OrdenDeTrabajoId = ordenId,
            ProductoId       = productoId,
            Cantidad         = cantidad,
            ValorUnitario    = valorUnitario,
            Comentario       = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
        };
        d.SetCreacion(usuarioId);
        return d;
    }
}
