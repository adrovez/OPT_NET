using FluentValidation;

namespace OPT.Application.Features.Productos.Commands.Actualizar;

public class ActualizarProductoCommandValidator : AbstractValidator<ActualizarProductoCommand>
{
    public ActualizarProductoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("El código es obligatorio.").MaximumLength(50);

        ProductoValidacion.Aplicar(this, x => x.Descripcion, x => x.CategoriaId,
            x => x.Marca, x => x.Modelo, x => x.Color);
    }
}
