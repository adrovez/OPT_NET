namespace OPT.Application.Common.Interfaces;

/// <summary>
/// Unidad de trabajo: confirma todos los cambios del repositorio en una sola transacción.
/// La capa de Application llama a CommitAsync — nunca accede al DbContext directamente (ADR 0001).
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Ejecuta <paramref name="accion"/> dentro de una transacción de base de datos: todos los
    /// <see cref="CommitAsync"/> que haga se confirman juntos o se revierten juntos. Se usa cuando
    /// una operación necesita más de un guardado (p. ej. obtener el Id generado de una entidad
    /// antes de crear otra que la referencia, o intercambiar un índice único filtrado).
    /// </summary>
    Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> accion, CancellationToken ct = default);
}
