using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Empresas.Commands.Crear;

public sealed class CrearEmpresaCommandHandler(
    IEmpresaRepositorio empresaRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<CrearEmpresaCommand, EmpresaDto>
{
    public async Task<EmpresaDto> Handle(CrearEmpresaCommand request, CancellationToken ct)
    {
        if (await empresaRepo.ExisteRutAsync(request.Rut, ct: ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Rut"] = ["Ya existe una empresa con este RUT."]
            });

        var empresa = Empresa.Crear(
            request.Nombre, request.Rut, request.RazonSocial, request.Giro,
            request.Direccion, request.Telefono, request.Email, request.Contacto,
            currentUser.UsuarioId);

        empresaRepo.Agregar(empresa);
        await uow.CommitAsync(ct);

        return new EmpresaDto(empresa.PublicId, empresa.Nombre, empresa.Rut, empresa.RazonSocial,
            empresa.Giro, empresa.Direccion, empresa.Telefono, empresa.Email, empresa.Contacto);
    }
}
