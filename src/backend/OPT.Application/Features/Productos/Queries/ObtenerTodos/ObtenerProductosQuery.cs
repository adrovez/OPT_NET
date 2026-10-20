using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.Productos.Queries.ObtenerTodos;

/// <summary>
/// Listado paginado del catálogo de productos (4.018 filas migradas — nunca lista completa).
/// Hereda <see cref="ParametrosPaginacion"/>: pagina, tamanioPagina, busqueda (código, descripción,
/// marca, modelo o color), ordenarPor (codigo|descripcion|marca|modelo|color) y direccionOrden (asc|desc).
/// <see cref="CategoriaId"/> filtra por categoría (opcional).
/// </summary>
public record ObtenerProductosQuery : ParametrosPaginacion, IRequest<PagedResult<ProductoDto>>
{
    public int? CategoriaId { get; init; }
}
