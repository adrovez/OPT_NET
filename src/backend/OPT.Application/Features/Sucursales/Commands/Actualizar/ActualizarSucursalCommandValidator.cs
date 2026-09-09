using FluentValidation;

namespace OPT.Application.Features.Sucursales.Commands.Actualizar;

public class ActualizarSucursalCommandValidator : AbstractValidator<ActualizarSucursalCommand>
{
    public ActualizarSucursalCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.Direccion).MaximumLength(200);
        RuleFor(x => x.Telefono).MaximumLength(20);
    }
}
