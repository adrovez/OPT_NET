using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.RegistrarAbono;

public class RegistrarAbonoCommandValidator : AbstractValidator<RegistrarAbonoCommand>
{
    public RegistrarAbonoCommandValidator()
    {
        RuleFor(x => x.OrdenPublicId).NotEmpty();
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto del abono debe ser mayor a cero.");
        RuleFor(x => x.FormaPagoId).GreaterThanOrEqualTo(0)
            .WithMessage("La forma de pago es obligatoria.");
        RuleFor(x => x.Referencia).MaximumLength(50);
    }
}
