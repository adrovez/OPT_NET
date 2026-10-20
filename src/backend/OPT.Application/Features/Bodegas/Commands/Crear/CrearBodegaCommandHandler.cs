using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Bodegas.Commands.Crear;

public sealed class CrearBodegaCommandHandler(
    IBodegaRepositorio   bodegaRepo,
    ISucursalRepositorio sucursalRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<CrearBodegaCommand, BodegaDto>
{
    public async Task<BodegaDto> Handle(CrearBodegaCommand request, CancellationToken ct)
    {
        _ = await sucursalRepo.ObtenerPorIdAsync(request.SucursalId, ct)
            ?? throw new NotFoundException("Sucursal", request.SucursalId);

        AutorizacionSucursal.ValidarAcceso(currentUser, request.SucursalId);

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await bodegaRepo.ExisteCodigoAsync(request.SucursalId, codigo, ct: ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Codigo"] = ["Ya existe una bodega con este código en la sucursal."]
            });

        var yaTieneBodegas = await bodegaRepo.SucursalTieneBodegasAsync(request.SucursalId, ct);
        var esPrincipal    = request.EsPrincipal || !yaTieneBodegas;

        var bodega = Bodega.Crear(request.SucursalId, codigo, request.Nombre, esPrincipal, currentUser.UsuarioId);

        // El índice único de "una sola principal" obliga a quitar la marca a la anterior
        // ANTES de insertar la nueva: dos guardados, una transacción.
        await uow.EjecutarEnTransaccionAsync(async token =>
        {
            if (esPrincipal && yaTieneBodegas)
            {
                var actual = await bodegaRepo.ObtenerPrincipalAsync(request.SucursalId, token);
                if (actual is not null)
                {
                    actual.QuitarPrincipal(currentUser.UsuarioId);
                    await uow.CommitAsync(token);
                }
            }

            bodegaRepo.Agregar(bodega);
            await uow.CommitAsync(token);
        }, ct);

        return BodegaDto.Desde(bodega);
    }
}
