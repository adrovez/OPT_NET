using MediatR;

namespace OPT.Application.Features.Operativos.Commands.Actualizar;

/// <summary>
/// Datos de cabecera editables mientras el Operativo no esté en un estado terminal
/// (CERRADO/ANULADO). Empresa y Sucursal son inmutables tras crear el Operativo — igual
/// criterio que <c>NumeroOT</c> en la OT: cambiarlas después rompería la trazabilidad de la
/// jornada que ya se registró.
/// </summary>
public record ActualizarOperativoCommand(
    Guid     PublicId,
    string   Nombre,
    DateOnly Fecha,
    string?  Observacion = null,
    string?  NombreContacto = null,
    string?  MailContacto = null,
    string?  TelefonoContacto = null) : IRequest<OperativoDto>;
