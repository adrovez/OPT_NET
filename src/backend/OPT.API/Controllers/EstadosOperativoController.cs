using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.EstadosOperativo;
using OPT.Application.Features.EstadosOperativo.Queries.ObtenerTodos;

namespace OPT.API.Controllers;

/// <summary>
/// Catálogo del módulo Operativo. Sembrado y de solo lectura (mismo criterio que
/// <c>EstadosOTController</c>): se expone con el Id interno porque no es un dato personal.
/// </summary>
[ApiController]
[Authorize]
[Route("api/estados-operativo")]
public sealed class EstadosOperativoController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EstadoOperativoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerEstadosOperativoQuery(), ct));
}
