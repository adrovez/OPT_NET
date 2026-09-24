using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Productos.Commands.DarDeBaja;

public sealed class DarDeBajaProductoCommandHandler(
    IProductoRepositorio productoRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<DarDeBajaProductoCommand>
{
    public async Task Handle(DarDeBajaProductoCommand request, CancellationToken ct)
    {
        var producto = await productoRepo.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Producto", request.Id);

        producto.Eliminar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
