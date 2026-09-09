using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Commands.Activar;

public sealed class ActivarUsuarioCommandHandler(
    IUsuarioRepositorio usuarioRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<ActivarUsuarioCommand>
{
    public async Task Handle(ActivarUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await usuarioRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Usuario", request.PublicId);

        usuario.Activar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
