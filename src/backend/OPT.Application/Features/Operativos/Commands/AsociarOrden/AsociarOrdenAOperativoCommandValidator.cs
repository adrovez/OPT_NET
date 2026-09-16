using FluentValidation;

namespace OPT.Application.Features.Operativos.Commands.AsociarOrden;

public class AsociarOrdenAOperativoCommandValidator : AbstractValidator<AsociarOrdenAOperativoCommand>
{
    public AsociarOrdenAOperativoCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty();
        RuleFor(x => x.OrdenPublicId).NotEmpty().WithMessage("La Orden de Trabajo es obligatoria.");
    }
}
