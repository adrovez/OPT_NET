namespace OPT.Domain.Common;

/// <summary>
/// Excepción semántica del dominio. Se lanza cuando una regla de negocio invariante
/// es violada. La capa API la traduce a HTTP 422 Unprocessable Entity.
/// Nunca usar throw ex (destruye el stack trace) — siempre throw o esta clase.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}
