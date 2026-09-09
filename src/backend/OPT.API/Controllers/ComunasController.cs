using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.Comunas;
using OPT.Application.Features.Comunas.Queries.ObtenerPorRegion;

namespace OPT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ComunasController(IMediator mediator) : ControllerBase
{
    /// <summary>Catálogo de comunas de Chile filtrado por región (solo lectura — datos sembrados, sin mutadores).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ComunaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorRegion([FromQuery] int regionId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerComunasPorRegionQuery(regionId), ct));
}
