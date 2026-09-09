namespace OPT.Application.Common.Interfaces;

/// <summary>
/// Servicio de hashing de contraseñas.
/// Corrección directa del hallazgo de seguridad crítico del legacy:
/// contraseñas en texto plano en OPT_Usuario.Clave (ver reglas-negocio-legado.md).
/// </summary>
public interface IPasswordService
{
    /// <summary>Genera un hash seguro de la contraseña (bcrypt/Argon2).</summary>
    string Hashear(string claveTextoPlano);

    /// <summary>Verifica la contraseña contra el hash almacenado.</summary>
    bool Verificar(string claveTextoPlano, string hash);
}
