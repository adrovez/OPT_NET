using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.RegistrarPago;

public class RegistrarPagoCommandValidator : AbstractValidator<RegistrarPagoCommand>
{
    public RegistrarPagoCommandValidator()
    {
        RuleFor(x => x.OrdenPublicId).NotEmpty();
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto del pago debe ser mayor a cero.");
        RuleFor(x => x.FormaPagoId).GreaterThanOrEqualTo(0)
            .WithMessage("La forma de pago es obligatoria.");
        RuleFor(x => x.Referencia).MaximumLength(100);
    }
}
