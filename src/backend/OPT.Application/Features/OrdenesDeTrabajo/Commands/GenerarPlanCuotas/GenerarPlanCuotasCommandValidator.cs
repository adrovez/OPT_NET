using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.GenerarPlanCuotas;

public class GenerarPlanCuotasCommandValidator : AbstractValidator<GenerarPlanCuotasCommand>
{
    public GenerarPlanCuotasCommandValidator()
    {
        RuleFor(x => x.OrdenPublicId).NotEmpty();
        RuleFor(x => x.NumeroCuotas).InclusiveBetween(1, 60)
            .WithMessage("El número de cuotas debe estar entre 1 y 60.");
        RuleFor(x => x.PrimerVencimiento).NotEmpty()
            .WithMessage("La fecha del primer vencimiento es obligatoria.");
    }
}
