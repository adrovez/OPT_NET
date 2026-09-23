using OPT.Domain.Entities.Clinico;

namespace OPT.Domain.Interfaces.Repositories;

public interface IRecetaCristalesRepositorio : IRepositorioBase<RecetaCristales>
{
    Task<RecetaCristales?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default);
    Task<IReadOnlyList<RecetaCristales>> ObtenerPorClienteAsync(int clienteId, CancellationToken ct = default);

    /// <summary>
    /// Recetas materializadas en una OT (legacy <c>idOT</c>). Normalmente una; los datos
    /// migrados tienen 2 órdenes con dos recetas, por eso devuelve lista.
    /// </summary>
    Task<IReadOnlyList<RecetaCristales>> ObtenerPorOrdenAsync(int ordenDeTrabajoId, CancellationToken ct = default);

    /// <summary>
    /// Igual que <see cref="ObtenerPorOrdenAsync"/> pero para varias OT en una sola consulta —
    /// usado por el Reporte de Cristales de un Operativo (HU-OP-10) para no hacer una consulta
    /// por cada OT asociada.
    /// </summary>
    Task<IReadOnlyList<RecetaCristales>> ObtenerPorOrdenesAsync(IReadOnlyCollection<int> ordenDeTrabajoIds, CancellationToken ct = default);
}
