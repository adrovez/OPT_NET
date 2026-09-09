using EntidadRecetaCristales = OPT.Domain.Entities.Clinico.RecetaCristales;

namespace OPT.Application.Features.RecetaCristales;

internal static class RecetaCristalesMapper
{
    public static RecetaCristalesDto Mapear(EntidadRecetaCristales r, Guid clientePublicId) => new(
        r.PublicId, clientePublicId,
        r.OdEsferaLejos, r.OdCilindroLejos, r.OdEjeLejos,
        r.OdEsferaCerca, r.OdCilindroCerca, r.OdEjeCerca,
        r.OiEsferaLejos, r.OiCilindroLejos, r.OiEjeLejos,
        r.OiEsferaCerca, r.OiCilindroCerca, r.OiEjeCerca,
        r.Urgente, r.RequiereLab, r.Observaciones, r.DpLejos, r.DpCerca, r.AddLejos,
        r.IncluirLejos, r.ObservacionOdLejos, r.ObservacionOiLejos, r.ObservacionDpLejos,
        r.IncluirCerca, r.ObservacionOdCerca, r.ObservacionOiCerca, r.ObservacionDpCerca,
        r.CreadoEn);
}
