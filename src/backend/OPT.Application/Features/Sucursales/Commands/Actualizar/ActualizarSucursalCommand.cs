using MediatR;
using OPT.Application.Features.Sucursales;

namespace OPT.Application.Features.Sucursales.Commands.Actualizar;

public record ActualizarSucursalCommand(
    int Id, string Nombre, string? Direccion, string? Telefono) : IRequest<SucursalDto>;
