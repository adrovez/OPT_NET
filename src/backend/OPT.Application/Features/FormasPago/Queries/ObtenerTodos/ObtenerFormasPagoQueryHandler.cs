using MediatR;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.FormasPago.Queries.ObtenerTodos;

public sealed class ObtenerFormasPagoQueryHandler(IFormaPagoRepositorio formaPagoRepo)
    : IRequestHandler<ObtenerFormasPagoQuery, IReadOnlyList<FormaPagoDto>>
{
    public async Task<IReadOnlyList<FormaPagoDto>> Handle(ObtenerFormasPagoQuery request, CancellationToken ct)
    {
        var formas = await formaPagoRepo.ObtenerTodosAsync(ct);
        return formas.Select(f => new FormaPagoDto(f.Id, f.Nombre)).ToList();
    }
}
