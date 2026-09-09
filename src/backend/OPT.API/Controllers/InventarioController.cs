using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OPT.API.Controllers;

/// <summary>
/// Stub del controlador de Inventario.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class InventarioController(IMediator mediator) : ControllerBase
{
    // TODO — Fase 3: ObtenerProducto, AjustarStock, CrearNotaTraslado
}
