using OPT.Domain.Entities.Operativo;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>
/// EstadoOperativo es CatalogEntity, no AuditableEntity — no encaja en IRepositorioBase&lt;T&gt;.
/// Catálogo sembrado (mismo patrón que IEstadoOTRepositorio), sin operaciones de escritura.
/// </summary>
public interface IEstadoOperativoRepositorio
{
    Task<IReadOnlyList<EstadoOperativo>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(int id, CancellationToken ct = default);
}
