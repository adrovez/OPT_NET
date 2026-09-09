using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Sucursales.Commands.Actualizar;

public sealed class ActualizarSucursalCommandHandler(
    ISucursalRepositorio sucursalRepo,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<ActualizarSucursalCommand, SucursalDto>
{
    public async Task<SucursalDto> Handle(ActualizarSucursalCommand request, CancellationToken ct)
    {
        var sucursal = await sucursalRepo.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Sucursal", request.Id);

        sucursal.Actualizar(request.Nombre, request.Direccion, request.Telefono, currentUser.UsuarioId);
        await uow.CommitAsync(ct);

        return new SucursalDto(sucursal.Id, sucursal.Nombre, sucursal.Direccion, sucursal.Telefono, sucursal.EsMatriz);
    }
}
