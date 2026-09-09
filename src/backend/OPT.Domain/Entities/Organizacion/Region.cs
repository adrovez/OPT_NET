using OPT.Domain.Common;

namespace OPT.Domain.Entities.Organizacion;

/// <summary>
/// Región administrativa de Chile (15 regiones oficiales + Ñuble).
/// Entidad de catálogo estática: no lleva auditoría ni borrado lógico.
/// </summary>
public sealed class Region : CatalogEntity
{
    /// <summary>Código oficial INE, p. ej. "13" para la Región Metropolitana.</summary>
    public string CodigoOficial { get; private set; } = string.Empty;

    private readonly List<Comuna> _comunas = [];
    public IReadOnlyCollection<Comuna> Comunas => _comunas.AsReadOnly();

    // Constructor para EF Core
    private Region() { }

    public Region(int id, string nombre, string codigoOficial)
        : base(id, nombre)
    {
        CodigoOficial = codigoOficial;
    }
}
