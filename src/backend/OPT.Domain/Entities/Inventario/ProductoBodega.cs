using OPT.Domain.Common;

namespace OPT.Domain.Entities.Inventario;

/// <summary>
/// Stock de un producto en una bodega específica (ADR 0012). Reemplaza al stock que antes
/// vivía en ProductoSucursal; el precio de venta sigue siendo por sucursal.
/// </summary>
public class ProductoBodega : AuditableEntity
{
    public int ProductoId  { get; private set; }
    public int BodegaId    { get; private set; }
    public int StockActual { get; private set; }
    public int StockMinimo { get; private set; }
    public int StockMaximo { get; private set; }

    protected ProductoBodega() { }

    public static ProductoBodega Crear(int productoId, int bodegaId, int stockMinimo, int stockMaximo, int usuarioId)
    {
        var pb = new ProductoBodega
        {
            ProductoId  = productoId,
            BodegaId    = bodegaId,
            StockActual = 0,
            StockMinimo = stockMinimo,
            StockMaximo = stockMaximo
        };
        pb.SetCreacion(usuarioId);
        return pb;
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

    /// <summary>Ajuste manual (regularización); debe registrarse para trazabilidad.</summary>
    public void Regularizar(int nuevoStock, int usuarioId)
    {
        if (nuevoStock < 0) throw new DomainException("El stock regularizado no puede ser negativo.");
        StockActual = nuevoStock;
        SetModificacion(usuarioId);
    }

    public void DefinirLimites(int stockMinimo, int stockMaximo, int usuarioId)
    {
        if (stockMinimo < 0 || stockMaximo < stockMinimo)
            throw new DomainException("Los límites de stock no son válidos.");
        StockMinimo = stockMinimo;
        StockMaximo = stockMaximo;
        SetModificacion(usuarioId);
    }
}
