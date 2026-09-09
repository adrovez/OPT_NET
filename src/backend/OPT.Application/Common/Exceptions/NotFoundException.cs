namespace OPT.Application.Common.Exceptions;

/// <summary>La entidad solicitada no existe o fue eliminada lógicamente. → HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entidad, object clave)
        : base($"{entidad} con identificador '{clave}' no fue encontrada.") { }
}
