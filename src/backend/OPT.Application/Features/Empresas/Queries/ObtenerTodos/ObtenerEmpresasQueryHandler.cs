using MediatR;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Empresas.Queries.ObtenerTodos;

public sealed class ObtenerEmpresasQueryHandler(IEmpresaRepositorio empresaRepo)
    : IRequestHandler<ObtenerEmpresasQuery, PagedResult<EmpresaDto>>
{
    public async Task<PagedResult<EmpresaDto>> Handle(ObtenerEmpresasQuery request, CancellationToken ct)
    {
        var (items, total) = await empresaRepo.BuscarPaginadoAsync(request, ct);

        return PagedResultFactory.Crear(items, total, request, e => new EmpresaDto(
            e.PublicId, e.Nombre, e.Rut, e.RazonSocial, e.Giro, e.Direccion, e.Telefono, e.Email, e.Contacto));
    }
}
