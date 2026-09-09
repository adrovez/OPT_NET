using OPT.Domain.Common;

namespace OPT.Domain.Entities.Organizacion;

/// <summary>
/// Catálogo de roles del sistema. Determina permisos del Usuario.
/// Entidad de catálogo estática: no lleva auditoría ni borrado lógico.
/// </summary>
public sealed class Rol : CatalogEntity
{
    public string? Descripcion { get; private set; }

    private Rol() { }

    public Rol(int id, string nombre) : base(id, nombre) { }
}
