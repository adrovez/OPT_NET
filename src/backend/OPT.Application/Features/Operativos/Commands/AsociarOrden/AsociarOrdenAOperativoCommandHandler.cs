using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Commands.AsociarOrden;

public sealed class AsociarOrdenAOperativoCommandHandler(
    IOperativoRepositorio     operativoRepo,
    IOrdenDeTrabajoRepositorio ordenRepo,
    OperativoDtoFactory       dtoFactory,
    ICurrentUserService       currentUser,
    IUnitOfWork               uow)
    : IRequestHandler<AsociarOrdenAOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(AsociarOrdenAOperativoCommand request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        var orden = await ordenRepo.ObtenerPorPublicIdAsync(request.OrdenPublicId, ct)
            ?? throw new ValidationException(new Dictionary<string, string[]>
            {
                ["OrdenPublicId"] = ["La Orden de Trabajo indicada no existe."]
            });

        // Regla del requerimiento (sección 4.2): una OT con Operativo asociado debe tener
        // Empresa asociada — sin empresa no hay a qué jornada/cliente convenio imputarla.
        if (orden.EmpresaId is null)
            throw new DomainException(
                "La Orden de Trabajo no tiene una Empresa asociada; no puede vincularse a un Operativo.");

        // Una OT pertenece a lo sumo un Operativo — el índice único de la BD lo refuerza,
        // pero se valida acá para dar un mensaje de negocio en vez de un 500 por violación de constraint.
        if (await operativoRepo.OrdenYaAsociadaAsync(orden.Id, ct))
            throw new DomainException("Esa Orden de Trabajo ya está asociada a un Operativo.");

        operativo.AsociarOrden(orden.Id, orden.Precio, orden.TotalAbonado, currentUser.UsuarioId);

        operativoRepo.Actualizar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
