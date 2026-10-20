using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.Bodegas.Queries.ObtenerTodos;

/// <summary>
/// Listado paginado de bodegas. Hereda <see cref="ParametrosPaginacion"/> (busqueda sobre
/// Código/Nombre, ordenarPor codigo|nombre). Si no se indica <see cref="SucursalId"/>, quien no
/// tiene alcance nacional ve solo las de su sucursal activa.
/// </summary>
public record ObtenerBodegasQuery : ParametrosPaginacion, IRequest<PagedResult<BodegaDto>>
{
    public int? SucursalId  { get; init; }
    public bool SoloActivas { get; init; }
}
