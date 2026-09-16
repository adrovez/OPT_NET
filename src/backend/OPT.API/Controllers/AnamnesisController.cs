using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.API.Authorization;
using OPT.Application.Features.Anamnesis;
using OPT.Application.Features.Anamnesis.Commands.Crear;
using OPT.Application.Features.Anamnesis.Commands.Eliminar;
using OPT.Application.Features.Anamnesis.Queries.ObtenerPorCliente;
using OPT.Application.Features.Anamnesis.Queries.ObtenerPorId;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

/// <summary>Datos clínicos (ADR 0004) — mismo criterio de acceso que Clientes.</summary>
[ApiController]
[Authorize]
[AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor, RolesOPT.JefeSucursal,
                 RolesOPT.Vendedor, RolesOPT.TecnicoMedico, RolesOPT.Operador)]
[Route("api/[controller]")]
public sealed class AnamnesisController(IMediator mediator) : ControllerBase
{
    // Regla de negocio: la anamnesis es inmutable una vez creada — no existe endpoint de actualización.

    /// <summary>Historial de fichas de anamnesis de un cliente (más reciente primero).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AnamnesisDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorCliente([FromQuery] Guid clientePublicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerAnamnesisPorClienteQuery(clientePublicId), ct));

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(AnamnesisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid publicId, CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerAnamnesisPorPublicIdQuery(publicId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(AnamnesisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] CrearAnamnesisCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command, ct));


    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid publicId, CancellationToken ct)
    {
        await mediator.Send(new EliminarAnamnesisCommand(publicId), ct);
        return NoContent();
    }
}
