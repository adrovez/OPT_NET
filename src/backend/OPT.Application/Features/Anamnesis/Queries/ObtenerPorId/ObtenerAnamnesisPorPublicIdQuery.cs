using MediatR;
using OPT.Application.Features.Anamnesis;

namespace OPT.Application.Features.Anamnesis.Queries.ObtenerPorId;

public record ObtenerAnamnesisPorPublicIdQuery(Guid PublicId) : IRequest<AnamnesisDto>;
