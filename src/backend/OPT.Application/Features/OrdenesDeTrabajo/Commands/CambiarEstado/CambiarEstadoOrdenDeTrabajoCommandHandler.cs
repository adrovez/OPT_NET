using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.CambiarEstado;

public sealed class CambiarEstadoOrdenDeTrabajoCommandHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    IEstadoOTRepositorio       estadoRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<CambiarEstadoOrdenDeTrabajoCommand, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(CambiarEstadoOrdenDeTrabajoCommand request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.PublicId);

        if (!await estadoRepo.ExisteAsync(request.NuevoEstadoId, ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["NuevoEstadoId"] = ["El estado indicado no existe."]
            });

        // Las reglas de transición (avance/retroceso de a un paso, terminales, observación
        // obligatoria al retroceder) viven en el dominio, no aquí.
        orden.CambiarEstado(request.NuevoEstadoId, currentUser.UsuarioId, request.Observacion);

        ordenRepo.Actualizar(orden);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
