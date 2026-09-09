using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace OPT.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación genérica del repositorio que filtra automáticamente los registros
/// con borrado lógico (Eliminado = true) en todas las consultas.
/// </summary>
public class RepositorioBase<T>(AppDbContext context)
    : IRepositorioBase<T> where T : AuditableEntity
{
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    /// <summary>
    /// Contexto para las consultas que necesitan otra entidad además de <typeparamref name="T"/>
    /// (p. ej. el listado de OT filtrando por datos del Cliente). Se expone aquí para que los
    /// repositorios derivados no capturen el parámetro del constructor primario (CS9107).
    /// </summary>
    protected AppDbContext Contexto { get; } = context;

    // Filtro global de borrado lógico
    protected IQueryable<T> Activos => _dbSet.Where(e => !e.Eliminado);

    public virtual async Task<T?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<IReadOnlyList<T>> ObtenerTodosAsync(CancellationToken ct = default)
        => await Activos.ToListAsync(ct);

    public virtual async Task<IReadOnlyList<T>> BuscarAsync(
        Expression<Func<T, bool>> predicado, CancellationToken ct = default)
        => await Activos.Where(predicado).ToListAsync(ct);

    public virtual async Task<bool> ExisteAsync(
        Expression<Func<T, bool>> predicado, CancellationToken ct = default)
        => await Activos.AnyAsync(predicado, ct);

    public virtual void Agregar(T entidad)    => _dbSet.Add(entidad);
    public virtual void Actualizar(T entidad) => _dbSet.Update(entidad);
}
