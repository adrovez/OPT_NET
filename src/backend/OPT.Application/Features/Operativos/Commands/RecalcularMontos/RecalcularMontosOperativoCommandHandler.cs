using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Commands.RecalcularMontos;

public sealed class RecalcularMontosOperativoCommandHandler(
    IOperativoRepositorio      operativoRepo,
    IOrdenDeTrabajoRepositorio ordenRepo,
    OperativoDtoFactory        dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<RecalcularMontosOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(RecalcularMontosOperativoCommand request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        var ordenIds = operativo.Ordenes.Select(o => o.OrdenDeTrabajoId).ToList();
        var ordenes  = await ordenRepo.BuscarAsync(o => ordenIds.Contains(o.Id), ct);

        var montosPorOrden = ordenes.ToDictionary(o => o.Id, o => (o.Precio, o.TotalAbonado));

        operativo.RecalcularMontosDesdeOT(montosPorOrden, currentUser.UsuarioId);

        operativoRepo.Actualizar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
