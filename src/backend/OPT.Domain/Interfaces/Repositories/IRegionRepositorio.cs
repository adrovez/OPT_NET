using OPT.Domain.Entities.Organizacion;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>
/// Region es CatalogEntity, no AuditableEntity — no encaja en IRepositorioBase&lt;T&gt;.
/// Catálogo sembrado (16 regiones de Chile), sin operaciones de escritura expuestas a Application.
/// </summary>
public interface IRegionRepositorio
{
    Task<IReadOnlyList<Region>> ObtenerTodosAsync(CancellationToken ct = default);
}
