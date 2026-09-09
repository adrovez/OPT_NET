using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class RecetaCristalesRepositorio(AppDbContext context)
    : RepositorioBase<RecetaCristales>(context), IRecetaCristalesRepositorio
{
    public async Task<RecetaCristales?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default)
        => await Activos.Include(r => r.Cliente).FirstOrDefaultAsync(r => r.PublicId == publicId, ct);

    public async Task<IReadOnlyList<RecetaCristales>> ObtenerPorClienteAsync(int clienteId, CancellationToken ct = default)
        => await Activos
            .Where(r => r.ClienteId == clienteId)
            .OrderByDescending(r => r.CreadoEn)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RecetaCristales>> ObtenerPorOrdenAsync(int ordenDeTrabajoId, CancellationToken ct = default)
        => await Activos
            .Where(r => r.OrdenDeTrabajoId == ordenDeTrabajoId)
            .OrderBy(r => r.Id)
            .ToListAsync(ct);
}
