using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Bodegas.Queries.ObtenerPorId;

public sealed class ObtenerBodegaPorPublicIdQueryHandler(
    IBodegaRepositorio  bodegaRepo,
    ICurrentUserService currentUser)
    : IRequestHandler<ObtenerBodegaPorPublicIdQuery, BodegaDto>
{
    public async Task<BodegaDto> Handle(ObtenerBodegaPorPublicIdQuery request, CancellationToken ct)
    {
        var bodega = await bodegaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Bodega", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, bodega.SucursalId);
        return BodegaDto.Desde(bodega);
    }
}
