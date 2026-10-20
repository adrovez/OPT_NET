using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Infrastructure.Persistence.Repositories;

/// <summary>Catálogo sembrado, solo lectura — mismo patrón que EstadoCuotaRepositorio.</summary>
public sealed class CategoriaProductoRepositorio(AppDbContext context) : ICategoriaProductoRepositorio
{
    public async Task<IReadOnlyList<CategoriaProducto>> ObtenerTodosAsync(CancellationToken ct = default)
        => await context.Set<CategoriaProducto>().OrderBy(c => c.Id).ToListAsync(ct);

    public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => await context.Set<CategoriaProducto>().AnyAsync(c => c.Id == id, ct);
}
