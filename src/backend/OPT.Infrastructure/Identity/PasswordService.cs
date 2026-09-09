using OPT.Application.Common.Interfaces;

namespace OPT.Infrastructure.Identity;

/// <summary>
/// Implementación con BCrypt (work factor 12).
/// Corrección del hallazgo crítico del legacy: contraseñas en texto plano.
/// Nunca almacenar ni comparar texto plano — todo pasa por esta clase.
/// </summary>
public sealed class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string Hashear(string claveTextoPlano)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(claveTextoPlano);
        return BCrypt.Net.BCrypt.HashPassword(claveTextoPlano, WorkFactor);
    }

    public bool Verificar(string claveTextoPlano, string hash)
    {
        if (string.IsNullOrWhiteSpace(claveTextoPlano) || string.IsNullOrWhiteSpace(hash))
            return false;
        return BCrypt.Net.BCrypt.Verify(claveTextoPlano, hash);
    }
}
