using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Commands.EliminarGasto;

public sealed class EliminarGastoOperativoCommandHandler(
    IOperativoRepositorio operativoRepo,
    OperativoDtoFactory   dtoFactory,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<EliminarGastoOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(EliminarGastoOperativoCommand request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        operativo.EliminarGasto(request.GastoId, currentUser.UsuarioId);

        operativoRepo.Actualizar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
