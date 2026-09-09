using OPT.Domain.Entities.Comercial;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>
/// EstadoOT es CatalogEntity, no AuditableEntity — no encaja en IRepositorioBase&lt;T&gt;.
/// Catálogo sembrado (mismo patrón que IRolRepositorio), sin operaciones de escritura.
/// </summary>
public interface IEstadoOTRepositorio
{
    Task<IReadOnlyList<EstadoOT>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(int id, CancellationToken ct = default);
}

/// <summary>Catálogo de medios de pago (EFECTIVO, TARJETA…, CHEQUE). Solo lectura.</summary>
public interface IFormaPagoRepositorio
{
    Task<IReadOnlyList<FormaPago>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(int id, CancellationToken ct = default);
}

/// <summary>Catálogo de estados de cuota (PENDIENTE / PAGADA / ANULADA). Solo lectura.</summary>
public interface IEstadoCuotaRepositorio
{
    Task<IReadOnlyList<EstadoCuota>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(int id, CancellationToken ct = default);
}
