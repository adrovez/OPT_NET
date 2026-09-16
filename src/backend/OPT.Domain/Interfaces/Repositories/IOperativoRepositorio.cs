using OPT.Domain.Common;
using OPT.Domain.Entities.Operativo;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>
/// El Operativo es la raíz de su propio agregado: las OT asociadas y los gastos se cargan y
/// modifican siempre a través de él. La API lo direcciona por <see cref="Operativo.PublicId"/>
/// (mismo criterio que <c>OrdenDeTrabajo</c>, ADR 0004).
/// </summary>
public interface IOperativoRepositorio : IRepositorioBase<Operativo>
{
    /// <summary>Cabecera sola, por identificador público — para lecturas que no tocan las colecciones.</summary>
    Task<Operativo?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>
    /// Agregado completo (OT asociadas + gastos) por identificador público. Es la carga que
    /// necesita cualquier comando que modifique dinero, OT asociadas o estado.
    /// </summary>
    Task<Operativo?> ObtenerCompletaPorPublicIdAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>True si la OT (por su Id interno) ya está asociada a algún Operativo — no anulado o no.</summary>
    Task<bool> OrdenYaAsociadaAsync(int ordenDeTrabajoId, CancellationToken ct = default);

    /// <summary>
    /// Listado paginado. <see cref="ParametrosPaginacion.Busqueda"/> hace match contra el
    /// Correlativo (si el término es numérico) y la Observación. Los filtros son acumulativos
    /// y opcionales.
    /// </summary>
    Task<(IReadOnlyList<Operativo> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros,
        int? empresaId = null,
        int? sucursalId = null,
        int? estadoOperativoId = null,
        CancellationToken ct = default);
}
