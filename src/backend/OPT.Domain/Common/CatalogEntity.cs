namespace OPT.Domain.Common;

/// <summary>
/// Clase base para entidades de catálogo estáticas (sin auditoría ni borrado lógico).
/// Usar para datos de referencia que no cambian en el negocio: Region, Comuna, Rol, etc.
/// </summary>
public abstract class CatalogEntity
{
    public int    Id     { get; protected set; }
    public string Nombre { get; protected set; } = string.Empty;

    protected CatalogEntity() { }

    protected CatalogEntity(int id, string nombre)
    {
        Id     = id;
        Nombre = nombre;
    }
}
