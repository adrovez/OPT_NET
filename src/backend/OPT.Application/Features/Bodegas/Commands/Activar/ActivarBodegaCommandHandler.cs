using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Bodegas.Commands.Activar;

public sealed class ActivarBodegaCommandHandler(
    IBodegaRepositorio  bodegaRepo,
    ICurrentUserService currentUser,
    IUnitOfWork         uow)
    : IRequestHandler<ActivarBodegaCommand, BodegaDto>
{
    public async Task<BodegaDto> Handle(ActivarBodegaCommand request, CancellationToken ct)
    {
        var bodega = await bodegaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Bodega", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, bodega.SucursalId);

        bodega.Activar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);

        return BodegaDto.Desde(bodega);
    }
}
