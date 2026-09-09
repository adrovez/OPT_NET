using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.RecetaCristales;
using OPT.Application.Features.RecetaCristales.Commands.Actualizar;
using OPT.Application.Features.RecetaCristales.Commands.Crear;
using OPT.Application.Features.RecetaCristales.Commands.Eliminar;
using OPT.Application.Features.RecetaCristales.Queries.ObtenerPorCliente;
using OPT.Application.Features.RecetaCristales.Queries.ObtenerPorId;

namespace OPT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class RecetaCristalesController(IMediator mediator) : ControllerBase
{
    /// <summary>Historial de recetas de un cliente (más reciente primero).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecetaCristalesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorCliente([FromQuery] Guid clientePublicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerRecetaCristalesPorClienteQuery(clientePublicId), ct));

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(RecetaCristalesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerRecetaCristalesPorPublicIdQuery(publicId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(RecetaCristalesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] CrearRecetaCristalesCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(RecetaCristalesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid publicId, [FromBody] ActualizarRecetaCristalesCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid publicId, CancellationToken ct)
    {
        await mediator.Send(new EliminarRecetaCristalesCommand(publicId), ct);
        return NoContent();
    }
}
