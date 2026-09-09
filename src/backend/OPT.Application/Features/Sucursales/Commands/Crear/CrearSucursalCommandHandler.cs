using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Entities.Organizacion;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Sucursales.Commands.Crear;

public sealed class CrearSucursalCommandHandler(
    ISucursalRepositorio sucursalRepo,
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

        sucursalRepo.Agregar(sucursal);
        await uow.CommitAsync(ct);

        return new SucursalDto(sucursal.Id, sucursal.Nombre, sucursal.Direccion, sucursal.Telefono, sucursal.EsMatriz);
    }
}
