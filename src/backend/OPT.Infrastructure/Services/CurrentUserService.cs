using Microsoft.AspNetCore.Http;
using OPT.Application.Common.Interfaces;
using System.Security.Claims;

namespace OPT.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool EstaAutenticado => User?.Identity?.IsAuthenticated ?? false;

    public int UsuarioId
        => int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                         User?.FindFirstValue("sub"), out var id) ? id : 0;

    public string Rut
        => User?.FindFirstValue("rut") ?? string.Empty;

    public int RolId
        => int.TryParse(User?.FindFirstValue("rolId"), out var rid) ? rid : 0;

    public int? SucursalId
        => int.TryParse(User?.FindFirstValue("sucursalId"), out var sid) ? sid : null;

    public IReadOnlyCollection<int> SucursalesAsignadas
        => (User?.FindFirstValue("sucursales") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .ToArray();
}
