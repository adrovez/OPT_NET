using OPT.Migracion.Destino;
using OPT.Migracion.Legacy;

namespace OPT.Migracion;

/// <summary>
/// El legacy usa códigos INE de 5 dígitos para idComuna (más un centinela 0 = "SIN COMUNA");
/// el nuevo catálogo (ya sembrado, ver ComunaConfiguration.HasData()) usa ids secuenciales
/// propios. El mapeo debe hacerse por nombre exacto, igual que RolMapper (ver .agents/progress.md,
/// 2026-08-24) — nunca copiar el id legacy directamente.
/// </summary>
public static class ComunaMapper
{
    public sealed record Resultado(IReadOnlyDictionary<int, int> LegacyANuevo, IReadOnlyList<string> SinMapear);

    public static Resultado Construir(IReadOnlyList<LegacyComuna> comunasLegacy, IReadOnlyList<DestinoComuna> comunasDestino)
    {
        var porNombre = comunasDestino
            .GroupBy(c => c.Nombre.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var mapa = new Dictionary<int, int>();
        var sinMapear = new List<string>();

        foreach (var comuna in comunasLegacy.Where(c => c.IdComuna != 0))
        {
            if (porNombre.TryGetValue(comuna.Comuna.Trim(), out var nuevoId))
                mapa[comuna.IdComuna] = nuevoId;
            else
                sinMapear.Add($"{comuna.IdComuna} — {comuna.Comuna}");
        }

        return new Resultado(mapa, sinMapear);
    }
}
