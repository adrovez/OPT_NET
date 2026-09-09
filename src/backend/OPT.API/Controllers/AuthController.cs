using MediatR;
using Microsoft.AspNetCore.Mvc;
using OPT.Application.Features.Auth.Commands.Login;

namespace OPT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>Inicia sesión con RUT y clave. Retorna un token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var resultado = await mediator.Send(command, ct);
        return Ok(resultado);
        // Sin try/catch — ExceptionHandlingMiddleware se encarga (ADR 0001 / CLAUDE.md)
    }
}
