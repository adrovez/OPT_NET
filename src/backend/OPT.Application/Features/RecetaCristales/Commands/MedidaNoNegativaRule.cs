using System.Globalization;
using FluentValidation;

namespace OPT.Application.Features.RecetaCristales.Commands;

/// <summary>
/// DP y ADD llegan como texto (sin cambio de esquema) pero son medidas que no pueden ser
/// negativas: DP = distancia pupilar en mm, ADD = adición para visión de cerca.
/// </summary>
internal static class MedidaNoNegativaRule
{
    public static IRuleBuilderOptions<T, string?> MedidaNoNegativa<T>(
        this IRuleBuilder<T, string?> rule, string nombre) =>
        rule.Must(valor =>
            string.IsNullOrWhiteSpace(valor)
            || (decimal.TryParse(valor.Replace(',', '.'), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var n) && n >= 0))
            .WithMessage($"{nombre} debe ser un número mayor o igual a 0.");
}
