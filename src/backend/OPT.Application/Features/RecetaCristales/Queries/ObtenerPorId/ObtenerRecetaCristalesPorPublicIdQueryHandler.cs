using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.RecetaCristales.Queries.ObtenerPorId;

public sealed class ObtenerRecetaCristalesPorPublicIdQueryHandler(IRecetaCristalesRepositorio recetaRepo)
    : IRequestHandler<ObtenerRecetaCristalesPorPublicIdQuery, RecetaCristalesDto>
{
    public async Task<RecetaCristalesDto> Handle(ObtenerRecetaCristalesPorPublicIdQuery request, CancellationToken ct)
    {
        var receta = await recetaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("RecetaCristales", request.PublicId);

        return RecetaCristalesMapper.Mapear(receta, receta.Cliente!.PublicId);
    }
}
