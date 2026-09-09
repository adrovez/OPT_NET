using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Commands.QuitarSucursal;

public sealed class QuitarSucursalUsuarioCommandHandler(
    IUsuarioRepositorio usuarioRepo,
    IUnitOfWork          uow)
    : IRequestHandler<QuitarSucursalUsuarioCommand>
{
    public async Task Handle(QuitarSucursalUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await usuarioRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Usuario", request.PublicId);

        usuario.QuitarSucursal(request.SucursalId);
        await uow.CommitAsync(ct);
    }
}
