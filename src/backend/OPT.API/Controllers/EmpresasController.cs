using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.Empresas;
using OPT.Application.Features.Empresas.Commands.Actualizar;
using OPT.Application.Features.Empresas.Commands.Crear;
using OPT.Application.Features.Empresas.Commands.Eliminar;
using OPT.Application.Features.Empresas.Queries.ObtenerPorId;
using OPT.Application.Features.Empresas.Queries.ObtenerTodos;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class EmpresasController(IMediator mediator) : ControllerBase
{
    /// <summary>Listado paginado. Query params: pagina, tamanioPagina, busqueda, ordenarPor, direccionOrden.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmpresaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ObtenerEmpresasQuery query, CancellationToken ct = default)
        => Ok(await mediator.Send(query, ct));

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(EmpresaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerEmpresaPorPublicIdQuery(publicId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(EmpresaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearEmpresaCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(EmpresaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid publicId, [FromBody] ActualizarEmpresaCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid publicId, CancellationToken ct)
    {
        await mediator.Send(new EliminarEmpresaCommand(publicId), ct);
        return NoContent();
    }
}
