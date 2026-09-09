using FluentValidation;
using MediatR;

namespace OPT.Application.Common.Behaviours;

/// <summary>
/// Pipeline de MediatR que ejecuta validaciones FluentValidation antes de cada handler.
/// Si falla, lanza ValidationException — el middleware la traduce a HTTP 400.
/// Los controllers nunca hacen try/catch (ADR 0001).
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request,
                                         RequestHandlerDelegate<TResponse> next,
                                         CancellationToken ct)
    {
        if (!validators.Any()) return await next();

        var contexto = new ValidationContext<TRequest>(request);
        var resultados = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(contexto, ct)));

        var errores = resultados
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());

        if (errores.Count > 0)
            throw new Exceptions.ValidationException(errores);

        return await next();
    }
}
