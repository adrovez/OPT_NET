using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;
using EntidadOperativo = OPT.Domain.Entities.Operativo.Operativo;

namespace OPT.Application.Features.Operativos.Commands.Crear;

public sealed class CrearOperativoCommandHandler(
    IOperativoRepositorio  operativoRepo,
    IEmpresaRepositorio    empresaRepo,
    ISucursalRepositorio   sucursalRepo,
    OperativoDtoFactory    dtoFactory,
    ICurrentUserService    currentUser,
    IUnitOfWork            uow)
    : IRequestHandler<CrearOperativoCommand, OperativoDto>
{
    public async Task<OperativoDto> Handle(CrearOperativoCommand request, CancellationToken ct)
    {
        var errores = new Dictionary<string, string[]>();

        var empresa = await empresaRepo.ObtenerPorPublicIdAsync(request.EmpresaPublicId, ct);
        if (empresa is null) errores["EmpresaPublicId"] = ["La empresa indicada no existe."];

        var sucursal = await sucursalRepo.ObtenerPorIdAsync(request.SucursalId, ct);
        if (sucursal is null) errores["SucursalId"] = ["La sucursal indicada no existe."];

        if (errores.Count > 0) throw new ValidationException(errores);

        // BOLA/IDOR: recién acá, para que un SucursalId inexistente siga dando el error de
        // validación de arriba en vez de un 403 confuso (mismo criterio que CrearOrdenDeTrabajo).
        AutorizacionSucursal.ValidarAcceso(currentUser, request.SucursalId);

        var operativo = EntidadOperativo.Crear(
            request.Nombre, empresa!.Id, request.SucursalId, request.Fecha,
            currentUser.UsuarioId, request.Observacion,
            request.NombreContacto, request.MailContacto, request.TelefonoContacto);

        operativoRepo.Agregar(operativo);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
