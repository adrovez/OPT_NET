using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Bodegas.Commands.Desactivar;

public sealed class DesactivarBodegaCommandHandler(
    IBodegaRepositorio  bodegaRepo,
    ICurrentUserService currentUser,
    IUnitOfWork         uow)
    : IRequestHandler<DesactivarBodegaCommand, BodegaDto>
{
    public async Task<BodegaDto> Handle(DesactivarBodegaCommand request, CancellationToken ct)
    {
        var bodega = await bodegaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Bodega", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, bodega.SucursalId);

        var stock = await bodegaRepo.ObtenerStockTotalAsync(bodega.Id, ct);
        bodega.Desactivar(stock, currentUser.UsuarioId);   // rechaza la principal y las que tienen stock
        await uow.CommitAsync(ct);

        return BodegaDto.Desde(bodega);
    }
}
