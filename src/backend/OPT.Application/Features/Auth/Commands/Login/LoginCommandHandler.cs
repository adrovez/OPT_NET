using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUsuarioRepositorio usuarioRepo,
    IPasswordService    passwordService,
    ITokenService       tokenService,
    IUnitOfWork         uow)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        // 1. Buscar usuario por RUT (convención de negocio — preservar)
        var usuario = await usuarioRepo.ObtenerPorRutConSucursalesAsync(
            request.Rut.Trim().ToUpperInvariant(), ct)
            ?? throw new NotFoundException("Usuario", request.Rut);

        // 2. Verificar clave contra el hash — nunca comparación en texto plano (CLAUDE.md)
        if (!passwordService.Verificar(request.Clave, usuario.ClaveHash))
            throw new NotFoundException("Usuario", request.Rut);   // mismo error para no revelar si el RUT existe

        // 3. Asignar sucursal activa: la primera de la lista (regla preservada del legacy)
        var primeraS = usuario.Sucursales.FirstOrDefault()
            ?? throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Sucursal"] = ["El usuario no tiene sucursales asignadas."]
            });

        usuario.SetSucursalActiva(primeraS.SucursalId);
        await uow.CommitAsync(ct);

        // 4. Emitir JWT
        var token = tokenService.GenerarToken(usuario);

        return new LoginResult(
            Token: token,
            UsuarioId: usuario.Id,
            NombreCompleto: $"{usuario.Nombre} {usuario.Apellido}",
            SucursalActivaId: primeraS.SucursalId);
    }
}
