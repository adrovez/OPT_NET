using MediatR;

namespace OPT.Application.Features.Bodegas.Commands.Crear;

/// <param name="EsPrincipal">Si la sucursal aún no tiene bodegas la nueva es principal sí o sí.</param>
public record CrearBodegaCommand(int SucursalId, string Codigo, string Nombre, bool EsPrincipal = false)
    : IRequest<BodegaDto>;
