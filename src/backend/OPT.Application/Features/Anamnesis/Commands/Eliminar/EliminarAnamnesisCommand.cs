using MediatR;

namespace OPT.Application.Features.Anamnesis.Commands.Eliminar;

public record EliminarAnamnesisCommand(Guid PublicId) : IRequest;
