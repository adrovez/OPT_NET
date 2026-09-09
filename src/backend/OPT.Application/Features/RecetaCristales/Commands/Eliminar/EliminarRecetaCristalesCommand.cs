using MediatR;

namespace OPT.Application.Features.RecetaCristales.Commands.Eliminar;

public record EliminarRecetaCristalesCommand(Guid PublicId) : IRequest;
