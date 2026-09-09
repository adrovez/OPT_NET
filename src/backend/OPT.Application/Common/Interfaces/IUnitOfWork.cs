namespace OPT.Application.Common.Interfaces;

/// <summary>
/// Unidad de trabajo: confirma todos los cambios del repositorio en una sola transacción.
/// La capa de Application llama a CommitAsync — nunca accede al DbContext directamente (ADR 0001).
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
