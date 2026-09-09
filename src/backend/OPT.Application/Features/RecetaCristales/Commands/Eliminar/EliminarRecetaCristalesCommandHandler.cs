using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.RecetaCristales.Commands.Eliminar;

public sealed class EliminarRecetaCristalesCommandHandler(
    IRecetaCristalesRepositorio recetaRepo,
    ICurrentUserService         currentUser,
    IUnitOfWork                 uow)
    : IRequestHandler<EliminarRecetaCristalesCommand>
{
    public async Task Handle(EliminarRecetaCristalesCommand request, CancellationToken ct)
    {
        var receta = await recetaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("RecetaCristales", request.PublicId);

        receta.Eliminar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
