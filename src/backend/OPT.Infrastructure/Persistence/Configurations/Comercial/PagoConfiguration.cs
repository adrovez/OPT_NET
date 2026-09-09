using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

/// <summary>
/// Espeja la tabla creada a mano en <c>004_comercial_pagos_cuotas.sql</c> — cualquier cambio
/// aquí debe reflejarse en un script nuevo de <c>src/basedatos/</c> (ADR 0006).
/// </summary>
public sealed class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("OPT_Pago");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .UseIdentityColumn()
               .HasColumnName("PagoId");

        builder.Property(p => p.OrdenDeTrabajoId).IsRequired();
        builder.Property(p => p.FechaPago).IsRequired();
        builder.Property(p => p.Monto).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.FormaPagoId).IsRequired();
        builder.Property(p => p.Referencia).HasMaxLength(100);

        builder.HasIndex(p => p.OrdenDeTrabajoId).HasDatabaseName("IX_Pagos_OrdenDeTrabajoId");
        builder.HasIndex(p => p.FormaPagoId).HasDatabaseName("IX_Pagos_FormaPagoId");

        builder.HasOne<FormaPago>()
               .WithMany()
               .HasForeignKey(p => p.FormaPagoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Pagos_FormasPago");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(p => p.CreadoEn).IsRequired();
        builder.Property(p => p.CreadoPor).IsRequired();
        builder.Property(p => p.ModificadoEn);
        builder.Property(p => p.ModificadoPor);
        builder.Property(p => p.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.EliminadoEn);
        builder.Property(p => p.EliminadoPor);
    }
}
