using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.Productos;
using OPT.Application.Features.Productos.Queries.ObtenerTodos;
using OPT.Domain.Common;

namespace OPT.API.Controllers;

/// <summary>
/// Catálogo de productos, de solo lectura. Existe para que el detalle de una Orden de
/// Trabajo pueda elegir el producto por código/descripción en vez de por Id digitado;
/// el módulo Inventario completo (stock, traslados, alta de productos) sigue pendiente
/// en <see cref="InventarioController"/>.
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
}
