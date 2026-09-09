namespace OPT.Migracion;

/// <summary>
/// Cadenas de conexión de origen (legacy) y destino (nuevo esquema).
/// Ambas usan Windows Authentication (Trusted_Connection) — no hay credenciales que proteger.
/// Sobrescribibles por argumentos de línea de comandos (--legacy=..., --destino=...).
/// </summary>
public sealed class ConexionOptions
{
    public string Legacy { get; init; } =
        "Server=localhost;Database=db_a25cfd_opt2;Trusted_Connection=True;TrustServerCertificate=True;";

    public string Destino { get; init; } =
        "Server=localhost;Database=dbOPT_NET;Trusted_Connection=True;TrustServerCertificate=True;";

    public static ConexionOptions DesdeArgumentos(string[] args)
    {
        string? legacy = null;
        string? destino = null;

        foreach (var arg in args)
        {
            if (arg.StartsWith("--legacy=", StringComparison.OrdinalIgnoreCase))
                legacy = arg["--legacy=".Length..];
            else if (arg.StartsWith("--destino=", StringComparison.OrdinalIgnoreCase))
                destino = arg["--destino=".Length..];
        }

        var defaults = new ConexionOptions();
        return new ConexionOptions
        {
            Legacy = legacy ?? defaults.Legacy,
            Destino = destino ?? defaults.Destino
        };
    }
}
