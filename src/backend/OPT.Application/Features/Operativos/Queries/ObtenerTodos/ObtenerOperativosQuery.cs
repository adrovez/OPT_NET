using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.Operativos.Queries.ObtenerTodos;

/// <summary>
/// Listado paginado de Operativos. Hereda <see cref="ParametrosPaginacion"/>: pagina,
/// tamanioPagina, busqueda (correlativo, nombre u observación), ordenarPor
/// (correlativo|nombre|fecha|estado|creadoEn) y direccionOrden (asc|desc). Filtros adicionales
/// acumulativos y opcionales.
/// </summary>
public record ObtenerOperativosQuery : ParametrosPaginacion, IRequest<PagedResult<OperativoResumenDto>>
{
    public Guid? EmpresaPublicId    { get; init; }
    public int?  SucursalId         { get; init; }
    public int?  EstadoOperativoId  { get; init; }
}
