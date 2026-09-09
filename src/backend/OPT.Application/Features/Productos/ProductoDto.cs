namespace OPT.Application.Features.Productos;

/// <summary>
/// Ítem del catálogo de Inventario tal como lo consume el selector de producto del detalle
/// de una Orden de Trabajo. Se expone con el Id interno (igual que Sucursal y los catálogos):
/// no es un dato personal ni sensible, así que queda fuera del alcance del ADR 0004.
/// </summary>
public record ProductoDto(int Id, string Codigo, string Descripcion, bool ControlStock, int CategoriaId);
