using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Commands.CambiarClave;

public sealed class CambiarClaveUsuarioCommandHandler(
    IUsuarioRepositorio usuarioRepo,
    IPasswordService     passwordService,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<CambiarClaveUsuarioCommand>
{
    public async Task Handle(CambiarClaveUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await usuarioRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Usuario", request.PublicId);

        if (!passwordService.Verificar(request.ClaveActual, usuario.ClaveHash))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["ClaveActual"] = ["La clave actual no es correcta."]
            });

        var nuevoHash = passwordService.Hashear(request.ClaveNueva);
        usuario.ActualizarClave(nuevoHash, currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
