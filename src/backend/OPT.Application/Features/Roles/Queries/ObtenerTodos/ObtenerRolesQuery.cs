using MediatR;
using OPT.Application.Features.Roles;

namespace OPT.Application.Features.Roles.Queries.ObtenerTodos;

public record ObtenerRolesQuery : IRequest<IReadOnlyList<RolDto>>;
