using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class ComunaRepositorio(AppDbContext context) : IComunaRepositorio
{
    public async Task<IReadOnlyList<Comuna>> ObtenerPorRegionAsync(int regionId, CancellationToken ct = default)
        => await context.Set<Comuna>()
            .Where(c => c.RegionId == regionId)
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);

    public async Task<Comuna?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => await context.Set<Comuna>()
            .Include(c => c.Region)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => await context.Set<Comuna>().AnyAsync(c => c.Id == id, ct);
}
