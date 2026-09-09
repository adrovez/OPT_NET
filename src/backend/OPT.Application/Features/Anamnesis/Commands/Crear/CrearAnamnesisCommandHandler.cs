using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;
using EntidadAnamnesis = OPT.Domain.Entities.Clinico.Anamnesis;

namespace OPT.Application.Features.Anamnesis.Commands.Crear;

public sealed class CrearAnamnesisCommandHandler(
    IAnamnesisRepositorio anamnesisRepo,
    IClienteRepositorio   clienteRepo,
    ICurrentUserService   currentUser,
    IUnitOfWork           uow)
    : IRequestHandler<CrearAnamnesisCommand, AnamnesisDto>
{
    public async Task<AnamnesisDto> Handle(CrearAnamnesisCommand request, CancellationToken ct)
    {
        var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.ClientePublicId, ct)
            ?? throw new NotFoundException("Cliente", request.ClientePublicId);

        var anamnesis = EntidadAnamnesis.Crear(
            cliente.Id, request.Hipertension, request.Diabetes, request.Alergias,
            request.DetalleAlergias, request.UsaLentesPrevio, request.Observaciones,
            currentUser.UsuarioId);

        anamnesisRepo.Agregar(anamnesis);
        await uow.CommitAsync(ct);

        return new AnamnesisDto(anamnesis.PublicId, cliente.PublicId, anamnesis.Hipertension,
            anamnesis.Diabetes, anamnesis.Alergias, anamnesis.DetalleAlergias,
            anamnesis.UsaLentesPrevio, anamnesis.Observaciones, anamnesis.CreadoEn);
    }
}
