using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.EstadosCuota;
using OPT.Application.Features.EstadosCuota.Queries.ObtenerTodos;
using OPT.Application.Features.EstadosOT;
using OPT.Application.Features.EstadosOT.Queries.ObtenerTodos;
using OPT.Application.Features.FormasPago;
using OPT.Application.Features.FormasPago.Queries.ObtenerTodos;

namespace OPT.API.Controllers;

/// <summary>
/// Catálogos del módulo Comercial. Son sembrados y de solo lectura (mismo criterio que Roles,
/// Regiones y Comunas): se exponen con el Id interno porque no son datos personales y el
/// frontend los necesita para poblar selectores y traducir ids a nombres.
/// </summary>
[ApiController]
[Authorize]
[Route("api/estados-ot")]
public sealed class EstadosOTController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EstadoOTDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerEstadosOTQuery(), ct));
}

[ApiController]
[Authorize]
[Route("api/formas-pago")]
public sealed class FormasPagoController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FormaPagoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerFormasPagoQuery(), ct));
}

[ApiController]
[Authorize]
[Route("api/estados-cuota")]
public sealed class EstadosCuotaController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EstadoCuotaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerEstadosCuotaQuery(), ct));
}
