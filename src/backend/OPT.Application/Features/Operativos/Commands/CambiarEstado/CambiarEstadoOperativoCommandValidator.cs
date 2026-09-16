using FluentValidation;

namespace OPT.Application.Features.Operativos.Commands.CambiarEstado;

public class CambiarEstadoOperativoCommandValidator : AbstractValidator<CambiarEstadoOperativoCommand>
{
    public CambiarEstadoOperativoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.NuevoEstadoId).GreaterThan(0).WithMessage("El nuevo estado es obligatorio.");
    }
}
