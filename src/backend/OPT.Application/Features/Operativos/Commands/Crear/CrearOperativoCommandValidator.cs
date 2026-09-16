using FluentValidation;

namespace OPT.Application.Features.Operativos.Commands.Crear;

public class CrearOperativoCommandValidator : AbstractValidator<CrearOperativoCommand>
{
    public CrearOperativoCommandValidator()
    {
        RuleFor(x => x.EmpresaPublicId).NotEmpty().WithMessage("La empresa es obligatoria.");
        RuleFor(x => x.SucursalId).GreaterThan(0).WithMessage("La sucursal es obligatoria.");
        RuleFor(x => x.Fecha).NotEmpty().WithMessage("La fecha del Operativo es obligatoria.");
        RuleFor(x => x.Observacion).MaximumLength(500);
    }
}
