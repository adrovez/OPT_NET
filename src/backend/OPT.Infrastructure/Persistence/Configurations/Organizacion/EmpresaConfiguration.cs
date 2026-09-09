using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Organizacion;

public sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("OPT_Empresa");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .UseIdentityColumn()
               .HasColumnName("EmpresaId");

        // Identificador público no enumerable — expuesto en API/URLs en vez del Id interno (ADR 0004).
        builder.Property(e => e.PublicId)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(e => e.PublicId)
               .IsUnique()
               .HasDatabaseName("UQ_Empresas_PublicId");

        builder.Property(e => e.Nombre)
               .IsRequired()
               .HasMaxLength(100);

        // RUT: atributo de negocio único (no PK — mejora respecto al legacy)
        builder.Property(e => e.Rut)
               .IsRequired()
               .HasMaxLength(12);

        builder.HasIndex(e => e.Rut)
               .IsUnique()
               .HasDatabaseName("UQ_Empresas_Rut");

        builder.Property(e => e.RazonSocial)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(e => e.Giro)
               .HasMaxLength(150)
               .HasDefaultValue(string.Empty);

        builder.Property(e => e.Direccion)
               .HasMaxLength(200)
               .HasDefaultValue(string.Empty);

        builder.Property(e => e.Telefono)
               .HasMaxLength(20)
               .HasDefaultValue(string.Empty);

        builder.Property(e => e.Email)
               .HasMaxLength(150)
               .HasDefaultValue(string.Empty);

        // Contacto: persona de contacto (del legacy OPT_Empresa.Contacto)
        builder.Property(e => e.Contacto)
               .HasMaxLength(100)
               .HasDefaultValue(string.Empty);

        // ── Campos de auditoría (heredados de AuditableEntity) ───────────────
        builder.Property(e => e.CreadoEn).IsRequired();
        builder.Property(e => e.CreadoPor).IsRequired();
        builder.Property(e => e.ModificadoEn);
        builder.Property(e => e.ModificadoPor);
        builder.Property(e => e.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.EliminadoEn);
        builder.Property(e => e.EliminadoPor);

        // No seed — las empresas las crea el usuario administrador al configurar el sistema
    }
}
