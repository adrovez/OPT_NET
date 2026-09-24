using FluentValidation;

namespace OPT.Application.Features.Productos.Commands.Crear;

public class CrearProductoCommandValidator : AbstractValidator<CrearProductoCommand>
{
    public CrearProductoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("El código es obligatorio.").MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().WithMessage("La descripción es obligatoria.").MaximumLength(200);
    }
}
