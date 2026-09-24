using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Productos.Commands.Crear;

public sealed class CrearProductoCommandHandler(
    IProductoRepositorio productoRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<CrearProductoCommand, ProductoDto>
{
    /// <summary>Id sembrado de "General" en OPT_CategoriaProducto (script 002).</summary>
    private const int CategoriaPorDefecto = 1;

    public async Task<ProductoDto> Handle(CrearProductoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await productoRepo.ObtenerPorCodigoAsync(codigo, ct) is not null)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Codigo"] = [$"Ya existe un producto con el código {codigo}."]
            });

        var producto = Producto.Crear(request.Codigo, request.Descripcion, request.ControlStock,
            request.CategoriaId ?? CategoriaPorDefecto, currentUser.UsuarioId);

        productoRepo.Agregar(producto);
        await uow.CommitAsync(ct);

        return new ProductoDto(producto.Id, producto.Codigo, producto.Descripcion,
            producto.ControlStock, producto.CategoriaId);
    }
}
