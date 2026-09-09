using System.Globalization;
using System.Text.RegularExpressions;

namespace OPT.Migracion;

/// <summary>
/// El legacy guarda esfera/cilindro como varchar con formato inconsistente: a veces con punto
/// decimal ("+1.25"), a veces como dígitos puros que representan centésimas ("+125" = 1.25,
/// "+025" = 0.25). Ambas formas conviven para el mismo concepto en filas distintas — se detectó
/// perfilando datos reales antes de escribir este parser (ver .agents/context/migracion-datos-legacy.md,
/// regla 1). Cualquier valor que no calce ninguno de los dos formatos se reporta como excepción
/// y se guarda NULL — nunca se inventa un valor.
/// </summary>
public static class RecetaCristalesParser
{
    public sealed record Excepcion(long IdRecetaCristales, string Campo, string ValorCrudo);

    private static readonly Regex SoloDigitosConSigno = new(@"^[+-]?\d+$", RegexOptions.Compiled);

    public static decimal? ParsearGraduacion(string? valor, long idRecetaCristales, string campo, List<Excepcion> excepciones)
    {
        var original = valor?.Trim();
        if (string.IsNullOrEmpty(original))
            return null;

        // "NEUTRO"/"NEUTROS" es terminología clínica real (sin corrección), no un valor inválido —
        // se detectó perfilando las excepciones de un dry-run real antes de descartarlas como basura.
        if (original.Equals("NEUTRO", StringComparison.OrdinalIgnoreCase) ||
            original.Equals("NEUTROS", StringComparison.OrdinalIgnoreCase))
            return 0.00m;

        // El legacy mezcla coma decimal (formato chileno, "-0,50") con punto — se normaliza antes de parsear.
        var v = original.Replace(',', '.');

        if (v.Contains('.') &&
            decimal.TryParse(v, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var directo))
            return directo;

        if (SoloDigitosConSigno.IsMatch(v) &&
            int.TryParse(v, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var centesimas))
            return centesimas / 100m;

        excepciones.Add(new Excepcion(idRecetaCristales, campo, original));
        return null;
    }

    /// <summary>Eje (ángulo, 0-180). A diferencia de la graduación, es un entero simple sin escala implícita.</summary>
    public static int? ParsearEje(string? valor, long idRecetaCristales, string campo, List<Excepcion> excepciones)
    {
        var v = valor?.Trim();
        if (string.IsNullOrEmpty(v))
            return null;

        if (int.TryParse(v, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var eje))
            return eje;

        excepciones.Add(new Excepcion(idRecetaCristales, campo, v));
        return null;
    }

    public static string? LimpiarTexto(string? valor)
    {
        var v = valor?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }

    /// <summary>
    /// OPT_RecetaCristales nuevo tiene un único campo Observaciones; el legacy tiene cuatro
    /// (Lejos/Cerca x OD/OI) más las de DP. Se combinan en un solo texto etiquetado — no se pierde
    /// contenido, solo se deja de tener una columna por campo.
    /// </summary>
    public static string? CombinarObservaciones(params (string Etiqueta, string? Valor)[] campos)
    {
        var partes = campos
            .Where(c => !string.IsNullOrWhiteSpace(c.Valor))
            .Select(c => $"{c.Etiqueta}: {c.Valor!.Trim()}");
        var combinado = string.Join("; ", partes);
        return combinado.Length == 0 ? null : combinado;
    }
}
