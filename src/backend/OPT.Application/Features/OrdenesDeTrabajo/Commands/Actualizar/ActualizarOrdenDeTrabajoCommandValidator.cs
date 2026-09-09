using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.Actualizar;

public class ActualizarOrdenDeTrabajoCommandValidator : AbstractValidator<ActualizarOrdenDeTrabajoCommand>
{
    public ActualizarOrdenDeTrabajoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.FechaEntrega).NotEmpty().WithMessage("La fecha de entrega es obligatoria.");
        RuleFor(x => x.Observaciones).MaximumLength(500);
        RuleFor(x => x.Beneficiario).MaximumLength(100);

        RuleFor(x => x.Detalles)
            .NotEmpty().When(x => x.Detalles is not null)
            .WithMessage("Si envía el detalle, debe traer al menos una línea.");

        RuleForEach(x => x.Detalles!)
            .ChildRules(d =>
            {
                d.RuleFor(l => l.ProductoId).GreaterThan(0).WithMessage("El producto es obligatorio.");
                d.RuleFor(l => l.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");
                d.RuleFor(l => l.ValorUnitario).GreaterThanOrEqualTo(0)
                    .WithMessage("El valor unitario no puede ser negativo.");
            })
            .When(x => x.Detalles is not null);
    }
}
