using OPT.Application.Common.Interfaces;

namespace OPT.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
