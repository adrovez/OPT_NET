using FluentValidation;

namespace OPT.Application.Features.Operativos.Commands.Actualizar;

public class ActualizarOperativoCommandValidator : AbstractValidator<ActualizarOperativoCommand>
{
    public ActualizarOperativoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.Fecha).NotEmpty().WithMessage("La fecha del Operativo es obligatoria.");
        RuleFor(x => x.Observacion).MaximumLength(500);
    }
}
