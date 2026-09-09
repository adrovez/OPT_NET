using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Anamnesis.Queries.ObtenerPorCliente;

public sealed class ObtenerAnamnesisPorClienteQueryHandler(
    IAnamnesisRepositorio anamnesisRepo,
    IClienteRepositorio   clienteRepo)
    : IRequestHandler<ObtenerAnamnesisPorClienteQuery, IReadOnlyList<AnamnesisDto>>
{
    public async Task<IReadOnlyList<AnamnesisDto>> Handle(ObtenerAnamnesisPorClienteQuery request, CancellationToken ct)
    {
        var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.ClientePublicId, ct)
            ?? throw new NotFoundException("Cliente", request.ClientePublicId);

        var registros = await anamnesisRepo.ObtenerPorClienteAsync(cliente.Id, ct);

        return registros
            .Select(a => new AnamnesisDto(a.PublicId, cliente.PublicId, a.Hipertension,
                a.Diabetes, a.Alergias, a.DetalleAlergias, a.UsaLentesPrevio, a.Observaciones,
                a.CreadoEn))
            .ToList();
    }
}
