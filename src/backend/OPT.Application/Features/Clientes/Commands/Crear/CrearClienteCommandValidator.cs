using FluentValidation;

namespace OPT.Application.Features.Clientes.Commands.Crear;

public class CrearClienteCommandValidator : AbstractValidator<CrearClienteCommand>
{
    public CrearClienteCommandValidator()
    {
        RuleFor(x => x.Rut).NotEmpty().WithMessage("El RUT es obligatorio.").MaximumLength(12);
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().WithMessage("El apellido es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(150).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Telefono).MaximumLength(20);
        RuleFor(x => x.Direccion).MaximumLength(200);
        RuleFor(x => x.TipoPrevision).MaximumLength(50);
    }
}
