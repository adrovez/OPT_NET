using OPT.Domain.Common;

namespace OPT.Domain.Entities.Operativo;

/// <summary>
/// Catálogo de estados del ciclo de vida de un Operativo (jornada de atención en terreno).
/// Entidad de catálogo estática: no lleva auditoría ni borrado lógico.
/// </summary>
public sealed class EstadoOperativo : CatalogEntity
{
    private EstadoOperativo() { }

    public EstadoOperativo(int id, string nombre) : base(id, nombre) { }
}
