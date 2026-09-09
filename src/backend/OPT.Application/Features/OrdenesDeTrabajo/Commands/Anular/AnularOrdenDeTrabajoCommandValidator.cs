using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Anular;

public class AnularOrdenDeTrabajoCommandValidator : AbstractValidator<AnularOrdenDeTrabajoCommand>
{
    public AnularOrdenDeTrabajoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("Anular una OT exige indicar el motivo.")
            .MaximumLength(200);
    }
}
