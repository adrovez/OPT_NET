using FluentValidation;

namespace OPT.Application.Features.Bodegas.Commands.Actualizar;

public class ActualizarBodegaCommandValidator : AbstractValidator<ActualizarBodegaCommand>
{
    public ActualizarBodegaCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("El código es obligatorio.").MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100);
    }
}
