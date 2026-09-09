using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Anamnesis.Commands.Eliminar;

public sealed class EliminarAnamnesisCommandHandler(
    IAnamnesisRepositorio anamnesisRepo,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<EliminarAnamnesisCommand>
{
    public async Task Handle(EliminarAnamnesisCommand request, CancellationToken ct)
    {
        var anamnesis = await anamnesisRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Anamnesis", request.PublicId);

        anamnesis.Eliminar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
