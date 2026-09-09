using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Empresas.Commands.Eliminar;

public sealed class EliminarEmpresaCommandHandler(
    IEmpresaRepositorio empresaRepo,
    ICurrentUserService  currentUser,
    IUnitOfWork          uow)
    : IRequestHandler<EliminarEmpresaCommand>
{
    public async Task Handle(EliminarEmpresaCommand request, CancellationToken ct)
    {
        var empresa = await empresaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Empresa", request.PublicId);

        empresa.Eliminar(currentUser.UsuarioId);
        await uow.CommitAsync(ct);
    }
}
