using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Interfaces.Repositories;
using OPT.Infrastructure.Identity;
using OPT.Infrastructure.Persistence;
using OPT.Infrastructure.Persistence.Interceptors;
using OPT.Infrastructure.Persistence.Repositories;
using OPT.Infrastructure.Services;
using System.Text;

namespace OPT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
                                                         IConfiguration config)
    {
        // ── Auditoría ─────────────────────────────────────────────────────────
        services.AddScoped<AuditInterceptor>();

        // ── Base de datos ─────────────────────────────────────────────────────
        // La cadena de conexión viene de variables de entorno / user-secrets, NUNCA hardcodeada.
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                config.GetConnectionString("Default"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        // ── Unit of Work ──────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Repositorios ──────────────────────────────────────────────────────
        services.AddScoped<IUsuarioRepositorio,        UsuarioRepositorio>();
        services.AddScoped<IClienteRepositorio,         ClienteRepositorio>();
        services.AddScoped<IAnamnesisRepositorio,       AnamnesisRepositorio>();
        services.AddScoped<IRecetaCristalesRepositorio, RecetaCristalesRepositorio>();
        services.AddScoped<IOrdenDeTrabajoRepositorio,  OrdenDeTrabajoRepositorio>();
        services.AddScoped<ISucursalRepositorio,        SucursalRepositorio>();
        services.AddScoped<IEmpresaRepositorio,         EmpresaRepositorio>();
        services.AddScoped<IRolRepositorio,             RolRepositorio>();
        services.AddScoped<IRegionRepositorio,          RegionRepositorio>();
        services.AddScoped<IComunaRepositorio,          ComunaRepositorio>();
        services.AddScoped<IProductoRepositorio,        ProductoRepositorio>();
        services.AddScoped<IEstadoOTRepositorio,        EstadoOTRepositorio>();
        services.AddScoped<IFormaPagoRepositorio,       FormaPagoRepositorio>();
        services.AddScoped<IEstadoCuotaRepositorio,     EstadoCuotaRepositorio>();
        services.AddScoped<IOperativoRepositorio,       OperativoRepositorio>();
        services.AddScoped<IEstadoOperativoRepositorio, EstadoOperativoRepositorio>();

        // ── Identidad ─────────────────────────────────────────────────────────
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService,    TokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── JWT ───────────────────────────────────────────────────────────────
        var jwtConfig = config.GetSection("Jwt");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtConfig["Issuer"],
                    ValidAudience            = jwtConfig["Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
                };
            });

        return services;
    }
}
