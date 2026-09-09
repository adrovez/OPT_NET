using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Persistence.Extensions;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class ClienteRepositorio(AppDbContext context)
    : RepositorioBase<Cliente>(context), IClienteRepositorio
{
    /// <summary>Columnas por las que el listado permite ordenar (lista blanca — el resto se ignora).</summary>
    private static readonly IReadOnlyDictionary<string, Expression<Func<Cliente, object>>> ColumnasOrden =
        new Dictionary<string, Expression<Func<Cliente, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["rut"]      = c => c.Rut,
            ["nombre"]   = c => c.Nombre,
            ["apellido"] = c => c.Apellido,
            ["email"]    = c => c.Email!,
        };

    public async Task<Cliente?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(c => c.PublicId == publicId, ct);

    public async Task<Cliente?> ObtenerPorRutAsync(string rut, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(c => c.Rut == rut, ct);

    public async Task<IReadOnlyList<Cliente>> ObtenerPorIdsAsync(
        IEnumerable<int> ids, CancellationToken ct = default)
    {
        var lista = ids.Distinct().ToList();
        if (lista.Count == 0) return [];

        return await Activos.Where(c => lista.Contains(c.Id)).ToListAsync(ct);
    }

    public async Task<bool> ExisteRutAsync(string rut, int? excluirId = null, CancellationToken ct = default)
        => await Activos.AnyAsync(
            c => c.Rut == rut && (excluirId == null || c.Id != excluirId), ct);

    public async Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default)
    {
        var query = Activos;

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(c =>
                c.Rut.Contains(busqueda) ||
                c.Nombre.Contains(busqueda) ||
                c.Apellido.Contains(busqueda));

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, c => c.Apellido);

        return await query.PaginarAsync(parametros, ct);
    }
}
