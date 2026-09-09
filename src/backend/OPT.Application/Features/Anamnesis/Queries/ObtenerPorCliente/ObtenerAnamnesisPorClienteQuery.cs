using MediatR;
using OPT.Application.Features.Anamnesis;

namespace OPT.Application.Features.Anamnesis.Queries.ObtenerPorCliente;

public record ObtenerAnamnesisPorClienteQuery(Guid ClientePublicId) : IRequest<IReadOnlyList<AnamnesisDto>>;
