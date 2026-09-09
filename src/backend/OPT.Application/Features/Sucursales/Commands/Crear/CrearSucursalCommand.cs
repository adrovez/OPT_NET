using MediatR;
using OPT.Application.Features.Sucursales;

namespace OPT.Application.Features.Sucursales.Commands.Crear;

public record CrearSucursalCommand(
    string Nombre, bool EsMatriz, string? Direccion, string? Telefono) : IRequest<SucursalDto>;
