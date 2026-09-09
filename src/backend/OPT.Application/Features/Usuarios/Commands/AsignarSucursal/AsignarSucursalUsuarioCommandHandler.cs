using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Commands.AsignarSucursal;

public sealed class AsignarSucursalUsuarioCommandHandler(
    IUsuarioRepositorio   usuarioRepo,
    ISucursalRepositorio  sucursalRepo,
    IUnitOfWork           uow)
    : IRequestHandler<AsignarSucursalUsuarioCommand>
{
    public async Task Handle(AsignarSucursalUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await usuarioRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Usuario", request.PublicId);

        if (!await sucursalRepo.ExisteAsync(s => s.Id == request.SucursalId, ct))
            throw new NotFoundException("Sucursal", request.SucursalId);

        usuario.AsignarSucursal(request.SucursalId);
        await uow.CommitAsync(ct);
    }
}
