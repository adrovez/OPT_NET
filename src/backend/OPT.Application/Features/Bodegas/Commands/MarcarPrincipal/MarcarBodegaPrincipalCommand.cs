using MediatR;

namespace OPT.Application.Features.Bodegas.Commands.MarcarPrincipal;

public record MarcarBodegaPrincipalCommand(Guid PublicId) : IRequest<BodegaDto>;
