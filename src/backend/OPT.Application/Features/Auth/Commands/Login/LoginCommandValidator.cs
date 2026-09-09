using FluentValidation;

namespace OPT.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Rut)
            .NotEmpty().WithMessage("El RUT es obligatorio.")
            .MaximumLength(12);

        RuleFor(x => x.Clave)
            .NotEmpty().WithMessage("La clave es obligatoria.")
            .MinimumLength(6).WithMessage("La clave debe tener al menos 6 caracteres.");
    }
}
