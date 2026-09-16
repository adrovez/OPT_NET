using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Commands.QuitarOrden;

public sealed class QuitarOrdenDeOperativoCommandHandler(
    IOperativoRepositorio      operativoRepo,
    IOrdenDeTrabajoRepositorio ordenRepo,
    OperativoDtoFactory        dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<QuitarOrdenDeOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(QuitarOrdenDeOperativoCommand request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        var orden = await ordenRepo.ObtenerPorPublicIdAsync(request.OrdenPublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.OrdenPublicId);

        operativo.QuitarOrden(orden.Id, currentUser.UsuarioId);

        operativoRepo.Actualizar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
