using OPT.Domain.Common;
using OPT.Domain.Entities.Clinico;

namespace OPT.Domain.Interfaces.Repositories;

public interface IClienteRepositorio : IRepositorioBase<Cliente>
{
    Task<Cliente?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default);
    Task<Cliente?> ObtenerPorRutAsync(string rut, CancellationToken ct = default);
    Task<bool>     ExisteRutAsync(string rut, int? excluirId = null, CancellationToken ct = default);

    /// <summary>
    /// Resuelve varios clientes por su Id interno en una sola consulta. Lo usa el listado de
    /// Órdenes de Trabajo para mostrar el RUT/nombre del cliente de cada fila de la página
    /// (una consulta por página, no una por fila).
    /// </summary>
    Task<IReadOnlyList<Cliente>> ObtenerPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);

    /// <summary>
    /// Búsqueda paginada — Cliente tiene ~12.000 filas, no se expone un listado completo.
    /// <see cref="ParametrosPaginacion.Busqueda"/> hace match (Contains) contra RUT, Nombre y Apellido.
    /// </summary>
    Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default);
}
