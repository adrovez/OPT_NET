using OPT.Domain.Common;
using System.Linq.Expressions;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato genérico de repositorio.
/// La capa de Application solo ve estas interfaces — nunca el DbContext directamente (ADR 0001).
/// </summary>
public interface IRepositorioBase<T> where T : AuditableEntity
{
    Task<T?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> BuscarAsync(Expression<Func<T, bool>> predicado, CancellationToken ct = default);
    Task<bool> ExisteAsync(Expression<Func<T, bool>> predicado, CancellationToken ct = default);
    void Agregar(T entidad);
    void Actualizar(T entidad);
}
