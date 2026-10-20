using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Bodegas.Commands.Eliminar;

public sealed class EliminarBodegaCommandHandler(
    IBodegaRepositorio  bodegaRepo,
    ICurrentUserService currentUser,
    IUnitOfWork         uow)
    : IRequestHandler<EliminarBodegaCommand>
{
    public async Task Handle(EliminarBodegaCommand request, CancellationToken ct)
    {
        var bodega = await bodegaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Bodega", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, bodega.SucursalId);

        if (bodega.EsPrincipal)
            throw new DomainException("No se puede eliminar la bodega principal: designe otra primero.");
        if (await bodegaRepo.ObtenerStockTotalAsync(bodega.Id, ct) > 0)
            throw new DomainException("No se puede eliminar una bodega con stock.");

        bodega.Eliminar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
