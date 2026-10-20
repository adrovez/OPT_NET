using MediatR;

namespace OPT.Application.Features.Bodegas.Commands.Activar;

public record ActivarBodegaCommand(Guid PublicId) : IRequest<BodegaDto>;
