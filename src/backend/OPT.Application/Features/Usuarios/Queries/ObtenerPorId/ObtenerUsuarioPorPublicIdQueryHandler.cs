using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Queries.ObtenerPorId;

public sealed class ObtenerUsuarioPorPublicIdQueryHandler(IUsuarioRepositorio usuarioRepo)
    : IRequestHandler<ObtenerUsuarioPorPublicIdQuery, UsuarioDto>
{
    public async Task<UsuarioDto> Handle(ObtenerUsuarioPorPublicIdQuery request, CancellationToken ct)
    {
        var usuario = await usuarioRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Usuario", request.PublicId);

        return new UsuarioDto(usuario.PublicId, usuario.Rut, usuario.Nombre, usuario.Apellido,
            usuario.Email, usuario.RolId, usuario.Rol?.Nombre ?? string.Empty,
            usuario.SucursalActivaId, usuario.Activo);
    }
}
