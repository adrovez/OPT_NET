using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Empresas.Queries.ObtenerPorId;

public sealed class ObtenerEmpresaPorPublicIdQueryHandler(IEmpresaRepositorio empresaRepo)
    : IRequestHandler<ObtenerEmpresaPorPublicIdQuery, EmpresaDto>
{
    public async Task<EmpresaDto> Handle(ObtenerEmpresaPorPublicIdQuery request, CancellationToken ct)
    {
        var empresa = await empresaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Empresa", request.PublicId);

        return new EmpresaDto(empresa.PublicId, empresa.Nombre, empresa.Rut, empresa.RazonSocial,
            empresa.Giro, empresa.Direccion, empresa.Telefono, empresa.Email, empresa.Contacto);
    }
}
