using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Bodegas.Commands.Actualizar;

public sealed class ActualizarBodegaCommandHandler(
    IBodegaRepositorio  bodegaRepo,
    ICurrentUserService currentUser,
    IUnitOfWork         uow)
    : IRequestHandler<ActualizarBodegaCommand, BodegaDto>
{
    public async Task<BodegaDto> Handle(ActualizarBodegaCommand request, CancellationToken ct)
    {
        var bodega = await bodegaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Bodega", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, bodega.SucursalId);

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await bodegaRepo.ExisteCodigoAsync(bodega.SucursalId, codigo, excluirId: bodega.Id, ct: ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Codigo"] = ["Ya existe otra bodega con este código en la sucursal."]
            });

        bodega.Actualizar(codigo, request.Nombre, currentUser.UsuarioId);
        await uow.CommitAsync(ct);

        return BodegaDto.Desde(bodega);
    }
}
