using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Productos.Commands.Actualizar;

public sealed class ActualizarProductoCommandHandler(
    IProductoRepositorio productoRepo,
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

        producto.Actualizar(request.Codigo, request.Descripcion, request.ControlStock,
            request.CategoriaId ?? producto.CategoriaId, currentUser.UsuarioId);
        await uow.CommitAsync(ct);

        return new ProductoDto(producto.Id, producto.Codigo, producto.Descripcion,
            producto.ControlStock, producto.CategoriaId);
    }
}
