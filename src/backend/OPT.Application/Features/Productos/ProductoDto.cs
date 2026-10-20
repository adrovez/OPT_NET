namespace OPT.Application.Features.Productos;

/// <summary>
/// Ítem del catálogo de Inventario tal como lo consume el selector de producto del detalle
/// de una Orden de Trabajo. Se expone con el Id interno (igual que Sucursal y los catálogos):
/// no es un dato personal ni sensible, así que queda fuera del alcance del ADR 0004.
/// Marca/Modelo/Color solo vienen informados en Marcos y Lentes de Sol (ADR 0013); en esas
/// categorías la Descripción es calculada.
/// </summary>
public record ProductoDto(
    int Id, string Codigo, string Descripcion, bool ControlStock, int CategoriaId,
    string? Marca, string? Modelo, string? Color);
