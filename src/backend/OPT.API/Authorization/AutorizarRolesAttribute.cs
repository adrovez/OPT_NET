using Microsoft.AspNetCore.Mvc.Filters;

namespace OPT.API.Authorization;

/// <summary>
/// Restringe una acción o controller a los roles indicados, leyendo el claim <c>rolId</c>
/// que ya viaja en el JWT (<see cref="OPT.Infrastructure.Identity.TokenService"/>).
/// No usa el <c>Roles=</c> de <see cref="Microsoft.AspNetCore.Authorization.AuthorizeAttribute"/>
/// porque ese mecanismo compara contra el claim <c>ClaimTypes.Role</c>, que este JWT no emite —
/// el rol viaja como <c>rolId</c> numérico (ver ADR y CLAUDE.md, sección de autorización).
/// Requiere que el usuario ya esté autenticado (<c>[Authorize]</c> en el controller).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class AutorizarRolesAttribute(params int[] rolesPermitidos) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        var rolIdClaim = user.FindFirst("rolId")?.Value;
        if (!int.TryParse(rolIdClaim, out var rolId) || !rolesPermitidos.Contains(rolId))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
        }
    }
}
