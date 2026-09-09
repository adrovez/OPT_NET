using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Organizacion;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("OPT_Usuario");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
               .UseIdentityColumn()
               .HasColumnName("UsuarioId");

        // Identificador público no enumerable — expuesto en API/URLs en vez del Id interno (ADR 0004).
        builder.Property(u => u.PublicId)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(u => u.PublicId)
               .IsUnique()
               .HasDatabaseName("UQ_Usuarios_PublicId");

        // RUT: atributo único, no clave primaria (corrección crítica vs legacy donde era PK)
        builder.Property(u => u.Rut)
               .IsRequired()
               .HasMaxLength(12);

        builder.HasIndex(u => u.Rut)
               .IsUnique()
               .HasDatabaseName("UQ_Usuarios_Rut");

        builder.Property(u => u.Nombre)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.Apellido)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(150);

        builder.HasIndex(u => u.Email)
               .IsUnique()
               .HasDatabaseName("UQ_Usuarios_Email");

        // ClaveHash: BCrypt work factor 12 — nunca texto plano (corrección crítica vs legacy)
        builder.Property(u => u.ClaveHash)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.RolId)
               .IsRequired();

        builder.Property(u => u.SucursalActivaId);

        builder.Property(u => u.Activo)
               .IsRequired()
               .HasDefaultValue(true);

        // ── Relaciones ────────────────────────────────────────────────────────
        builder.HasOne(u => u.Rol)
               .WithMany()
               .HasForeignKey(u => u.RolId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Usuarios_Roles");

        builder.HasOne(u => u.SucursalActiva)
               .WithMany()
               .HasForeignKey(u => u.SucursalActivaId)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("FK_Usuarios_Sucursales_Activa");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(u => u.CreadoEn).IsRequired();
        builder.Property(u => u.CreadoPor).IsRequired();
        builder.Property(u => u.ModificadoEn);
        builder.Property(u => u.ModificadoPor);
        builder.Property(u => u.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(u => u.EliminadoEn);
        builder.Property(u => u.EliminadoPor);

        // No seed de usuario administrador (según decisión del equipo)
    }
}
