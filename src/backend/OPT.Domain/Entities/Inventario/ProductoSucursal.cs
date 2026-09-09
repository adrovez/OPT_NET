using OPT.Domain.Common;

namespace OPT.Domain.Entities.Inventario;

/// <summary>
/// Stock de un producto en una sucursal específica.
/// El stock se controla por producto+sucursal de forma independiente (preservar del legacy).
/// </summary>
public class ProductoSucursal : AuditableEntity
{
    public int     ProductoId   { get; private set; }
    public int     SucursalId   { get; private set; }
    public int     StockActual  { get; private set; }
    public int     StockMinimo  { get; private set; }
    public int     StockMaximo  { get; private set; }
    public decimal PrecioVenta  { get; private set; }

    protected ProductoSucursal() { }

    public static ProductoSucursal Crear(int productoId, int sucursalId, decimal precioVenta,
                                          int stockMinimo, int stockMaximo, int usuarioId)
    {
        var ps = new ProductoSucursal
        {
            ProductoId  = productoId,
            SucursalId  = sucursalId,
            StockActual = 0,
            StockMinimo = stockMinimo,
            StockMaximo = stockMaximo,
            PrecioVenta = precioVenta
        };
        ps.SetCreacion(usuarioId);
        return ps;
    }

    /// <summary>Incrementa el stock (ingreso de mercadería).</summary>
    public void Ingresar(int cantidad, int usuarioId)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad de ingreso debe ser mayor a cero.");
        StockActual += cantidad;
        SetModificacion(usuarioId);
    }

    /// <summary>Decrementa el stock (salida por venta o traslado).</summary>
    public void Egresar(int cantidad, int usuarioId)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad de egreso debe ser mayor a cero.");
        if (cantidad > StockActual)
            throw new DomainException($"Stock insuficiente. Disponible: {StockActual}, solicitado: {cantidad}.");
        StockActual -= cantidad;
        SetModificacion(usuarioId);
    }

    /// <summary>
    /// Ajuste manual de inventario (regularización).
    /// Toda regularización debe registrarse en RegularizacionStock (trazabilidad).
    /// </summary>
    public void Regularizar(int nuevoStock, int usuarioId)
    {
        if (nuevoStock < 0) throw new DomainException("El stock regularizado no puede ser negativo.");
        StockActual = nuevoStock;
        SetModificacion(usuarioId);
    }
}
