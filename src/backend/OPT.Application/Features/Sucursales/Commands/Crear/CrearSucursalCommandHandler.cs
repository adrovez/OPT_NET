using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Sucursales.Commands.Crear;

public sealed class CrearSucursalCommandHandler(
    ISucursalRepositorio sucursalRepo,
    IBodegaRepositorio    bodegaRepo,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<CrearSucursalCommand, SucursalDto>
{
    public async Task<SucursalDto> Handle(CrearSucursalCommand request, CancellationToken ct)
    {
        if (request.EsMatriz && await sucursalRepo.ExisteAsync(s => s.EsMatriz, ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["EsMatriz"] = ["Ya existe una sucursal Matriz activa."]
            });

        var sucursal = Sucursal.Crear(
            request.Nombre, request.EsMatriz, currentUser.UsuarioId,
            request.Direccion, request.Telefono);

        // Toda sucursal tiene al menos una bodega (ADR 0012): se crea la principal junto con ella.
        // Hacen falta dos guardados (la bodega necesita el Id generado de la sucursal), así que
        // van en una transacción: o quedan las dos o no queda ninguna.
        await uow.EjecutarEnTransaccionAsync(async token =>
        {
            sucursalRepo.Agregar(sucursal);
            await uow.CommitAsync(token);

            var nombreBodega = $"Bodega {sucursal.Nombre}";
            bodegaRepo.Agregar(Bodega.Crear(
                sucursal.Id,
                $"BOD-{sucursal.Id:00}-01",
                nombreBodega.Length > 100 ? nombreBodega[..100] : nombreBodega,
                esPrincipal: true,
                currentUser.UsuarioId));
            await uow.CommitAsync(token);
        }, ct);

        return new SucursalDto(sucursal.Id, sucursal.Nombre, sucursal.Direccion, sucursal.Telefono, sucursal.EsMatriz);
    }
}
