using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Comercial;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Infrastructure.Persistence.Repositories;

/// <summary>
/// Catálogos del módulo Comercial — sembrados, solo lectura, mismo patrón que RolRepositorio.
/// No heredan de RepositorioBase&lt;T&gt; porque son CatalogEntity (sin auditoría ni borrado lógico).
/// </summary>
public sealed class EstadoOTRepositorio(AppDbContext context) : IEstadoOTRepositorio
{
    public async Task<IReadOnlyList<EstadoOT>> ObtenerTodosAsync(CancellationToken ct = default)
        => await context.Set<EstadoOT>().OrderBy(e => e.Id).ToListAsync(ct);

    public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => await context.Set<EstadoOT>().AnyAsync(e => e.Id == id, ct);
}

public sealed class FormaPagoRepositorio(AppDbContext context) : IFormaPagoRepositorio
{
    public async Task<IReadOnlyList<FormaPago>> ObtenerTodosAsync(CancellationToken ct = default)
        => await context.Set<FormaPago>().OrderBy(f => f.Id).ToListAsync(ct);

    public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => await context.Set<FormaPago>().AnyAsync(f => f.Id == id, ct);
}

public sealed class EstadoCuotaRepositorio(AppDbContext context) : IEstadoCuotaRepositorio
{
    public async Task<IReadOnlyList<EstadoCuota>> ObtenerTodosAsync(CancellationToken ct = default)
        => await context.Set<EstadoCuota>().OrderBy(e => e.Id).ToListAsync(ct);

    public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => await context.Set<EstadoCuota>().AnyAsync(e => e.Id == id, ct);
}
