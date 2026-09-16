using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.GenerarPlanCuotas;

public sealed class GenerarPlanCuotasCommandHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<GenerarPlanCuotasCommand, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(GenerarPlanCuotasCommand request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.OrdenPublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.OrdenPublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, orden.SucursalId);

        orden.GenerarPlanCuotas(request.NumeroCuotas, request.PrimerVencimiento,
            currentUser.UsuarioId);

        ordenRepo.Actualizar(orden);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
