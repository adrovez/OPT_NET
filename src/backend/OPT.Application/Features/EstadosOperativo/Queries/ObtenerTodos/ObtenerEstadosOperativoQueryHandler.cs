using MediatR;
using OPT.Domain.Interfaces.Repositories;
using EstadosOperativoDominio = OPT.Domain.Entities.Operativo.EstadosOperativo;

namespace OPT.Application.Features.EstadosOperativo.Queries.ObtenerTodos;

public sealed class ObtenerEstadosOperativoQueryHandler(IEstadoOperativoRepositorio estadoRepo)
    : IRequestHandler<ObtenerEstadosOperativoQuery, IReadOnlyList<EstadoOperativoDto>>
{
    public async Task<IReadOnlyList<EstadoOperativoDto>> Handle(
        ObtenerEstadosOperativoQuery request, CancellationToken ct)
    {
        var estados = await estadoRepo.ObtenerTodosAsync(ct);
        return estados
            .Select(e => new EstadoOperativoDto(e.Id, e.Nombre,
                EstadosOperativoDominio.EsTerminal(e.Id),
                EstadosOperativoDominio.PuedeAnularseDesde(e.Id)))
            .ToList();
    }
}
