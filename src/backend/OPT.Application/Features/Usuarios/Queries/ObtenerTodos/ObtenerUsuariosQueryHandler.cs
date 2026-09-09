using MediatR;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Usuarios.Queries.ObtenerTodos;

public sealed class ObtenerUsuariosQueryHandler(IUsuarioRepositorio usuarioRepo)
    : IRequestHandler<ObtenerUsuariosQuery, PagedResult<UsuarioDto>>
{
    public async Task<PagedResult<UsuarioDto>> Handle(ObtenerUsuariosQuery request, CancellationToken ct)
    {
        var (items, total) = await usuarioRepo.BuscarPaginadoAsync(request, ct);

        return PagedResultFactory.Crear(items, total, request, u => new UsuarioDto(
            u.PublicId, u.Rut, u.Nombre, u.Apellido, u.Email,
            u.RolId, u.Rol?.Nombre ?? string.Empty, u.SucursalActivaId, u.Activo));
    }
}
