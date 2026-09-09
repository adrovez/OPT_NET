using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Clientes.Commands.Eliminar;

public sealed class EliminarClienteCommandHandler(
    IClienteRepositorio clienteRepo,
    ICurrentUserService currentUser,
    IUnitOfWork         uow)
    : IRequestHandler<EliminarClienteCommand>
{
    public async Task Handle(EliminarClienteCommand request, CancellationToken ct)
    {
        var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Cliente", request.PublicId);

        cliente.Eliminar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
