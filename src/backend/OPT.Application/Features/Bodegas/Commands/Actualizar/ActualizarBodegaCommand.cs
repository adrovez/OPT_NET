using MediatR;

namespace OPT.Application.Features.Bodegas.Commands.Actualizar;

public record ActualizarBodegaCommand(Guid PublicId, string Codigo, string Nombre) : IRequest<BodegaDto>;
