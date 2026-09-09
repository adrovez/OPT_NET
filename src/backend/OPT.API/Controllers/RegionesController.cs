using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.Regiones;
using OPT.Application.Features.Regiones.Queries.ObtenerTodos;

namespace OPT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class RegionesController(IMediator mediator) : ControllerBase
{
    /// <summary>Catálogo de regiones de Chile (solo lectura — datos sembrados, sin mutadores).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RegionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerRegionesQuery(), ct));
}
