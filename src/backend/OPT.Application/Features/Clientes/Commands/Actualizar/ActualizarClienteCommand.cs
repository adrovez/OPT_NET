using MediatR;
using OPT.Application.Features.Clientes;

namespace OPT.Application.Features.Clientes.Commands.Actualizar;

public record ActualizarClienteCommand(
    Guid PublicId, string Nombre, string Apellido, string? Email, string? Telefono,
    string? Direccion, int? ComunaId, DateOnly? FechaNacimiento, string? TipoPrevision)
    : IRequest<ClienteDto>;
