using OPT.Migracion.Destino;
using OPT.Migracion.Legacy;

namespace OPT.Migracion;

/// <summary>
/// Región/Comuna: el nuevo esquema ya viene con el catálogo oficial INE completo sembrado vía
/// HasData() (16 regiones, 346 comunas — ver RegionConfiguration/ComunaConfiguration). Este
/// verificador NUNCA escribe: solo confirma que el catálogo legacy está cubierto por nombre,
/// para dejar constancia de que no falta ninguna migración de datos ahí.
/// </summary>
public static class CatalogoVerificador
{
    public sealed record Resultado(
        int RegionesLegacy, int RegionesCubiertas, IReadOnlyList<string> RegionesSinCobertura,
        int ComunasLegacy, int ComunasCubiertas, IReadOnlyList<string> ComunasSinCobertura);

    public static Resultado Verificar(
        IReadOnlyList<LegacyRegion> regionesLegacy, IReadOnlyList<DestinoRegion> regionesDestino,
        IReadOnlyList<LegacyComuna> comunasLegacy, IReadOnlyList<DestinoComuna> comunasDestino)
    {
        // idRegion=0 "SIN REGION" e idComuna=0 "SIN COMUNA" son centinelas del legacy sin
        // equivalente real — no representan una región/comuna a cubrir.
        var regionesReales = regionesLegacy.Where(r => r.IdRegion != 0).ToList();
        var comunasReales = comunasLegacy.Where(c => c.IdComuna != 0).ToList();

        var nombresRegionDestino = regionesDestino
            .Select(r => NormalizarNombreRegion(r.Nombre))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var nombresComunaDestino = comunasDestino
            .Select(c => c.Nombre.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var regionesSinCobertura = regionesReales
            .Where(r => !nombresRegionDestino.Contains(NormalizarNombreRegion(r.Region)))
            .Select(r => $"{r.IdRegion} — {r.Region}")
            .ToList();

        var comunasSinCobertura = comunasReales
            .Where(c => !nombresComunaDestino.Contains(c.Comuna.Trim()))
            .Select(c => $"{c.IdComuna} — {c.Comuna}")
            .ToList();

        return new Resultado(
            regionesReales.Count, regionesReales.Count - regionesSinCobertura.Count, regionesSinCobertura,
            comunasReales.Count, comunasReales.Count - comunasSinCobertura.Count, comunasSinCobertura);
    }

    // El legacy tiene nombres cortos ("Tarapacá", "Valparaíso"); el nuevo catálogo usa el
    // nombre oficial completo ("Región de Tarapacá"). Se compara por la parte distintiva.
    private static string NormalizarNombreRegion(string nombre)
    {
        var n = nombre.Trim();
        foreach (var prefijo in new[] { "Región de ", "Región del ", "Región " })
            if (n.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                return n[prefijo.Length..].Trim();
        return n;
    }
}
