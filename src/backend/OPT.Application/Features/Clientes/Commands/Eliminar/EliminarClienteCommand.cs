using MediatR;

namespace OPT.Application.Features.Clientes.Commands.Eliminar;

public record EliminarClienteCommand(Guid PublicId) : IRequest;
