using OPT.Domain.Common;

namespace OPT.Domain.Entities.Inventario;

/// <summary>
/// Precio de venta de un producto en una sucursal. Desde el ADR 0012 el stock ya no vive aquí:
/// se controla por bodega en <see cref="ProductoBodega"/>.
/// </summary>
public class ProductoSucursal : AuditableEntity
{
    public int     ProductoId  { get; private set; }
    public int     SucursalId  { get; private set; }
    public decimal PrecioVenta { get; private set; }

    protected ProductoSucursal() { }

    public static ProductoSucursal Crear(int productoId, int sucursalId, decimal precioVenta, int usuarioId)
    {
        if (precioVenta < 0) throw new DomainException("El precio de venta no puede ser negativo.");
        var ps = new ProductoSucursal
        {
            ProductoId  = productoId,
            SucursalId  = sucursalId,
            PrecioVenta = precioVenta
        };
        ps.SetCreacion(usuarioId);
        return ps;
    }

    public void CambiarPrecio(decimal precioVenta, int usuarioId)
    {
        if (precioVenta < 0) throw new DomainException("El precio de venta no puede ser negativo.");
        PrecioVenta = precioVenta;
        SetModificacion(usuarioId);
    }
}
