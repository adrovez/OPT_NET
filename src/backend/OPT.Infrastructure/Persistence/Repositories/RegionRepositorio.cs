using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class RegionRepositorio(AppDbContext context) : IRegionRepositorio
{
    public async Task<IReadOnlyList<Region>> ObtenerTodosAsync(CancellationToken ct = default)
        => await context.Set<Region>()
            .OrderBy(r => r.Nombre)
            .ToListAsync(ct);
}
