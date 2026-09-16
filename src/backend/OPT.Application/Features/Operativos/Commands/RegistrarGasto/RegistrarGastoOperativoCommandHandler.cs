using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Commands.RegistrarGasto;

public sealed class RegistrarGastoOperativoCommandHandler(
    IOperativoRepositorio operativoRepo,
    OperativoDtoFactory   dtoFactory,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<RegistrarGastoOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(RegistrarGastoOperativoCommand request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        operativo.RegistrarGasto(request.Monto, currentUser.UsuarioId,
            request.NumeroDocumento, request.Observacion);

        operativoRepo.Actualizar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
