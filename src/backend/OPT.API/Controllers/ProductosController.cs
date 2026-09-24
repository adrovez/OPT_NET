using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.API.Authorization;
using OPT.Application.Features.Productos;
using OPT.Application.Features.Productos.Commands.Actualizar;
using OPT.Application.Features.Productos.Commands.Crear;
using OPT.Application.Features.Productos.Commands.DarDeBaja;
using OPT.Application.Features.Productos.Queries.ObtenerTodos;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

/// <summary>
/// Catálogo de productos: listado paginado (también selector del detalle de una OT) y
/// alta / edición / baja lógica. Stock y traslados siguen pendientes en <see cref="InventarioController"/>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/productos")]
public sealed class ProductosController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Listado paginado. Query params: pagina, tamanioPagina, busqueda (código o
    /// descripción), ordenarPor (codigo|descripcion), direccionOrden (asc|desc).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] ObtenerProductosQuery query,
                                                    CancellationToken ct = default)
        => Ok(await mediator.Send(query, ct));

    [HttpPost]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor)]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] CrearProductoCommand command, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await mediator.Send(command, ct));

    [HttpPut("{id:int}")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor)]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProductoCommand command,
                                                 CancellationToken ct)
        => Ok(await mediator.Send(command with { Id = id }, ct));

    [HttpDelete("{id:int}")]
    [AutorizarRoles(RolesOPT.Administrador, RolesOPT.Supervisor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DarDeBaja(int id, CancellationToken ct)
    {
        await mediator.Send(new DarDeBajaProductoCommand(id), ct);
        return NoContent();
    }
}
