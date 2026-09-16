namespace OPT.Application.Common.Exceptions;

/// <summary>
/// El usuario está autenticado pero no tiene acceso al recurso solicitado (HTTP 403) —
/// hoy usada por <c>AutorizacionSucursal</c> para el control BOLA por sucursal.
/// </summary>
public sealed class ForbiddenAccessException(string mensaje) : Exception(mensaje);
