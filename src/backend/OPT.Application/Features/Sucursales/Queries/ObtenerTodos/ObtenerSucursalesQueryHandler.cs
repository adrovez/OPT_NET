using MediatR;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Sucursales.Queries.ObtenerTodos;

public sealed class ObtenerSucursalesQueryHandler(ISucursalRepositorio sucursalRepo)
    : IRequestHandler<ObtenerSucursalesQuery, PagedResult<SucursalDto>>
{
    public async Task<PagedResult<SucursalDto>> Handle(ObtenerSucursalesQuery request, CancellationToken ct)
    {
        var (items, total) = await sucursalRepo.BuscarPaginadoAsync(request, ct);

        return PagedResultFactory.Crear(items, total, request, s => new SucursalDto(
            s.Id, s.Nombre, s.Direccion, s.Telefono, s.EsMatriz));
    }
}
