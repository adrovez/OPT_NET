using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Commands.Eliminar;

public sealed class EliminarUsuarioCommandHandler(
    IUsuarioRepositorio usuarioRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<EliminarUsuarioCommand>
{
    public async Task Handle(EliminarUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await usuarioRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Usuario", request.PublicId);

        usuario.Eliminar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
