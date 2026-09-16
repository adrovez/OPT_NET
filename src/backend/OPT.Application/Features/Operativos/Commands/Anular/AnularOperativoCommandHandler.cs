using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Commands.Anular;

public sealed class AnularOperativoCommandHandler(
    IOperativoRepositorio operativoRepo,
    OperativoDtoFactory   dtoFactory,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<AnularOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(AnularOperativoCommand request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        operativo.Anular(request.Motivo, currentUser.UsuarioId);

        operativoRepo.Actualizar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
