using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Inventario;

public sealed class BodegaConfiguration : IEntityTypeConfiguration<Bodega>
{
    public void Configure(EntityTypeBuilder<Bodega> builder)
    {
        builder.ToTable("OPT_Bodega");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
               .UseIdentityColumn()
               .HasColumnName("BodegaId");

        // Identificador no enumerable expuesto en API/URLs — generado por la BD (ADR 0004).
        builder.Property(b => b.PublicId)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(b => b.PublicId)
               .IsUnique()
               .HasDatabaseName("UQ_Bodegas_PublicId");

        builder.Property(b => b.SucursalId).IsRequired();
        builder.Property(b => b.Codigo).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(b => b.EsPrincipal).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.Activa).IsRequired().HasDefaultValue(true);

        builder.HasOne<Sucursal>()
               .WithMany()
               .HasForeignKey(b => b.SucursalId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Bodegas_Sucursales");

        builder.HasIndex(b => b.SucursalId)
               .HasDatabaseName("IX_Bodegas_SucursalId");

        builder.HasIndex(b => new { b.SucursalId, b.Codigo })
               .HasFilter("[Eliminado] = 0")
               .IsUnique()
               .HasDatabaseName("UQ_Bodegas_SucursalCodigo");

        // Una sola bodega principal por sucursal.
        builder.HasIndex(b => b.SucursalId)
               .HasFilter("[EsPrincipal] = 1 AND [Eliminado] = 0")
               .IsUnique()
               .HasDatabaseName("UQ_Bodegas_UnicaPrincipal");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(b => b.CreadoEn).IsRequired();
        builder.Property(b => b.CreadoPor).IsRequired();
        builder.Property(b => b.ModificadoEn);
        builder.Property(b => b.ModificadoPor);
        builder.Property(b => b.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.EliminadoEn);
        builder.Property(b => b.EliminadoPor);
    }
}
