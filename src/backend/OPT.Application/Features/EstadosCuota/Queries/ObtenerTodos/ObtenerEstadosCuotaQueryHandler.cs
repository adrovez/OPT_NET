using MediatR;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.EstadosCuota.Queries.ObtenerTodos;

public sealed class ObtenerEstadosCuotaQueryHandler(IEstadoCuotaRepositorio estadoCuotaRepo)
    : IRequestHandler<ObtenerEstadosCuotaQuery, IReadOnlyList<EstadoCuotaDto>>
{
    public async Task<IReadOnlyList<EstadoCuotaDto>> Handle(ObtenerEstadosCuotaQuery request, CancellationToken ct)
    {
        var estados = await estadoCuotaRepo.ObtenerTodosAsync(ct);
        return estados.Select(e => new EstadoCuotaDto(e.Id, e.Nombre)).ToList();
    }
}
