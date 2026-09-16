using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Commands.CambiarEstado;

public sealed class CambiarEstadoOperativoCommandHandler(
    IOperativoRepositorio      operativoRepo,
    IEstadoOperativoRepositorio estadoRepo,
    OperativoDtoFactory        dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<CambiarEstadoOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(CambiarEstadoOperativoCommand request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        if (!await estadoRepo.ExisteAsync(request.NuevoEstadoId, ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["NuevoEstadoId"] = ["El estado indicado no existe."]
            });

        // Las reglas de transición (avance de a un paso, terminales) viven en el dominio.
        operativo.CambiarEstado(request.NuevoEstadoId, currentUser.UsuarioId);

        operativoRepo.Actualizar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
