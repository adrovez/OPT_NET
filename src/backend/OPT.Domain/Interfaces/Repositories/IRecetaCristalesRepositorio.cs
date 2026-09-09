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
}
