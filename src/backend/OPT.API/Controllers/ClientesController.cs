using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.Clientes;
using OPT.Domain.Common;
using OPT.Application.Features.Clientes.Commands.Actualizar;
using OPT.Application.Features.Clientes.Commands.Crear;
using OPT.Application.Features.Clientes.Commands.Eliminar;
using OPT.Application.Features.Clientes.Queries.ObtenerPorId;
using OPT.Application.Features.Clientes.Queries.ObtenerTodos;

namespace OPT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ClientesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Búsqueda paginada — Cliente tiene ~12.000 filas, no expone un listado completo.
    /// Query params: pagina, tamanioPagina, busqueda (RUT/Nombre/Apellido),
    /// ordenarPor (rut|nombre|apellido|email), direccionOrden (asc|desc).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ObtenerClientesQuery query, CancellationToken ct = default)
        => Ok(await mediator.Send(query, ct));

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerClientePorPublicIdQuery(publicId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearClienteCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid publicId, [FromBody] ActualizarClienteCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid publicId, CancellationToken ct)
    {
        await mediator.Send(new EliminarClienteCommand(publicId), ct);
        return NoContent();
    }
}
