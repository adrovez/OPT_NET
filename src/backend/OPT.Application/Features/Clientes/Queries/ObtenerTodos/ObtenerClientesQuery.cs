using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.Clientes.Queries.ObtenerTodos;

/// <summary>
/// Búsqueda paginada — Cliente tiene ~12.000 filas (ver CLAUDE.md), no se expone un listado completo.
/// Hereda <see cref="ParametrosPaginacion"/>: pagina, tamanioPagina, busqueda (RUT/Nombre/Apellido),
/// ordenarPor (rut|nombre|apellido|email) y direccionOrden (asc|desc). ASP.NET la enlaza desde
/// el query string ([FromQuery]).
/// </summary>
public record ObtenerClientesQuery : ParametrosPaginacion, IRequest<PagedResult<ClienteDto>>;
