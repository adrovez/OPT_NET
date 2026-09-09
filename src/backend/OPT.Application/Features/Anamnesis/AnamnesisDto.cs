namespace OPT.Application.Features.Anamnesis;

public record AnamnesisDto(
    Guid PublicId, Guid ClientePublicId, bool Hipertension, bool Diabetes, bool Alergias,
    string? DetalleAlergias, bool UsaLentesPrevio, string? Observaciones,
    DateTimeOffset FechaRegistro);
