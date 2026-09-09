using OPT.Domain.Common;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Domain.Interfaces.Repositories;

public interface ISucursalRepositorio : IRepositorioBase<Sucursal>
{
    /// <summary><see cref="ParametrosPaginacion.Busqueda"/> hace match (Contains) contra Nombre y Direccion.</summary>
    Task<(IReadOnlyList<Sucursal> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default);
}
