using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.PagarCuota;

public class PagarCuotaCommandValidator : AbstractValidator<PagarCuotaCommand>
{
    public PagarCuotaCommandValidator()
    {
        RuleFor(x => x.OrdenPublicId).NotEmpty();
        RuleFor(x => x.Numero).GreaterThan(0).WithMessage("El número de cuota debe ser mayor a cero.");
    }
}
