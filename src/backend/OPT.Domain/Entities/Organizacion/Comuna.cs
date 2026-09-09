using OPT.Domain.Common;

namespace OPT.Domain.Entities.Organizacion;

/// <summary>
/// Comuna de Chile agrupada por Región.
/// Entidad de catálogo estática: no lleva auditoría ni borrado lógico.
/// </summary>
public sealed class Comuna : CatalogEntity
{
    public int     RegionId { get; private set; }
    public Region? Region   { get; private set; }

    // Constructor para EF Core
    private Comuna() { }

    public Comuna(int id, string nombre, int regionId)
        : base(id, nombre)
    {
        RegionId = regionId;
    }
}
