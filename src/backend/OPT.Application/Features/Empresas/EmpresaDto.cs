namespace OPT.Application.Features.Empresas;

public record EmpresaDto(
    Guid PublicId, string Nombre, string Rut, string RazonSocial, string Giro,
    string Direccion, string Telefono, string Email, string Contacto);
