using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Commands.Actualizar;

public sealed class ActualizarUsuarioCommandHandler(
    IUsuarioRepositorio usuarioRepo,
    IRolRepositorio      rolRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<ActualizarUsuarioCommand, UsuarioDto>
{
    public async Task<UsuarioDto> Handle(ActualizarUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await usuarioRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Usuario", request.PublicId);

        var errores = new Dictionary<string, string[]>();

        var rol = (await rolRepo.ObtenerTodosAsync(ct)).FirstOrDefault(r => r.Id == request.RolId);
        if (rol is null)
            errores["RolId"] = ["El rol indicado no existe."];

        if (await usuarioRepo.ExisteRutAsync(request.Rut, excluirId: usuario.Id, ct: ct))
            errores["Rut"] = ["Ya existe otro usuario con este RUT."];

        if (!string.IsNullOrWhiteSpace(request.Email)
            && await usuarioRepo.ExisteAsync(u => u.Email == request.Email && u.Id != usuario.Id, ct))
            errores["Email"] = ["Ya existe otro usuario con este email."];

        if (errores.Count > 0) throw new ValidationException(errores);

        usuario.Actualizar(request.Rut, request.Nombre, request.Apellido, request.Email,
            request.RolId, currentUser.UsuarioId);
        await uow.CommitAsync(ct);

        return new UsuarioDto(usuario.PublicId, usuario.Rut, usuario.Nombre, usuario.Apellido,
            usuario.Email, usuario.RolId, rol!.Nombre, usuario.SucursalActivaId, usuario.Activo);
    }
}
