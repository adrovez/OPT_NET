using FluentValidation;

namespace OPT.Application.Features.Bodegas.Commands.Crear;

public class CrearBodegaCommandValidator : AbstractValidator<CrearBodegaCommand>
{
    public CrearBodegaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0).WithMessage("La sucursal es obligatoria.");
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("El código es obligatorio.").MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100);
    }
}
