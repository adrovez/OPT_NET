using FluentValidation;

namespace OPT.Application.Features.RecetaCristales.Commands.Actualizar;

public class ActualizarRecetaCristalesCommandValidator : AbstractValidator<ActualizarRecetaCristalesCommand>
{
    private const decimal MinGraduacion = -30m;
    private const decimal MaxGraduacion = 30m;

    public ActualizarRecetaCristalesCommandValidator()
    {
        RuleFor(x => x.OdEsferaLejos).InclusiveBetween(MinGraduacion, MaxGraduacion).When(x => x.OdEsferaLejos.HasValue);
        RuleFor(x => x.OdCilindroLejos).InclusiveBetween(MinGraduacion, MaxGraduacion).When(x => x.OdCilindroLejos.HasValue);
        RuleFor(x => x.OdEjeLejos).InclusiveBetween(0, 180).When(x => x.OdEjeLejos.HasValue);
        RuleFor(x => x.OdEsferaCerca).InclusiveBetween(MinGraduacion, MaxGraduacion).When(x => x.OdEsferaCerca.HasValue);
        RuleFor(x => x.OdCilindroCerca).InclusiveBetween(MinGraduacion, MaxGraduacion).When(x => x.OdCilindroCerca.HasValue);
        RuleFor(x => x.OdEjeCerca).InclusiveBetween(0, 180).When(x => x.OdEjeCerca.HasValue);

        RuleFor(x => x.OiEsferaLejos).InclusiveBetween(MinGraduacion, MaxGraduacion).When(x => x.OiEsferaLejos.HasValue);
        RuleFor(x => x.OiCilindroLejos).InclusiveBetween(MinGraduacion, MaxGraduacion).When(x => x.OiCilindroLejos.HasValue);
        RuleFor(x => x.OiEjeLejos).InclusiveBetween(0, 180).When(x => x.OiEjeLejos.HasValue);
        RuleFor(x => x.OiEsferaCerca).InclusiveBetween(MinGraduacion, MaxGraduacion).When(x => x.OiEsferaCerca.HasValue);
        RuleFor(x => x.OiCilindroCerca).InclusiveBetween(MinGraduacion, MaxGraduacion).When(x => x.OiCilindroCerca.HasValue);
        RuleFor(x => x.OiEjeCerca).InclusiveBetween(0, 180).When(x => x.OiEjeCerca.HasValue);

        RuleFor(x => x.Observaciones).MaximumLength(500);
        RuleFor(x => x.DpLejos).MaximumLength(20).MedidaNoNegativa("La DP de lejos");
        RuleFor(x => x.DpCerca).MaximumLength(20).MedidaNoNegativa("La DP de cerca");
        RuleFor(x => x.AddLejos).MaximumLength(20).MedidaNoNegativa("El ADD");

        RuleFor(x => x.ObservacionOdLejos).MaximumLength(50);
        RuleFor(x => x.ObservacionOiLejos).MaximumLength(50);
        RuleFor(x => x.ObservacionDpLejos).MaximumLength(50);
        RuleFor(x => x.ObservacionOdCerca).MaximumLength(50);
        RuleFor(x => x.ObservacionOiCerca).MaximumLength(50);
        RuleFor(x => x.ObservacionDpCerca).MaximumLength(50);

        // Antes exigían las 3 observaciones de cada bloque cuando su checkbox "Incluir
        // Cristales Lejos/Cerca" estaba activo. Se relajó a opcional el 2026-09-08 (ADR 0010) —
        // ver CrearRecetaCristalesCommandValidator para el detalle.
    }
}
