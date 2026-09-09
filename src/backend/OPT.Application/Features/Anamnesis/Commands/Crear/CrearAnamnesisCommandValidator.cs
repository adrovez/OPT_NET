using FluentValidation;

namespace OPT.Application.Features.Anamnesis.Commands.Crear;

public class CrearAnamnesisCommandValidator : AbstractValidator<CrearAnamnesisCommand>
{
    public CrearAnamnesisCommandValidator()
    {
        RuleFor(x => x.ClientePublicId).NotEmpty().WithMessage("El cliente es obligatorio.");
        RuleFor(x => x.DetalleAlergias).MaximumLength(500);
        RuleFor(x => x.Observaciones).MaximumLength(500);
    }
}
