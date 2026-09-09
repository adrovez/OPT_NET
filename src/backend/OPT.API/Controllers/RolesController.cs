using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.Roles;
using OPT.Application.Features.Roles.Queries.ObtenerTodos;

namespace OPT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class RolesController(IMediator mediator) : ControllerBase
{
    /// <summary>Catálogo de roles (solo lectura — no se agregan roles nuevos vía API).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerRolesQuery(), ct));
}
