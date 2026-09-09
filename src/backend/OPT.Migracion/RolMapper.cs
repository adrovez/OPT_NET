using OPT.Migracion.Destino;
using OPT.Migracion.Legacy;

namespace OPT.Migracion;

/// <summary>
/// El legacy tiene 6 roles (ids 1-6); el nuevo esquema los conserva como filas 4-8 junto a
/// 3 roles genéricos nuevos (1-3) — ver RolConfiguration.HasData(). El id NO es el mismo,
/// el mapeo debe hacerse por nombre exacto (ver .agents/progress.md, 2026-08-21).
/// </summary>
public static class RolMapper
{
    public sealed record Resultado(IReadOnlyDictionary<int, int> LegacyANuevo, IReadOnlyList<string> SinMapear);

    public static Resultado Construir(IReadOnlyList<LegacyRol> rolesLegacy, IReadOnlyList<DestinoRol> rolesDestino)
    {
        var porNombre = rolesDestino.ToDictionary(r => r.Nombre.Trim(), r => r.Id, StringComparer.OrdinalIgnoreCase);

        var mapa = new Dictionary<int, int>();
        var sinMapear = new List<string>();

        foreach (var rol in rolesLegacy)
        {
            if (porNombre.TryGetValue(rol.Rol.Trim(), out var nuevoId))
                mapa[rol.IdRol] = nuevoId;
            else
                sinMapear.Add($"{rol.IdRol} — {rol.Rol}");
        }

        return new Resultado(mapa, sinMapear);
    }
}
