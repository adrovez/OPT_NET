using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Commands.Desactivar;

public sealed class DesactivarUsuarioCommandHandler(
    IUsuarioRepositorio usuarioRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<DesactivarUsuarioCommand>
{
    public async Task Handle(DesactivarUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await usuarioRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Usuario", request.PublicId);

        usuario.Desactivar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
