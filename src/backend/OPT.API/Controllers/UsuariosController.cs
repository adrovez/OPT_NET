using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.API.Authorization;
using OPT.Application.Features.Usuarios;
using OPT.Application.Features.Usuarios.Commands.Activar;
using OPT.Application.Features.Usuarios.Commands.Actualizar;
using OPT.Application.Features.Usuarios.Commands.AsignarSucursal;
using OPT.Application.Features.Usuarios.Commands.CambiarClave;
using OPT.Application.Features.Usuarios.Commands.Crear;
using OPT.Application.Features.Usuarios.Commands.Desactivar;
using OPT.Application.Features.Usuarios.Commands.Eliminar;
using OPT.Application.Features.Usuarios.Commands.QuitarSucursal;
using OPT.Application.Features.Usuarios.Queries.ObtenerPorId;
using OPT.Application.Features.Usuarios.Queries.ObtenerTodos;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

/// <summary>
/// Administración de usuarios — datos de acceso y asignación de sucursal, se restringe
/// completa a Administrador: es la única superficie que crea/modifica credenciales de
/// otros usuarios, no corresponde compartirla con roles operativos.
/// </summary>
[ApiController]
[Authorize]
[AutorizarRoles(RolesOPT.Administrador)]
[Route("api/[controller]")]
public sealed class UsuariosController(IMediator mediator) : ControllerBase
{
    /// <summary>Listado paginado. Query params: pagina, tamanioPagina, busqueda, ordenarPor, direccionOrden.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ObtenerUsuariosQuery query, CancellationToken ct = default)
        => Ok(await mediator.Send(query, ct));

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerUsuarioPorPublicIdQuery(publicId), ct));

    /// <summary>El usuario creado queda sin sucursal asignada — usar POST .../sucursales/{sucursalId} para poder iniciar sesión.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid publicId, [FromBody] ActualizarUsuarioCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PublicId = publicId }, ct));

    [HttpPut("{publicId:guid}/clave")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarClave(Guid publicId, [FromBody] CambiarClaveUsuarioCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { PublicId = publicId }, ct);
        return NoContent();
    }

    [HttpPost("{publicId:guid}/activar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(Guid publicId, CancellationToken ct)
    {
        await mediator.Send(new ActivarUsuarioCommand(publicId), ct);
        return NoContent();
    }

    [HttpPost("{publicId:guid}/desactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(Guid publicId, CancellationToken ct)
    {
        await mediator.Send(new DesactivarUsuarioCommand(publicId), ct);
        return NoContent();
    }

    [HttpPost("{publicId:guid}/sucursales/{sucursalId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarSucursal(Guid publicId, int sucursalId, CancellationToken ct)
    {
        await mediator.Send(new AsignarSucursalUsuarioCommand(publicId, sucursalId), ct);
        return NoContent();
    }

    [HttpDelete("{publicId:guid}/sucursales/{sucursalId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QuitarSucursal(Guid publicId, int sucursalId, CancellationToken ct)
    {
        await mediator.Send(new QuitarSucursalUsuarioCommand(publicId, sucursalId), ct);
        return NoContent();
    }

    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid publicId, CancellationToken ct)
    {
        await mediator.Send(new EliminarUsuarioCommand(publicId), ct);
        return NoContent();
    }
}
