using FluentValidation;

namespace OPT.Application.Features.Empresas.Commands.Crear;

public class CrearEmpresaCommandValidator : AbstractValidator<CrearEmpresaCommand>
{
    public CrearEmpresaCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Rut).NotEmpty().WithMessage("El RUT es obligatorio.").MaximumLength(12);
        RuleFor(x => x.RazonSocial).MaximumLength(150);
        RuleFor(x => x.Giro).MaximumLength(150);
        RuleFor(x => x.Direccion).MaximumLength(200);
        RuleFor(x => x.Telefono).MaximumLength(20);
        RuleFor(x => x.Email).MaximumLength(150).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Contacto).MaximumLength(100);
    }
}
