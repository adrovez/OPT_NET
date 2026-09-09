using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.RecetaCristales.Commands.Actualizar;

public sealed class ActualizarRecetaCristalesCommandHandler(
    IRecetaCristalesRepositorio recetaRepo,
    ICurrentUserService         currentUser,
    IUnitOfWork                 uow)
    : IRequestHandler<ActualizarRecetaCristalesCommand, RecetaCristalesDto>
{
    public async Task<RecetaCristalesDto> Handle(ActualizarRecetaCristalesCommand request, CancellationToken ct)
    {
        var receta = await recetaRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("RecetaCristales", request.PublicId);

        receta.Actualizar(
            request.OdEsferaLejos, request.OdCilindroLejos, request.OdEjeLejos,
            request.OdEsferaCerca, request.OdCilindroCerca, request.OdEjeCerca,
            request.OiEsferaLejos, request.OiCilindroLejos, request.OiEjeLejos,
            request.OiEsferaCerca, request.OiCilindroCerca, request.OiEjeCerca,
            request.Urgente, request.RequiereLab, request.Observaciones,
            request.DpLejos, request.DpCerca, request.AddLejos,
            request.IncluirLejos, request.ObservacionOdLejos, request.ObservacionOiLejos, request.ObservacionDpLejos,
            request.IncluirCerca, request.ObservacionOdCerca, request.ObservacionOiCerca, request.ObservacionDpCerca,
            currentUser.UsuarioId);
        await uow.CommitAsync(ct);

        return RecetaCristalesMapper.Mapear(receta, receta.Cliente!.PublicId);
    }
}
