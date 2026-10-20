using MediatR;

namespace OPT.Application.Features.Bodegas.Commands.Eliminar;

public record EliminarBodegaCommand(Guid PublicId) : IRequest;
