using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Catálogo de estados del ciclo de vida de una Orden de Trabajo.
/// Entidad de catálogo estática: no lleva auditoría ni borrado lógico.
/// </summary>
public sealed class EstadoOT : CatalogEntity
{
    private EstadoOT() { }

    public EstadoOT(int id, string nombre) : base(id, nombre) { }
}
