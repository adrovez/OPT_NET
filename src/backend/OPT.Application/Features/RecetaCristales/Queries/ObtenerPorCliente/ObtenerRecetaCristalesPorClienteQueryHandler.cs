using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.RecetaCristales.Queries.ObtenerPorCliente;

public sealed class ObtenerRecetaCristalesPorClienteQueryHandler(
    IRecetaCristalesRepositorio recetaRepo,
    IClienteRepositorio         clienteRepo)
    : IRequestHandler<ObtenerRecetaCristalesPorClienteQuery, IReadOnlyList<RecetaCristalesDto>>
{
    public async Task<IReadOnlyList<RecetaCristalesDto>> Handle(ObtenerRecetaCristalesPorClienteQuery request, CancellationToken ct)
    {
        var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.ClientePublicId, ct)
            ?? throw new NotFoundException("Cliente", request.ClientePublicId);

        var registros = await recetaRepo.ObtenerPorClienteAsync(cliente.Id, ct);

        return registros
            .Select(r => RecetaCristalesMapper.Mapear(r, cliente.PublicId))
            .ToList();
    }
}
