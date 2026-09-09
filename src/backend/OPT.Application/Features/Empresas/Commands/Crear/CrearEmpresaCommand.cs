using MediatR;
using OPT.Application.Features.Empresas;

namespace OPT.Application.Features.Empresas.Commands.Crear;

public record CrearEmpresaCommand(
    string Nombre, string Rut, string RazonSocial, string Giro,
    string Direccion, string Telefono, string Email, string Contacto) : IRequest<EmpresaDto>;
