using OPT.Domain.Common;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Domain.Interfaces.Repositories;

public interface IEmpresaRepositorio : IRepositorioBase<Empresa>
{
    Task<Empresa?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default);
    Task<bool>     ExisteRutAsync(string rut, int? excluirId = null, CancellationToken ct = default);

    /// <summary><see cref="ParametrosPaginacion.Busqueda"/> hace match (Contains) contra Nombre, RUT y RazonSocial.</summary>
    Task<(IReadOnlyList<Empresa> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default);
}
