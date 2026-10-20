using MediatR;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Productos.Queries.ObtenerTodos;

public sealed class ObtenerProductosQueryHandler(IProductoRepositorio productoRepo)
    : IRequestHandler<ObtenerProductosQuery, PagedResult<ProductoDto>>
{
    public async Task<PagedResult<ProductoDto>> Handle(
        ObtenerProductosQuery request, CancellationToken ct)
    {
        var (items, total) = await productoRepo.BuscarPaginadoAsync(request, request.CategoriaId, ct);

        return PagedResultFactory.Crear(items, total, request, ProductoDtoFactory.Crear);
    }
}
