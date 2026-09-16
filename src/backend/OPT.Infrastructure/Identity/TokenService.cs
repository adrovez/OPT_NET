using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Entities.Organizacion;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OPT.Infrastructure.Identity;

/// <summary>
/// Generación de JWT.
/// La clave secreta no está en código ni en archivos versionados —
/// se lee de configuración (variable de entorno / secrets manager, ADR 0003).
/// </summary>
public sealed class TokenService(IConfiguration config) : ITokenService
{
    public string GenerarToken(Usuario usuario)
    {
        var jwtConfig = config.GetSection("Jwt");
        var claveStr  = jwtConfig["Key"]
            ?? throw new InvalidOperationException("JWT:Key no está configurada.");

        var clave     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveStr));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,  usuario.Id.ToString()),
            new Claim("rut",   usuario.Rut),
            new Claim("nombre", $"{usuario.Nombre} {usuario.Apellido}"),
            new Claim("rolId", usuario.RolId.ToString()),
            new Claim("sucursalId", usuario.SucursalActivaId?.ToString() ?? ""),
            new Claim("sucursales", string.Join(',', usuario.Sucursales.Select(s => s.SucursalId))),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiracion = int.TryParse(jwtConfig["ExpiresMinutes"], out var min) ? min : 480;

        var token = new JwtSecurityToken(
            issuer:              jwtConfig["Issuer"],
            audience:            jwtConfig["Audience"],
            claims:              claims,
            expires:             DateTime.UtcNow.AddMinutes(expiracion),
            signingCredentials:  credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
