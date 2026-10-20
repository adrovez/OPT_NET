using MediatR;

namespace OPT.Application.Features.CategoriasProducto.Queries.ObtenerTodos;

/// <summary>Catálogo completo de categorías de producto — sembrado, sin paginar (5 filas).</summary>
public record ObtenerCategoriasProductoQuery : IRequest<IReadOnlyList<CategoriaProductoDto>>;
