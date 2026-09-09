using MediatR;
using OPT.Application.Features.Clientes;

namespace OPT.Application.Features.Clientes.Commands.Crear;

public record CrearClienteCommand(
    string Rut, string Nombre, string Apellido, string? Email, string? Telefono,
    string? Direccion, int? ComunaId, DateOnly? FechaNacimiento, string? TipoPrevision)
    : IRequest<ClienteDto>;
