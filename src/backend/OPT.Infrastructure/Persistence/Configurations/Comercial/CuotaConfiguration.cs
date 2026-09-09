using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

/// <summary>
/// Espeja la tabla creada a mano en <c>004_comercial_pagos_cuotas.sql</c> — cualquier cambio
/// aquí debe reflejarse en un script nuevo de <c>src/basedatos/</c> (ADR 0006).
/// </summary>
public sealed class CuotaConfiguration : IEntityTypeConfiguration<Cuota>
{
    public void Configure(EntityTypeBuilder<Cuota> builder)
    {
        builder.ToTable("OPT_Cuota");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .UseIdentityColumn()
               .HasColumnName("CuotaId");

        builder.Property(c => c.OrdenDeTrabajoId).IsRequired();
        builder.Property(c => c.Numero).IsRequired();
        builder.Property(c => c.ValorCuota).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(c => c.FechaVencimiento).HasColumnType("date").IsRequired();
        builder.Property(c => c.FechaPago);
        builder.Property(c => c.FormaPagoId);
        builder.Property(c => c.EstadoCuotaId).IsRequired();

        // Propiedad de conveniencia derivada del estado — no se persiste.
        builder.Ignore(c => c.EstaPendiente);

        builder.HasIndex(c => new { c.OrdenDeTrabajoId, c.Numero })
               .IsUnique()
               .HasDatabaseName("UQ_Cuotas_OrdenDeTrabajoId_Numero");

        builder.HasIndex(c => c.EstadoCuotaId).HasDatabaseName("IX_Cuotas_EstadoCuotaId");

        builder.HasOne<FormaPago>()
               .WithMany()
               .HasForeignKey(c => c.FormaPagoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Cuotas_FormasPago");

        builder.HasOne<EstadoCuota>()
               .WithMany()
               .HasForeignKey(c => c.EstadoCuotaId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Cuotas_EstadosCuota");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(c => c.CreadoEn).IsRequired();
        builder.Property(c => c.CreadoPor).IsRequired();
        builder.Property(c => c.ModificadoEn);
        builder.Property(c => c.ModificadoPor);
        builder.Property(c => c.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(c => c.EliminadoEn);
        builder.Property(c => c.EliminadoPor);
    }
}
