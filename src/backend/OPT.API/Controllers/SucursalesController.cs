using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.Sucursales;
using OPT.Application.Features.Sucursales.Commands.Actualizar;
using OPT.Application.Features.Sucursales.Commands.Crear;
using OPT.Application.Features.Sucursales.Commands.Eliminar;
using OPT.Application.Features.Sucursales.Queries.ObtenerPorId;
using OPT.Application.Features.Sucursales.Queries.ObtenerTodos;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class SucursalesController(IMediator mediator) : ControllerBase
{
    /// <summary>Listado paginado. Query params: pagina, tamanioPagina, busqueda, ordenarPor, direccionOrden.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SucursalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ObtenerSucursalesQuery query, CancellationToken ct = default)
        => Ok(await mediator.Send(query, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SucursalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerSucursalPorIdQuery(id), ct));

    [HttpPost]
    [ProducesResponseType(typeof(SucursalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearSucursalCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SucursalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarSucursalCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { Id = id }, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await mediator.Send(new EliminarSucursalCommand(id), ct);
        return NoContent();
    }
}
