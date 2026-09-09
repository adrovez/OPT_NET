using OPT.Domain.Entities.Clinico;

namespace OPT.Domain.Interfaces.Repositories;

public interface IAnamnesisRepositorio : IRepositorioBase<Anamnesis>
{
    Task<Anamnesis?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default);
    Task<IReadOnlyList<Anamnesis>> ObtenerPorClienteAsync(int clienteId, CancellationToken ct = default);
}
