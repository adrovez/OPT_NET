using FluentValidation;

namespace OPT.Application.Features.Operativos.Commands.Actualizar;

public class ActualizarOperativoCommandValidator : AbstractValidator<ActualizarOperativoCommand>
{
    public ActualizarOperativoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre del Operativo es obligatorio.")
            .MaximumLength(200);
        RuleFor(x => x.Fecha).NotEmpty().WithMessage("La fecha del Operativo es obligatoria.");
        RuleFor(x => x.Observacion).MaximumLength(500);

        RuleFor(x => x.NombreContacto).MaximumLength(200);
        RuleFor(x => x.MailContacto).MaximumLength(200).EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.MailContacto));
        RuleFor(x => x.TelefonoContacto).MaximumLength(30);
    }
}
