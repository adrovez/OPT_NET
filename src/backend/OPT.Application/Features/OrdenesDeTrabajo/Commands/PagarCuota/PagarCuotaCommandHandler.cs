using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.PagarCuota;

public sealed class PagarCuotaCommandHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    IFormaPagoRepositorio      formaPagoRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<PagarCuotaCommand, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(PagarCuotaCommand request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.OrdenPublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.OrdenPublicId);

        if (request.FormaPagoId is not null &&
            !await formaPagoRepo.ExisteAsync(request.FormaPagoId.Value, ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["FormaPagoId"] = ["La forma de pago indicada no existe."]
            });

        orden.PagarCuota(request.Numero, currentUser.UsuarioId, request.FormaPagoId,
            request.FechaPago);

        ordenRepo.Actualizar(orden);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
