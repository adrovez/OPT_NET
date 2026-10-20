using MediatR;

namespace OPT.Application.Features.Bodegas.Commands.Desactivar;

public record DesactivarBodegaCommand(Guid PublicId) : IRequest<BodegaDto>;
