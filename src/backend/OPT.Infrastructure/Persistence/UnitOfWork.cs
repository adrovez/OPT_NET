using OPT.Application.Common.Interfaces;

namespace OPT.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);

    public async Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> accion, CancellationToken ct = default)
    {
        // Si el DisposeAsync corre sin CommitAsync, la transacción se revierte sola.
        await using var transaccion = await context.Database.BeginTransactionAsync(ct);
        await accion(ct);
        await transaccion.CommitAsync(ct);
    }
}
