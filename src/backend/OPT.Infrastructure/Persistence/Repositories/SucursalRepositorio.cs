using System.Linq.Expressions;
using OPT.Domain.Common;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Persistence.Extensions;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class SucursalRepositorio(AppDbContext context)
    : RepositorioBase<Sucursal>(context), ISucursalRepositorio
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Sucursal, object>>> ColumnasOrden =
        new Dictionary<string, Expression<Func<Sucursal, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["nombre"]    = s => s.Nombre,
            ["direccion"] = s => s.Direccion!,
        };

    public async Task<(IReadOnlyList<Sucursal> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default)
    {
        var query = Activos;

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(s =>
                s.Nombre.Contains(busqueda) ||
                (s.Direccion != null && s.Direccion.Contains(busqueda)));

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, s => s.Nombre);

        return await query.PaginarAsync(parametros, ct);
    }
}
