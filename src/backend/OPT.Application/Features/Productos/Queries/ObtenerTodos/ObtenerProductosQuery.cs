using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.Productos.Queries.ObtenerTodos;

/// <summary>
/// Listado paginado del catálogo de productos (4.018 filas migradas — nunca lista completa).
/// Hereda <see cref="ParametrosPaginacion"/>: pagina, tamanioPagina, busqueda (código o
/// descripción), ordenarPor (codigo|descripcion) y direccionOrden (asc|desc).
/// </summary>
public record ObtenerProductosQuery : ParametrosPaginacion, IRequest<PagedResult<ProductoDto>>;
