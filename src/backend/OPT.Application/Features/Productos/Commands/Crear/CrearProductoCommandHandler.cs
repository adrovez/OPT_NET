using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Productos.Commands.Crear;

public sealed class CrearProductoCommandHandler(
    IProductoRepositorio productoRepo,
    ICategoriaProductoRepositorio categoriaRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<CrearProductoCommand, ProductoDto>
{
    public async Task<ProductoDto> Handle(CrearProductoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await productoRepo.ObtenerPorCodigoAsync(codigo, ct) is not null)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Codigo"] = [$"Ya existe un producto con el código {codigo}."]
            });

        var categoriaId = request.CategoriaId ?? CategoriaProductoIds.General;
        if (!await categoriaRepo.ExisteAsync(categoriaId, ct))
            throw new NotFoundException("CategoriaProducto", categoriaId);

        var producto = Producto.Crear(request.Codigo, request.Descripcion, request.Marca, request.Modelo,
            request.Color, request.ControlStock, categoriaId, currentUser.UsuarioId);

        productoRepo.Agregar(producto);
        await uow.CommitAsync(ct);

        return ProductoDtoFactory.Crear(producto);
    }
}
