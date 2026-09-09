using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OPT.Domain.Common;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Persistence.Extensions;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepositorio(AppDbContext context)
    : RepositorioBase<Usuario>(context), IUsuarioRepositorio
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Usuario, object>>> ColumnasOrden =
        new Dictionary<string, Expression<Func<Usuario, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["rut"]      = u => u.Rut,
            ["nombre"]   = u => u.Nombre,
            ["apellido"] = u => u.Apellido,
            ["email"]    = u => u.Email!,
            ["activo"]   = u => u.Activo,
        };

    public async Task<Usuario?> ObtenerPorRutConSucursalesAsync(string rut, CancellationToken ct = default)
        => await Activos
            .Include(u => u.Sucursales)
            .FirstOrDefaultAsync(u => u.Rut == rut, ct);

    public async Task<Usuario?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default)
        => await Activos
            .Include(u => u.Sucursales)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.PublicId == publicId, ct);

    public async Task<bool> ExisteRutAsync(string rut, int? excluirId = null, CancellationToken ct = default)
        => await Activos.AnyAsync(
            u => u.Rut == rut && (excluirId == null || u.Id != excluirId), ct);

    public override async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken ct = default)
        => await Activos.Include(u => u.Rol).ToListAsync(ct);

    public async Task<(IReadOnlyList<Usuario> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default)
    {
        var query = Activos.Include(u => u.Rol).AsQueryable();

        var busqueda = parametros.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(u =>
                u.Nombre.Contains(busqueda) ||
                u.Apellido.Contains(busqueda) ||
                u.Rut.Contains(busqueda) ||
                (u.Email != null && u.Email.Contains(busqueda)));

        query = query.AplicarOrden(
            parametros.OrdenarPor, parametros.OrdenDescendente, ColumnasOrden, u => u.Nombre);

        return await query.PaginarAsync(parametros, ct);
    }
}
