using FluentValidation;

namespace OPT.Application.Features.Usuarios.Commands.CambiarClave;

public class CambiarClaveUsuarioCommandValidator : AbstractValidator<CambiarClaveUsuarioCommand>
{
    public CambiarClaveUsuarioCommandValidator()
    {
        RuleFor(x => x.ClaveActual).NotEmpty().WithMessage("La clave actual es obligatoria.");
        RuleFor(x => x.ClaveNueva)
            .NotEmpty().WithMessage("La clave nueva es obligatoria.")
            .MinimumLength(6).WithMessage("La clave debe tener al menos 6 caracteres.");
    }
}
