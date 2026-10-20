using OPT.Domain.Entities.Inventario;

namespace OPT.Application.Features.Productos;

internal static class ProductoDtoFactory
{
    public static ProductoDto Crear(Producto p)
        => new(p.Id, p.Codigo, p.Descripcion, p.ControlStock, p.CategoriaId, p.Marca, p.Modelo, p.Color);
}
