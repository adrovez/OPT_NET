using FluentValidation;
using OPT.Domain.Entities.Inventario;

namespace OPT.Application.Features.Productos;

/// <summary>Reglas de Marca/Modelo/Color/Descripción compartidas por Crear y Actualizar (ADR 0013).</summary>
internal static class ProductoValidacion
{
    /// <param name="categoriaId">
    /// Categoría a validar. Si es null (Actualizar sin categoría) no se aplican las reglas por
    /// categoría: el dominio decide con la categoría vigente del producto.
    /// </param>
    public static void Aplicar<T>(
        AbstractValidator<T> v,
        Func<T, string?> descripcion, Func<T, int?> categoriaId,
        Func<T, string?> marca, Func<T, string?> modelo, Func<T, string?> color)
    {
        v.RuleFor(x => descripcion(x)).MaximumLength(200);
        v.RuleFor(x => marca(x)).MaximumLength(100);
        v.RuleFor(x => modelo(x)).MaximumLength(100);
        v.RuleFor(x => color(x)).MaximumLength(50);

        v.RuleFor(x => marca(x)).NotEmpty().WithMessage("La marca es obligatoria para esta categoría.")
            .When(x => categoriaId(x) is { } c && CategoriaProductoIds.ExigeMarcaYModelo(c));
        v.RuleFor(x => modelo(x)).NotEmpty().WithMessage("El modelo es obligatorio para esta categoría.")
            .When(x => categoriaId(x) is { } c && CategoriaProductoIds.ExigeMarcaYModelo(c));
        v.RuleFor(x => descripcion(x)).NotEmpty().WithMessage("La descripción es obligatoria para esta categoría.")
            .When(x => categoriaId(x) is { } c && !CategoriaProductoIds.ExigeMarcaYModelo(c));
    }
}
