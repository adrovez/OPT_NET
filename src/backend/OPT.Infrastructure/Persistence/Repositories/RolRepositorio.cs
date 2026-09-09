using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class RolRepositorio(AppDbContext context) : IRolRepositorio
{
    public async Task<IReadOnlyList<Rol>> ObtenerTodosAsync(CancellationToken ct = default)
        => await context.Set<Rol>().ToListAsync(ct);

    public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => await context.Set<Rol>().AnyAsync(r => r.Id == id, ct);
}
