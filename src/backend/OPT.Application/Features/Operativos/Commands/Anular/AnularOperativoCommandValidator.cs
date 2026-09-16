using FluentValidation;

namespace OPT.Application.Features.Operativos.Commands.Anular;

public class AnularOperativoCommandValidator : AbstractValidator<AnularOperativoCommand>
{
    public AnularOperativoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.Motivo).NotEmpty().WithMessage("El motivo de anulación es obligatorio.")
            .MaximumLength(500);
    }
}
