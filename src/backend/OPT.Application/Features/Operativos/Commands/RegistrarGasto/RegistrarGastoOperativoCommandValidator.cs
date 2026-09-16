using FluentValidation;

namespace OPT.Application.Features.Operativos.Commands.RegistrarGasto;

public class RegistrarGastoOperativoCommandValidator : AbstractValidator<RegistrarGastoOperativoCommand>
{
    public RegistrarGastoOperativoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto del gasto debe ser mayor a cero.");
        RuleFor(x => x.NumeroDocumento).MaximumLength(50);
        RuleFor(x => x.Observacion).MaximumLength(500);
    }
}
