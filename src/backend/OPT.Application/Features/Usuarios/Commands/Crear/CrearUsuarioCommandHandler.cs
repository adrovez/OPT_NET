using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Commands.Crear;

public sealed class CrearUsuarioCommandHandler(
    IUsuarioRepositorio usuarioRepo,
    IRolRepositorio      rolRepo,
    IPasswordService     passwordService,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<CrearUsuarioCommand, UsuarioDto>
{
    public async Task<UsuarioDto> Handle(CrearUsuarioCommand request, CancellationToken ct)
    {
        var errores = new Dictionary<string, string[]>();

        var rol = (await rolRepo.ObtenerTodosAsync(ct)).FirstOrDefault(r => r.Id == request.RolId);
        if (rol is null)
            errores["RolId"] = ["El rol indicado no existe."];

        if (await usuarioRepo.ExisteRutAsync(request.Rut, ct: ct))
            errores["Rut"] = ["Ya existe un usuario con este RUT."];

        if (!string.IsNullOrWhiteSpace(request.Email)
            && await usuarioRepo.ExisteAsync(u => u.Email == request.Email, ct))
            errores["Email"] = ["Ya existe un usuario con este email."];

        if (errores.Count > 0) throw new ValidationException(errores);

        var claveHash = passwordService.Hashear(request.Clave);
        var usuario = Usuario.Crear(
            request.Rut, request.Nombre, request.Apellido, claveHash, request.RolId,
            currentUser.UsuarioId, request.Email);

        usuarioRepo.Agregar(usuario);
        await uow.CommitAsync(ct);

        return new UsuarioDto(usuario.PublicId, usuario.Rut, usuario.Nombre, usuario.Apellido,
            usuario.Email, usuario.RolId, rol!.Nombre, usuario.SucursalActivaId, usuario.Activo);
    }
}
