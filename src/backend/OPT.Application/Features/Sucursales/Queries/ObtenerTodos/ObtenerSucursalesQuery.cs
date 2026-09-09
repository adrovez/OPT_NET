using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.Sucursales.Queries.ObtenerTodos;

/// <summary>
/// Listado paginado de sucursales. Hereda <see cref="ParametrosPaginacion"/>: pagina,
/// tamanioPagina, busqueda (Nombre/Direccion), ordenarPor (nombre|direccion),
/// direccionOrden (asc|desc). ASP.NET la enlaza desde el query string ([FromQuery]).
/// </summary>
public record ObtenerSucursalesQuery : ParametrosPaginacion, IRequest<PagedResult<SucursalDto>>;
