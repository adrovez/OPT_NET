using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.RegistrarPago;

public sealed class RegistrarPagoCommandHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    IFormaPagoRepositorio      formaPagoRepo,
    OrdenDeTrabajoDtoFactory   dtoFactory,
    ICurrentUserService        currentUser,
    IUnitOfWork                uow)
    : IRequestHandler<RegistrarPagoCommand, OrdenDeTrabajoDto>
{
    public async Task<OrdenDeTrabajoDto> Handle(RegistrarPagoCommand request, CancellationToken ct)
    {
        var orden = await ordenRepo.ObtenerCompletaPorPublicIdAsync(request.OrdenPublicId, ct)
            ?? throw new NotFoundException("Orden de Trabajo", request.OrdenPublicId);

        if (!await formaPagoRepo.ExisteAsync(request.FormaPagoId, ct))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["FormaPagoId"] = ["La forma de pago indicada no existe."]
            });

        // El dominio imputa las cuotas y recalcula el saldo dentro de la misma operación.
        orden.RegistrarPago(request.Monto, request.FormaPagoId, currentUser.UsuarioId,
            request.FechaPago, request.Referencia);

        ordenRepo.Actualizar(orden);
        await uow.CommitAsync(ct);

        return await dtoFactory.CrearAsync(orden, ct);
    }
}
