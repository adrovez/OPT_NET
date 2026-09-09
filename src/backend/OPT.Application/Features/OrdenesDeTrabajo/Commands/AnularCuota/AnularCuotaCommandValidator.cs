using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.AnularCuota;

public class AnularCuotaCommandValidator : AbstractValidator<AnularCuotaCommand>
{
    public AnularCuotaCommandValidator()
    {
        RuleFor(x => x.OrdenPublicId).NotEmpty();
        RuleFor(x => x.Numero).GreaterThan(0).WithMessage("El número de cuota debe ser mayor a cero.");
    }
}
