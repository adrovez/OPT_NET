using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OPT.Application.Common.Interfaces;
using OPT.Infrastructure.Persistence.Interceptors;

namespace OPT.Infrastructure.Persistence;

/// <summary>
/// Factory para que las EF Core Tools (<c>dotnet ef migrations add</c>) puedan
/// instanciar AppDbContext en tiempo de diseño, sin necesidad de user-secrets.
///
/// Prioridad de cadena de conexión:
/// 1. Variable de entorno OPT_CONN
/// 2. Fallback a localhost/dbOPT_NET con autenticación de Windows
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("OPT_CONN")
            ?? "Server=localhost;Database=dbOPT_NET;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        // En tiempo de diseño no hay usuario autenticado.
        // El interceptor recibe un stub que devuelve 0 — no se invocará
        // ningún SaveChanges durante la generación de migraciones.
        var designTimeUserService = new DesignTimeCurrentUserService();
        var interceptor           = new AuditInterceptor(designTimeUserService);

        return new AppDbContext(options, interceptor);
    }

    /// <summary>
    /// Implementación mínima de ICurrentUserService para tiempo de diseño.
    /// </summary>
    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public int    UsuarioId  => 0;
        public string Rut        => string.Empty;
        public int?   SucursalId => null;
        public bool   EstaAutenticado => false;
    }
}
