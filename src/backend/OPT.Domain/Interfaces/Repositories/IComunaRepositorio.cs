using OPT.Domain.Entities.Organizacion;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>
/// Comuna es CatalogEntity, no AuditableEntity — no encaja en IRepositorioBase&lt;T&gt;.
/// Catálogo sembrado (346 comunas de Chile), sin operaciones de escritura expuestas a Application.
/// </summary>
public interface IComunaRepositorio
{
    Task<IReadOnlyList<Comuna>> ObtenerPorRegionAsync(int regionId, CancellationToken ct = default);

    /// <summary>Comuna con su Región cargada — para resolver nombres al armar un DTO.</summary>
    Task<Comuna?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteAsync(int id, CancellationToken ct = default);
}
