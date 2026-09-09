using FluentValidation;

namespace OPT.Application.Features.Clientes.Commands.Actualizar;

public class ActualizarClienteCommandValidator : AbstractValidator<ActualizarClienteCommand>
{
    public ActualizarClienteCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().WithMessage("El apellido es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(150).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Telefono).MaximumLength(20);
        RuleFor(x => x.Direccion).MaximumLength(200);
        RuleFor(x => x.TipoPrevision).MaximumLength(50);
    }
}
