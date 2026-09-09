using OPT.Domain.Entities.Organizacion;

namespace OPT.Application.Common.Interfaces;

public interface ITokenService
{
    /// <summary>Genera un JWT para el usuario autenticado.</summary>
    string GenerarToken(Usuario usuario);
}
