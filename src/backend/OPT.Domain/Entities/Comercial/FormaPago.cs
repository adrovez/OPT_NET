using OPT.Domain.Common;

namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Catálogo de medios de pago aceptados para un Abono.
/// Entidad de catálogo estática: no lleva auditoría ni borrado lógico.
/// </summary>
public sealed class FormaPago : CatalogEntity
{
    private FormaPago() { }

    public FormaPago(int id, string nombre) : base(id, nombre) { }
}
