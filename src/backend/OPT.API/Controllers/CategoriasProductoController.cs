using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.CategoriasProducto;
using OPT.Application.Features.CategoriasProducto.Queries.ObtenerTodos;

namespace OPT.API.Controllers;

/// <summary>Catálogo de categorías de producto: sembrado y de solo lectura (ADR 0013).</summary>
[ApiController]
[Authorize]
[Route("api/categorias-producto")]
public sealed class CategoriasProductoController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
        => Ok(await mediator.Send(new ObtenerCategoriasProductoQuery(), ct));
}
