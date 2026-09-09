using MediatR;
using OPT.Domain.Interfaces.Repositories;
using EstadosOTDominio = OPT.Domain.Entities.Comercial.EstadosOT;

namespace OPT.Application.Features.EstadosOT.Queries.ObtenerTodos;

public sealed class ObtenerEstadosOTQueryHandler(IEstadoOTRepositorio estadoRepo)
    : IRequestHandler<ObtenerEstadosOTQuery, IReadOnlyList<EstadoOTDto>>
{
    public async Task<IReadOnlyList<EstadoOTDto>> Handle(ObtenerEstadosOTQuery request, CancellationToken ct)
    {
        var estados = await estadoRepo.ObtenerTodosAsync(ct);
        return estados
            .Select(e => new EstadoOTDto(e.Id, e.Nombre, EstadosOTDominio.EsTerminal(e.Id)))
            .ToList();
    }
}
