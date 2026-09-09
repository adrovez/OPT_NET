using MediatR;
using OPT.Application.Features.Empresas;

namespace OPT.Application.Features.Empresas.Commands.Actualizar;

public record ActualizarEmpresaCommand(
    Guid PublicId, string Nombre, string Rut, string RazonSocial, string Giro,
    string Direccion, string Telefono, string Email, string Contacto) : IRequest<EmpresaDto>;
