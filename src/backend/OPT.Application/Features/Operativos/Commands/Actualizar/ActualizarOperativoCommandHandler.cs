using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Commands.Actualizar;

public sealed class ActualizarOperativoCommandHandler(
    IOperativoRepositorio operativoRepo,
    OperativoDtoFactory   dtoFactory,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<ActualizarOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(ActualizarOperativoCommand request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        operativo.Actualizar(request.Nombre, request.Fecha, request.Observacion, currentUser.UsuarioId,
            request.NombreContacto, request.MailContacto, request.TelefonoContacto);

        operativoRepo.Actualizar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
