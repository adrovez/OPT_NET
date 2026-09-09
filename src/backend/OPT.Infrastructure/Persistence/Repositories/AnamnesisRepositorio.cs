using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Infrastructure.Persistence.Repositories;

public sealed class AnamnesisRepositorio(AppDbContext context)
    : RepositorioBase<Anamnesis>(context), IAnamnesisRepositorio
{
    public async Task<Anamnesis?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default)
        => await Activos.Include(a => a.Cliente).FirstOrDefaultAsync(a => a.PublicId == publicId, ct);

    public async Task<IReadOnlyList<Anamnesis>> ObtenerPorClienteAsync(int clienteId, CancellationToken ct = default)
        => await Activos
            .Where(a => a.ClienteId == clienteId)
            .OrderByDescending(a => a.CreadoEn)
            .ToListAsync(ct);
}
