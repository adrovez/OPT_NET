using OPT.Domain.Common;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Domain.Interfaces.Repositories;

public interface IUsuarioRepositorio : IRepositorioBase<Usuario>
{
    Task<Usuario?> ObtenerPorRutConSucursalesAsync(string rut, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default);
    Task<bool>     ExisteRutAsync(string rut, int? excluirId = null, CancellationToken ct = default);

    /// <summary>
    /// Listado paginado (incluye <c>Rol</c> para el nombre en el DTO).
    /// <see cref="ParametrosPaginacion.Busqueda"/> hace match (Contains) contra Nombre, Apellido, RUT y Email.
    /// </summary>
    Task<(IReadOnlyList<Usuario> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros, CancellationToken ct = default);
}
