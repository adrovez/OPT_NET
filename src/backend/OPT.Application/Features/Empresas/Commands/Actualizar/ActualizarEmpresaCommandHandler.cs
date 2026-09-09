using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Empresas.Commands.Actualizar;

public sealed class ActualizarEmpresaCommandHandler(
    IEmpresaRepositorio empresaRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<ActualizarEmpresaCommand, EmpresaDto>
{
    public async Task<EmpresaDto> Handle(ActualizarEmpresaCommand request, CancellationToken ct)
    {
        var empresa = await empresaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Empresa", request.PublicId);

        if (await empresaRepo.ExisteRutAsync(request.Rut, excluirId: empresa.Id, ct: ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Rut"] = ["Ya existe otra empresa con este RUT."]
            });

        empresa.Actualizar(request.Nombre, request.Rut, request.RazonSocial, request.Giro,
            request.Direccion, request.Telefono, request.Email, request.Contacto, currentUser.UsuarioId);
        await uow.CommitAsync(ct);

        return new EmpresaDto(empresa.PublicId, empresa.Nombre, empresa.Rut, empresa.RazonSocial,
            empresa.Giro, empresa.Direccion, empresa.Telefono, empresa.Email, empresa.Contacto);
    }
}
