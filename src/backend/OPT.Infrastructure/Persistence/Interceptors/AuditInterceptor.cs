using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OPT.Application.Common.Interfaces;
using OPT.Domain.Common;

namespace OPT.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor de EF Core que rellena automáticamente los campos de auditoría
/// (CreadoEn/CreadoPor/ModificadoEn/ModificadoPor) antes de cada SaveChanges.
/// Garantiza que ninguna entidad AuditableEntity quede sin auditar (ADR 0003).
/// </summary>
public sealed class AuditInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
                                                           InterceptionResult<int> result)
    {
        AplicarAuditoria(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        AplicarAuditoria(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void AplicarAuditoria(DbContext? context)
    {
        if (context is null) return;

        var usuarioId = currentUser.EstaAutenticado ? currentUser.UsuarioId : 0;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Usa reflexión para llamar al método privado de la entidad
                    entry.Entity.GetType()
                         .GetMethod("SetCreacion",
                                    System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.NonPublic)?
                         .Invoke(entry.Entity, [usuarioId]);
                    break;

                case EntityState.Modified:
                    entry.Entity.GetType()
                         .GetMethod("SetModificacion",
                                    System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.NonPublic)?
                         .Invoke(entry.Entity, [usuarioId]);
                    break;
            }
        }
    }
}
