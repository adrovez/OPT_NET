using MediatR;

namespace OPT.Application.Features.Sucursales.Commands.Eliminar;

public record EliminarSucursalCommand(int Id) : IRequest;
