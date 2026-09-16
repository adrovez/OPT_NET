using Microsoft.AspNetCore.Mvc;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Common;
using System.Text.Json;

namespace OPT.API.Middleware;

/// <summary>
/// Middleware central de manejo de excepciones.
/// Traduce las excepciones de las capas internas a respuestas HTTP consistentes (ProblemDetails).
/// Los controllers NO tienen try/catch — todo pasa por aquí (ADR 0001 / CLAUDE.md).
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next,
                                                 ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await ManejarExcepcionAsync(context, ex, logger);
        }
    }

    private static async Task ManejarExcepcionAsync(HttpContext context, Exception ex,
                                                      ILogger logger)
    {
        var (statusCode, titulo, errores) = ex switch
        {
            NotFoundException nfe     => (StatusCodes.Status404NotFound,
                                          nfe.Message, (object?)null),

            ValidationException ve    => (StatusCodes.Status400BadRequest,
                                          "Errores de validación.", ve.Errores),

            DomainException de        => (StatusCodes.Status422UnprocessableEntity,
                                          de.Message, (object?)null),

            ForbiddenAccessException fae => (StatusCodes.Status403Forbidden,
                                             fae.Message, (object?)null),

            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized,
                                            "No autorizado.", (object?)null),

            _                         => (StatusCodes.Status500InternalServerError,
                                          "Error interno del servidor.", (object?)null)
        };

        if (statusCode >= 500)
            logger.LogError(ex, "Error no controlado: {Message}", ex.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title  = titulo,
            Extensions = errores is not null
                ? new Dictionary<string, object?> { ["errores"] = errores }
                : new Dictionary<string, object?>()
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, _jsonOptions));
    }
}
