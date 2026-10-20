using OPT.Domain.Common;
using OPT.Domain.Entities.Inventario;

namespace OPT.Domain.Interfaces.Repositories;

public interface IBodegaRepositorio : IRepositorioBase<Bodega>
{
    Task<Bodega?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>La bodega principal vigente de la sucursal, si tiene.</summary>
    Task<Bodega?> ObtenerPrincipalAsync(int sucursalId, CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(int sucursalId, string codigo, int? excluirId = null, CancellationToken ct = default);

    Task<bool> SucursalTieneBodegasAsync(int sucursalId, CancellationToken ct = default);

    /// <summary>Suma del stock de todos los productos de la bodega (para decidir si se puede desactivar o eliminar).</summary>
    Task<int> ObtenerStockTotalAsync(int bodegaId, CancellationToken ct = default);

    /// <summary>
    /// <see cref="ParametrosPaginacion.Busqueda"/> hace match (Contains) contra Código y Nombre.
    /// <paramref name="sucursalId"/> filtra por sucursal; <paramref name="soloActivas"/> excluye las desactivadas.
    /// </summary>
    Task<(IReadOnlyList<Bodega> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, int? sucursalId, bool soloActivas, CancellationToken ct = default);
}
