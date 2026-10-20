using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Bodegas.Commands.MarcarPrincipal;

public sealed class MarcarBodegaPrincipalCommandHandler(
    IBodegaRepositorio  bodegaRepo,
    ICurrentUserService currentUser,
    IUnitOfWork         uow)
    : IRequestHandler<MarcarBodegaPrincipalCommand, BodegaDto>
{
    public async Task<BodegaDto> Handle(MarcarBodegaPrincipalCommand request, CancellationToken ct)
    {
        var bodega = await bodegaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Bodega", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, bodega.SucursalId);

        if (bodega.EsPrincipal) return BodegaDto.Desde(bodega);

        // Quitar la marca a la anterior y ponérsela a esta, en una sola transacción
        // (el índice único filtrado no admite dos principales ni siquiera de paso).
        await uow.EjecutarEnTransaccionAsync(async token =>
        {
            var actual = await bodegaRepo.ObtenerPrincipalAsync(bodega.SucursalId, token);
            if (actual is not null)
            {
                actual.QuitarPrincipal(currentUser.UsuarioId);
                await uow.CommitAsync(token);
            }

            bodega.MarcarPrincipal(currentUser.UsuarioId);
            await uow.CommitAsync(token);
        }, ct);

        return BodegaDto.Desde(bodega);
    }
}
