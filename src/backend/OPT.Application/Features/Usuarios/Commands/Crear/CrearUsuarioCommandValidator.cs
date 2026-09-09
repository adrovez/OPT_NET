using FluentValidation;

namespace OPT.Application.Features.Usuarios.Commands.Crear;

public class CrearUsuarioCommandValidator : AbstractValidator<CrearUsuarioCommand>
{
    public CrearUsuarioCommandValidator()
    {
        RuleFor(x => x.Rut).NotEmpty().WithMessage("El RUT es obligatorio.").MaximumLength(12);
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().WithMessage("El apellido es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Clave)
            .NotEmpty().WithMessage("La clave es obligatoria.")
            .MinimumLength(6).WithMessage("La clave debe tener al menos 6 caracteres.");
        RuleFor(x => x.RolId).GreaterThan(0).WithMessage("El rol es obligatorio.");
        RuleFor(x => x.Email).MaximumLength(150).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
