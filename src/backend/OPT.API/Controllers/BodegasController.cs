using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.API.Authorization;
using OPT.Application.Features.Bodegas;
using OPT.Application.Features.Bodegas.Commands.Activar;
using OPT.Application.Features.Bodegas.Commands.Actualizar;
using OPT.Application.Features.Bodegas.Commands.Crear;
using OPT.Application.Features.Bodegas.Commands.Desactivar;
using OPT.Application.Features.Bodegas.Commands.Eliminar;
using OPT.Application.Features.Bodegas.Commands.MarcarPrincipal;
using OPT.Application.Features.Bodegas.Queries.ObtenerPorId;
using OPT.Application.Features.Bodegas.Queries.ObtenerTodos;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

/// <summary>
/// Bodegas de una sucursal (ADR 0012). Siempre por <c>PublicId</c>. La autorización por sucursal
/// (BOLA) se valida en cada handler contra la sucursal dueña de la bodega.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class BodegasController(IMediator mediator) : ControllerBase
{
    /// <summary>Listado paginado. Query params: sucursalId, soloActivas, pagina, tamanioPagina, busqueda, ordenarPor, direccionOrden.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BodegaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ObtenerBodegasQuery query, CancellationToken ct = default)
        => Ok(await mediator.Send(query, ct));

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(BodegaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerBodegaPorPublicIdQuery(publicId), ct));

    [HttpPost]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal)]
    [ProducesResponseType(typeof(BodegaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Crear([FromBody] CrearBodegaCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));

    [HttpPut("{publicId:guid}")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal)]
    [ProducesResponseType(typeof(BodegaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid publicId, [FromBody] ActualizarBodegaCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    /// <summary>Designa esta bodega como la principal de su sucursal (la anterior deja de serlo).</summary>
    [HttpPost("{publicId:guid}/principal")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal)]
    [ProducesResponseType(typeof(BodegaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> MarcarPrincipal(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new MarcarBodegaPrincipalCommand(publicId), ct));

    /// <summary>Desactiva la bodega. 422 si es la principal o si tiene stock.</summary>
    [HttpPost("{publicId:guid}/desactivar")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal)]
    [ProducesResponseType(typeof(BodegaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Desactivar(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new DesactivarBodegaCommand(publicId), ct));

    [HttpPost("{publicId:guid}/activar")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal)]
    [ProducesResponseType(typeof(BodegaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ActivarBodegaCommand(publicId), ct));

    /// <summary>Borrado lógico. 422 si es la principal o si tiene stock.</summary>
    [HttpDelete("{publicId:guid}")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Eliminar(Guid publicId, CancellationToken ct)
    {
        await mediator.Send(new EliminarBodegaCommand(publicId), ct);
        return NoContent();
    }
}
