using FluentValidation;

namespace OPT.Application.Features.OrdenesDeTrabajo.Commands.CambiarEstado;

public class CambiarEstadoOrdenDeTrabajoCommandValidator
    : AbstractValidator<CambiarEstadoOrdenDeTrabajoCommand>
{
    public CambiarEstadoOrdenDeTrabajoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.NuevoEstadoId).GreaterThanOrEqualTo(0)
            .WithMessage("El estado destino es obligatorio.");
        RuleFor(x => x.Observacion).MaximumLength(200);
    }
}
