using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Operativo;
using OPT.Domain.Entities.Organizacion;
using EntidadOperativo = OPT.Domain.Entities.Operativo.Operativo;

namespace OPT.Infrastructure.Persistence.Configurations.Operativo;

public sealed class OperativoConfiguration : IEntityTypeConfiguration<EntidadOperativo>
{
    private const string PrecisionMonto = "decimal(18,2)";

    public void Configure(EntityTypeBuilder<EntidadOperativo> builder)
    {
        builder.ToTable("OPT_Operativo");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
               .UseIdentityColumn()
               .HasColumnName("OperativoId");

        // Identificador no enumerable expuesto en API/URLs — generado por la BD (mismo criterio
        // que OrdenDeTrabajo, ADR 0004).
        builder.Property(o => o.PublicId)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(o => o.PublicId)
               .IsUnique()
               .HasDatabaseName("UQ_Operativos_PublicId");

        // Número correlativo visible — autogenerado por la BD (decisión 2026-09-15, punto 8.5
        // del requerimiento), no se asigna desde la aplicación.
        builder.Property(o => o.Correlativo)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEXT VALUE FOR [dbo].[SEQ_CorrelativoOperativo]");

        builder.HasIndex(o => o.Correlativo)
               .IsUnique()
               .HasDatabaseName("UQ_Operativos_Correlativo");

        builder.Property(o => o.Nombre).IsRequired().HasMaxLength(200);

        builder.Property(o => o.EmpresaId).IsRequired();
        builder.Property(o => o.SucursalId).IsRequired();
        builder.Property(o => o.EstadoOperativoId).IsRequired();
        builder.Property(o => o.Fecha).HasColumnType("date").IsRequired();
        builder.Property(o => o.Observacion).HasMaxLength(500);

        builder.Property(o => o.NombreContacto).HasMaxLength(200);
        builder.Property(o => o.MailContacto).HasMaxLength(200);
        builder.Property(o => o.TelefonoContacto).HasMaxLength(30);

        builder.Property(o => o.MontoTotalVendido).HasColumnType(PrecisionMonto).IsRequired();
        builder.Property(o => o.MontoTotalPagado).HasColumnType(PrecisionMonto).IsRequired();
        builder.Property(o => o.MontoTotalGastos).HasColumnType(PrecisionMonto).IsRequired();

        // Derivada de EstadoOperativoId — no se persiste.
        builder.Ignore(o => o.EstaAnulado);

        // ── Relaciones ────────────────────────────────────────────────────────
        builder.HasOne<Empresa>()
               .WithMany()
               .HasForeignKey(o => o.EmpresaId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Operativos_Empresas");

        builder.HasOne<Sucursal>()
               .WithMany()
               .HasForeignKey(o => o.SucursalId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Operativos_Sucursales");

        builder.HasOne<EstadoOperativo>()
               .WithMany()
               .HasForeignKey(o => o.EstadoOperativoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Operativos_EstadosOperativo");

        builder.HasMany(o => o.Ordenes)
               .WithOne()
               .HasForeignKey(r => r.OperativoId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_OperativoOT_Operativos");

        builder.HasMany(o => o.Gastos)
               .WithOne()
               .HasForeignKey(g => g.OperativoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_GastosOperativo_Operativos");

        builder.HasIndex(o => o.EmpresaId).HasDatabaseName("IX_Operativos_EmpresaId");
        builder.HasIndex(o => o.SucursalId).HasDatabaseName("IX_Operativos_SucursalId");
        builder.HasIndex(o => o.EstadoOperativoId).HasDatabaseName("IX_Operativos_EstadoOperativoId");
        builder.HasIndex(o => o.Fecha).HasDatabaseName("IX_Operativos_Fecha");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(o => o.CreadoEn).IsRequired();
        builder.Property(o => o.CreadoPor).IsRequired();
        builder.Property(o => o.ModificadoEn);
        builder.Property(o => o.ModificadoPor);
        builder.Property(o => o.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(o => o.EliminadoEn);
        builder.Property(o => o.EliminadoPor);
    }
}
