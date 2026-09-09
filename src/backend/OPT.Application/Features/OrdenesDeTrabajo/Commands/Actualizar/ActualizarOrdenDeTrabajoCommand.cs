using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Actualizar;

/// <summary>
/// Edición de la cabecera de la OT y, opcionalmente, de su detalle completo.
/// Si <see cref="Detalles"/> viene con valor reemplaza todas las líneas vigentes y el Precio
/// se recalcula; si viene null el detalle no se toca — el detalle es parte del agregado, nunca
/// un recurso propio (ADR 0004).
///
/// <see cref="RecetaPublicId"/> se comporta igual que <see cref="EmpresaPublicId"/>: el valor
/// recibido es el estado final. Si viene null la orden queda sin receta vinculada, así que el
/// cliente de la API debe reenviar la receta actual cuando no la quiera cambiar.
/// </summary>
public record ActualizarOrdenDeTrabajoCommand(
    Guid           PublicId,
    DateTimeOffset FechaEntrega,
    IReadOnlyList<LineaDetalleOTDto>? Detalles = null,
    Guid?     EmpresaPublicId = null,
    Guid?     RecetaPublicId  = null,
    string?   Observaciones   = null,
    string?   Beneficiario    = null,
    DateOnly? FechaAtencion   = null,
    TimeOnly? HoraEntrega     = null) : IRequest<OrdenDeTrabajoDto>;
