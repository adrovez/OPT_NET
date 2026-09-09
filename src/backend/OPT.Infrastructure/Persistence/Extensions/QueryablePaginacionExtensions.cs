using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;

namespace OPT.Infrastructure.Persistence.Extensions;

/// <summary>
/// Primitivas de paginación y ordenamiento reutilizables por todos los repositorios.
/// Viven en Infrastructure porque materializan <see cref="IQueryable{T}"/> con EF Core —
/// la capa de Application nunca toca <c>IQueryable</c> ni el <c>DbContext</c> (ver CLAUDE.md).
///
/// Uso típico dentro de un repositorio:
/// <code>
/// var q = Activos;
/// if (!string.IsNullOrWhiteSpace(p.Busqueda))
///     q = q.Where(e => e.Nombre.Contains(p.Busqueda));
/// q = q.AplicarOrden(p.OrdenarPor, p.OrdenDescendente, _columnasOrden, e => e.Nombre);
/// return await q.PaginarAsync(p, ct);
/// </code>
/// </summary>
public static class QueryablePaginacionExtensions
{
    /// <summary>
    /// Ordena por una columna de la lista blanca (segura contra inyección: solo se aceptan
    /// claves declaradas explícitamente por la entidad). Si <paramref name="ordenarPor"/> no
    /// coincide con ninguna clave, se usa <paramref name="ordenPorDefecto"/> ascendente.
    /// Siempre agrega <c>Id</c> como desempate final para que <c>Skip</c>/<c>Take</c> sea
    /// determinista entre páginas.
    /// </summary>
    public static IQueryable<T> AplicarOrden<T>(
        this IQueryable<T> query,
        string? ordenarPor,
        bool descendente,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> columnas,
        Expression<Func<T, object>> ordenPorDefecto)
        where T : AuditableEntity
    {
        var selector = !string.IsNullOrWhiteSpace(ordenarPor)
                       && columnas.TryGetValue(ordenarPor.Trim(), out var columna)
            ? columna
            : ordenPorDefecto;

        var ordenado = descendente
            ? query.OrderByDescending(selector)
            : query.OrderBy(selector);

        return ordenado.ThenBy(e => e.Id);
    }

    /// <summary>
    /// Obtiene el total (tras filtros) y la página solicitada. Llamar SIEMPRE después de
    /// aplicar filtros (<c>Where</c>) y orden (<see cref="AplicarOrden{T}"/>).
    /// </summary>
    public static async Task<(IReadOnlyList<T> Items, int Total)> PaginarAsync<T>(
        this IQueryable<T> query, ParametrosPaginacion parametros, CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);

        var items = await query
            .Skip(parametros.Saltar)
            .Take(parametros.TamanioPagina)
            .ToListAsync(ct);

        return (items, total);
    }
}
