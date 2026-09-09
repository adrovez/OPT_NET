using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;
using EntidadRecetaCristales = OPT.Domain.Entities.Clinico.RecetaCristales;

namespace OPT.Application.Features.RecetaCristales.Commands.Crear;

public sealed class CrearRecetaCristalesCommandHandler(
    IRecetaCristalesRepositorio recetaRepo,
    IClienteRepositorio         clienteRepo,
    ICurrentUserService         currentUser,
    IUnitOfWork                 uow)
    : IRequestHandler<CrearRecetaCristalesCommand, RecetaCristalesDto>
{
    public async Task<RecetaCristalesDto> Handle(CrearRecetaCristalesCommand request, CancellationToken ct)
    {
        var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.ClientePublicId, ct)
            ?? throw new NotFoundException("Cliente", request.ClientePublicId);

        var receta = EntidadRecetaCristales.Crear(
            cliente.Id, request.Urgente, request.RequiereLab, request.Observaciones, currentUser.UsuarioId);

        receta.SetOjoDerecho(request.OdEsferaLejos, request.OdCilindroLejos, request.OdEjeLejos,
            request.OdEsferaCerca, request.OdCilindroCerca, request.OdEjeCerca);
        receta.SetOjoIzquierdo(request.OiEsferaLejos, request.OiCilindroLejos, request.OiEjeLejos,
            request.OiEsferaCerca, request.OiCilindroCerca, request.OiEjeCerca);
        receta.SetDpAdd(request.DpLejos, request.DpCerca, request.AddLejos);
        receta.SetInclusionLejos(request.IncluirLejos, request.ObservacionOdLejos, request.ObservacionOiLejos, request.ObservacionDpLejos);
        receta.SetInclusionCerca(request.IncluirCerca, request.ObservacionOdCerca, request.ObservacionOiCerca, request.ObservacionDpCerca);

        recetaRepo.Agregar(receta);
        await uow.CommitAsync(ct);

        return RecetaCristalesMapper.Mapear(receta, cliente.PublicId);
    }
}
