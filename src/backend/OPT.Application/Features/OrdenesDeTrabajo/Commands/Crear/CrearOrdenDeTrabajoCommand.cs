using MediatR;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Crear;

/// <summary>
/// Alta de una Orden de Trabajo con su detalle. El Precio NO se recibe: se calcula como la
/// suma de las líneas (decisión 2026-08-27), igual que el Saldo.
///
/// <see cref="NumeroOT"/> se ingresa manualmente, igual que en el legacy (decisión 2026-09-11):
/// debe ser único entre las OT del mismo año que no estén anuladas — una OT anulada libera su
/// número para reutilizarlo ese mismo año.
///
/// Opcionalmente registra el abono inicial y genera el plan de cuotas en la misma transacción
/// — es el flujo real del mesón, donde la OT se crea y se abona en un solo acto.
///
/// <see cref="RecetaPublicId"/> vincula a la orden una receta ya tomada en la ficha clínica
/// del cliente: es la prescripción con la que se fabrican estos cristales, y es la que muestra
/// la pestaña Receta de la OT (equivale al <c>OPT_RecetaCristales.idOT</c> del legacy).
/// </summary>
public record CrearOrdenDeTrabajoCommand(
    int            NumeroOT,
    Guid           ClientePublicId,
    int            SucursalId,
    DateTimeOffset FechaEntrega,
    IReadOnlyList<LineaDetalleOTDto> Detalles,
    Guid?     EmpresaPublicId   = null,
    Guid?     RecetaPublicId    = null,
    string?   Observaciones     = null,
    string?   Beneficiario      = null,
    DateOnly? FechaAtencion     = null,
    TimeOnly? HoraEntrega       = null,
    decimal?  AbonoInicial      = null,
    int?      FormaPagoAbono    = null,
    string?   ReferenciaAbono   = null,
    int?      NumeroCuotas      = null,
    DateOnly? PrimerVencimiento = null) : IRequest<OrdenDeTrabajoDto>;
