using OPT.Domain.Entities.Organizacion;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>
/// Rol es CatalogEntity, no AuditableEntity — no encaja en IRepositorioBase&lt;T&gt;.
/// Catálogo sembrado, sin operaciones de escritura expuestas a Application.
/// </summary>
public interface IRolRepositorio
{
    Task<IReadOnlyList<Rol>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(int id, CancellationToken ct = default);
}
