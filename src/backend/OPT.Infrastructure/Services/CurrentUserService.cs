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

    public int? SucursalId
        => int.TryParse(User?.FindFirstValue("sucursalId"), out var sid) ? sid : null;
}
