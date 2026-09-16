using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Application.Common.Security;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Operativos.Queries.ObtenerPorId;

public sealed class ObtenerOperativoPorPublicIdQueryHandler(
    IOperativoRepositorio operativoRepo,
    OperativoDtoFactory   dtoFactory,
    ICurrentUserService   currentUser)
    : IRequestHandler<ObtenerOperativoPorPublicIdQuery, OperativoDto>
{
    public async Task<OperativoDto> Handle(ObtenerOperativoPorPublicIdQuery request, CancellationToken ct)
    {
        var operativo = await operativoRepo.ObtenerCompletaPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Operativo", request.PublicId);

        AutorizacionSucursal.ValidarAcceso(currentUser, operativo.SucursalId);

        return await dtoFactory.CrearAsync(operativo, ct);
    }
}
