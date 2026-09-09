namespace OPT.Application.Features.Clientes;

public record ClienteDto(
    Guid PublicId, string Rut, string Nombre, string Apellido, string? Email,
    string? Telefono, string? Direccion, int? ComunaId,
    DateOnly? FechaNacimiento, string? TipoPrevision);
