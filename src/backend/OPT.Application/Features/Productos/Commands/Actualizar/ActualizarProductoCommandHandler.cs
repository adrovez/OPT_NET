using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Productos.Commands.Actualizar;

public sealed class ActualizarProductoCommandHandler(
    IProductoRepositorio productoRepo,
    ICategoriaProductoRepositorio categoriaRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<ActualizarProductoCommand, ProductoDto>
{
    public async Task<ProductoDto> Handle(ActualizarProductoCommand request, CancellationToken ct)
    {
        var producto = await productoRepo.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Producto", request.Id);

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var otro = await productoRepo.ObtenerPorCodigoAsync(codigo, ct);
        if (otro is not null && otro.Id != producto.Id)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Codigo"] = [$"Ya existe un producto con el código {codigo}."]
            });

        var categoriaId = request.CategoriaId ?? producto.CategoriaId;
        if (!await categoriaRepo.ExisteAsync(categoriaId, ct))
            throw new NotFoundException("CategoriaProducto", categoriaId);

        producto.Actualizar(request.Codigo, request.Descripcion, request.Marca, request.Modelo,
            request.Color, request.ControlStock, categoriaId, currentUser.UsuarioId);
        await uow.CommitAsync(ct);

        return ProductoDtoFactory.Crear(producto);
    }
}
