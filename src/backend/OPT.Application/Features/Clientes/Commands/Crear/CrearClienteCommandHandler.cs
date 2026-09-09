using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Clientes.Commands.Crear;

public sealed class CrearClienteCommandHandler(
    IClienteRepositorio clienteRepo,
    IComunaRepositorio  comunaRepo,
    ICurrentUserService currentUser,
    IUnitOfWork         uow)
    : IRequestHandler<CrearClienteCommand, ClienteDto>
{
    public async Task<ClienteDto> Handle(CrearClienteCommand request, CancellationToken ct)
    {
        var errores = new Dictionary<string, string[]>();

        if (await clienteRepo.ExisteRutAsync(request.Rut, ct: ct))
            errores["Rut"] = ["Ya existe un cliente con este RUT."];

        if (request.ComunaId is not null && !await comunaRepo.ExisteAsync(request.ComunaId.Value, ct))
            errores["ComunaId"] = ["La comuna indicada no existe."];

        if (errores.Count > 0) throw new ValidationException(errores);

        var cliente = Cliente.Crear(
            request.Rut, request.Nombre, request.Apellido, currentUser.UsuarioId,
            request.Email, request.Telefono, request.Direccion, request.ComunaId,
            request.FechaNacimiento, request.TipoPrevision);

        clienteRepo.Agregar(cliente);
        await uow.CommitAsync(ct);

        return new ClienteDto(cliente.PublicId, cliente.Rut, cliente.Nombre, cliente.Apellido,
            cliente.Email, cliente.Telefono, cliente.Direccion, cliente.ComunaId,
            cliente.FechaNacimiento, cliente.TipoPrevision);
    }
}
