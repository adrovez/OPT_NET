using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Sucursales.Queries.ObtenerPorId;

public sealed class ObtenerSucursalPorIdQueryHandler(ISucursalRepositorio sucursalRepo)
    : IRequestHandler<ObtenerSucursalPorIdQuery, SucursalDto>
{
    public async Task<SucursalDto> Handle(ObtenerSucursalPorIdQuery request, CancellationToken ct)
    {
        var sucursal = await sucursalRepo.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Sucursal", request.Id);

        return new SucursalDto(sucursal.Id, sucursal.Nombre, sucursal.Direccion, sucursal.Telefono, sucursal.EsMatriz);
    }
}
