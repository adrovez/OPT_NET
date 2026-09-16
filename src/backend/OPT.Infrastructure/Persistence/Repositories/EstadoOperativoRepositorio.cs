using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Operativo;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Infrastructure.Persistence.Repositories;

/// <summary>Catálogo sembrado, solo lectura — mismo patrón que <c>EstadoOTRepositorio</c>.</summary>
public sealed class EstadoOperativoRepositorio(AppDbContext context) : IEstadoOperativoRepositorio
{
    public async Task<IReadOnlyList<EstadoOperativo>> ObtenerTodosAsync(CancellationToken ct = default)
        => await context.Set<EstadoOperativo>().OrderBy(e => e.Id).ToListAsync(ct);

    public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => await context.Set<EstadoOperativo>().AnyAsync(e => e.Id == id, ct);
}
