using FluentValidation;
using OPT.Domain.Entities.Inventario;

namespace OPT.Application.Features.Productos.Commands.Crear;

public class CrearProductoCommandValidator : AbstractValidator<CrearProductoCommand>
{
    public CrearProductoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("El código es obligatorio.").MaximumLength(50);

        ProductoValidacion.Aplicar(this, x => x.Descripcion,
            x => x.CategoriaId ?? CategoriaProductoIds.General,
            x => x.Marca, x => x.Modelo, x => x.Color);
    }
}
