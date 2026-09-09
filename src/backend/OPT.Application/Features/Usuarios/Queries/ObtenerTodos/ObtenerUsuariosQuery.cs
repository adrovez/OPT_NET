using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.Usuarios.Queries.ObtenerTodos;

/// <summary>
/// Listado paginado de usuarios. Hereda <see cref="ParametrosPaginacion"/>: pagina,
/// tamanioPagina, busqueda (Nombre/Apellido/RUT/Email),
/// ordenarPor (rut|nombre|apellido|email|activo), direccionOrden (asc|desc).
/// ASP.NET la enlaza desde el query string ([FromQuery]).
/// </summary>
public record ObtenerUsuariosQuery : ParametrosPaginacion, IRequest<PagedResult<UsuarioDto>>;
