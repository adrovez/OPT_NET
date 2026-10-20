using MediatR;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.CategoriasProducto.Queries.ObtenerTodos;

public sealed class ObtenerCategoriasProductoQueryHandler(ICategoriaProductoRepositorio categoriaRepo)
    : IRequestHandler<ObtenerCategoriasProductoQuery, IReadOnlyList<CategoriaProductoDto>>
{
    public async Task<IReadOnlyList<CategoriaProductoDto>> Handle(
        ObtenerCategoriasProductoQuery request, CancellationToken ct)
    {
        var categorias = await categoriaRepo.ObtenerTodosAsync(ct);
        return categorias.Select(c => new CategoriaProductoDto(c.Id, c.Nombre)).ToList();
    }
}
