using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Anamnesis.Queries.ObtenerPorId;

public sealed class ObtenerAnamnesisPorPublicIdQueryHandler(IAnamnesisRepositorio anamnesisRepo)
    : IRequestHandler<ObtenerAnamnesisPorPublicIdQuery, AnamnesisDto>
{
    public async Task<AnamnesisDto> Handle(ObtenerAnamnesisPorPublicIdQuery request, CancellationToken ct)
    {
        var anamnesis = await anamnesisRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Anamnesis", request.PublicId);

        return new AnamnesisDto(anamnesis.PublicId, anamnesis.Cliente!.PublicId, anamnesis.Hipertension,
            anamnesis.Diabetes, anamnesis.Alergias, anamnesis.DetalleAlergias,
            anamnesis.UsaLentesPrevio, anamnesis.Observaciones, anamnesis.CreadoEn);
    }
}
