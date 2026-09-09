namespace OPT.Application.Common.Exceptions;

/// <summary>La entrada no pasó la validación de FluentValidation. → HTTP 400.</summary>
public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errores { get; }

    public ValidationException(IDictionary<string, string[]> errores)
        : base("Se encontraron uno o más errores de validación.")
    {
        Errores = new Dictionary<string, string[]>(errores);
    }
}
