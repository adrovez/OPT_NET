using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.Empresas.Queries.ObtenerTodos;

/// <summary>
/// Listado paginado de empresas. Hereda <see cref="ParametrosPaginacion"/>: pagina,
/// tamanioPagina, busqueda (Nombre/RUT/RazonSocial), ordenarPor (nombre|rut|razonSocial),
/// direccionOrden (asc|desc). ASP.NET la enlaza desde el query string ([FromQuery]).
/// </summary>
public record ObtenerEmpresasQuery : ParametrosPaginacion, IRequest<PagedResult<EmpresaDto>>;
