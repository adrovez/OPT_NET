using MediatR;
using OPT.Application.Features.Anamnesis;

namespace OPT.Application.Features.Anamnesis.Commands.Crear;

public record CrearAnamnesisCommand(
    Guid ClientePublicId, bool Hipertension, bool Diabetes, bool Alergias,
    string? DetalleAlergias, bool UsaLentesPrevio, string? Observaciones)
    : IRequest<AnamnesisDto>;
