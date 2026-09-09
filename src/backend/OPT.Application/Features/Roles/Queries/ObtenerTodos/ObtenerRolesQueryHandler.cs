using MediatR;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Roles.Queries.ObtenerTodos;

public sealed class ObtenerRolesQueryHandler(IRolRepositorio rolRepo)
    : IRequestHandler<ObtenerRolesQuery, IReadOnlyList<RolDto>>
{
    public async Task<IReadOnlyList<RolDto>> Handle(ObtenerRolesQuery request, CancellationToken ct)
    {
        var roles = await rolRepo.ObtenerTodosAsync(ct);
        return roles.Select(r => new RolDto(r.Id, r.Nombre, r.Descripcion)).ToList();
    }
}
