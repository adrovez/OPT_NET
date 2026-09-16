using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Crear;

public class CrearOrdenDeTrabajoCommandValidator : AbstractValidator<CrearOrdenDeTrabajoCommand>
{
    public CrearOrdenDeTrabajoCommandValidator()
    {
        RuleFor(x => x.NumeroOT).GreaterThan(0).WithMessage("El número de orden es obligatorio.");
        RuleFor(x => x.ClientePublicId).NotEmpty().WithMessage("El cliente es obligatorio.");
        RuleFor(x => x.SucursalId).GreaterThan(0).WithMessage("La sucursal es obligatoria.");
        RuleFor(x => x.FechaEntrega).NotEmpty().WithMessage("La fecha de entrega es obligatoria.");

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("La OT debe tener al menos una línea de detalle.");

        RuleForEach(x => x.Detalles).ChildRules(d =>
        {
            d.RuleFor(l => l.ProductoId).GreaterThan(0).WithMessage("El producto es obligatorio.");
            d.RuleFor(l => l.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");
            d.RuleFor(l => l.ValorUnitario).GreaterThanOrEqualTo(0)
                .WithMessage("El valor unitario no puede ser negativo.");
        });

        RuleFor(x => x.Observaciones).MaximumLength(500);
        RuleFor(x => x.Beneficiario).MaximumLength(100);
        RuleFor(x => x.ReferenciaAbono).MaximumLength(50);

        RuleFor(x => x.AbonoInicial).GreaterThan(0)
            .When(x => x.AbonoInicial.HasValue)
            .WithMessage("El abono inicial debe ser mayor a cero.");

        RuleFor(x => x.FormaPagoAbono).NotNull()
            .When(x => x.AbonoInicial is > 0)
            .WithMessage("Indique la forma de pago del abono inicial.");

        RuleFor(x => x.NumeroCuotas).InclusiveBetween(1, 60)
            .When(x => x.NumeroCuotas.HasValue)
            .WithMessage("El número de cuotas debe estar entre 1 y 60.");
    }
}
