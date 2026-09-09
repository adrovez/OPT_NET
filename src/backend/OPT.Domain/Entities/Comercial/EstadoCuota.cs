using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Catálogo de estados de una cuota del plan de pago (PENDIENTE / PAGADA / ANULADA).
/// Entidad de catálogo estática: no lleva auditoría ni borrado lógico.
/// </summary>
public sealed class EstadoCuota : CatalogEntity
{
    private EstadoCuota() { }

    public EstadoCuota(int id, string nombre) : base(id, nombre) { }
}
