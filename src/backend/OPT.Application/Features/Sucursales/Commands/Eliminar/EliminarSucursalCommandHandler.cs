using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Sucursales.Commands.Eliminar;

public sealed class EliminarSucursalCommandHandler(
    ISucursalRepositorio sucursalRepo,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<EliminarSucursalCommand>
{
    public async Task Handle(EliminarSucursalCommand request, CancellationToken ct)
    {
        var sucursal = await sucursalRepo.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Sucursal", request.Id);

        sucursal.Eliminar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
