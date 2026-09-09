using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Persistence.Extensions;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class EmpresaRepositorio(AppDbContext context)
    : RepositorioBase<Empresa>(context), IEmpresaRepositorio
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Empresa, object>>> ColumnasOrden =
        new Dictionary<string, Expression<Func<Empresa, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["nombre"]      = e => e.Nombre,
            ["rut"]         = e => e.Rut,
            ["razonSocial"] = e => e.RazonSocial,
        };

    public async Task<Empresa?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default)
        => await Activos.FirstOrDefaultAsync(e => e.PublicId == publicId, ct);

    public async Task<bool> ExisteRutAsync(string rut, int? excluirId = null, CancellationToken ct = default)
        => await Activos.AnyAsync(
            e => e.Rut == rut && (excluirId == null || e.Id != excluirId), ct);

    public async Task<(IReadOnlyList<Empresa> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default)
    {
        var query = Activos;

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(e =>
                e.Nombre.Contains(busqueda) ||
                e.Rut.Contains(busqueda) ||
                e.RazonSocial.Contains(busqueda));

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, e => e.Nombre);

        return await query.PaginarAsync(parametros, ct);
    }
}
