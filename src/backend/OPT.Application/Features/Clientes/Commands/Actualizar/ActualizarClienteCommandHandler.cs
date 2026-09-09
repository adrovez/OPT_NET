using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Clientes.Commands.Actualizar;

public sealed class ActualizarClienteCommandHandler(
    IClienteRepositorio clienteRepo,
    IComunaRepositorio  comunaRepo,
    ICurrentUserService currentUser,
    IUnitOfWork         uow)
    : IRequestHandler<ActualizarClienteCommand, ClienteDto>
{
    public async Task<ClienteDto> Handle(ActualizarClienteCommand request, CancellationToken ct)
    {
        var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Cliente", request.PublicId);

        if (request.ComunaId is not null && !await comunaRepo.ExisteAsync(request.ComunaId.Value, ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["ComunaId"] = ["La comuna indicada no existe."]
            });

        cliente.Actualizar(request.Nombre, request.Apellido, request.Email, request.Telefono,
            request.Direccion, request.ComunaId, currentUser.UsuarioId,
            request.FechaNacimiento, request.TipoPrevision);
        await uow.CommitAsync(ct);

        return new ClienteDto(cliente.PublicId, cliente.Rut, cliente.Nombre, cliente.Apellido,
            cliente.Email, cliente.Telefono, cliente.Direccion, cliente.ComunaId,
            cliente.FechaNacimiento, cliente.TipoPrevision);
    }
}
