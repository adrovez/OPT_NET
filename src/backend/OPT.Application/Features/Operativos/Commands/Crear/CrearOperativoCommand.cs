using MediatR;

namespace OPT.Application.Features.Operativos.Commands.Crear;

/// <summary>
/// Alta de un Operativo. Nace en estado PROSPECTO, con montos en 0 — las OT se asocian
/// después con <c>POST /api/operativos/{publicId}/ordenes</c>.
/// El correlativo NO se recibe: lo asigna la base de datos (decisión 2026-09-15, punto 8.5).
/// </summary>
public record CrearOperativoCommand(
    Guid     EmpresaPublicId,
    int      SucursalId,
    DateOnly Fecha,
    string?  Observacion = null) : IRequest<OperativoDto>;
