namespace OPT.Migracion;

/// <summary>
/// Reglas de normalización ya aprobadas para la migración (ver .agents/progress.md, 2026-08-21).
/// Replican exactamente lo que hace OPT.Domain.Entities.Organizacion.Usuario.Crear() para no
/// generar datos que un alta manual posterior por la API no pudiera producir.
/// </summary>
public static class Normalizacion
{
    public static string NormalizarRut(string rut) => rut.Trim().ToUpperInvariant();

    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>Primera palabra → Nombre, resto → Apellido. Sin resto, Apellido = Nombre.</summary>
    public static (string Nombre, string Apellido) DividirNombreCompleto(string nombreCompleto)
    {
        var partes = nombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0)
            return (nombreCompleto.Trim(), nombreCompleto.Trim());

        var nombre = partes[0];
        var apellido = partes.Length > 1 ? string.Join(' ', partes[1..]) : partes[0];
        return (nombre, apellido);
    }

    /// <summary>
    /// FechaNacimiento del legacy tiene centinelas/errores de carga (ej. 0001-01-01, o años
    /// futuros como 2049 — probable error de digitación de 2 dígitos). Se descarta (NULL + reporte)
    /// cualquier valor fuera de un rango humano plausible en vez de migrarlo tal cual.
    /// </summary>
    public static bool EsFechaNacimientoPlausible(DateTime fecha) =>
        fecha.Year >= 1900 && fecha <= DateTime.UtcNow;
}
