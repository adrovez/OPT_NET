using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Clinico;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("OPT_Cliente");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .UseIdentityColumn()
               .HasColumnName("ClienteId");

        // Identificador público no enumerable — expuesto en API/URLs en vez del Id interno (ADR 0004).
        builder.Property(c => c.PublicId)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(c => c.PublicId)
               .IsUnique()
               .HasDatabaseName("UQ_Clientes_PublicId");

        // RUT: atributo único, no clave primaria (corrección crítica vs legacy OPT_Cliente, ADR 0003)
        builder.Property(c => c.Rut)
               .IsRequired()
               .HasMaxLength(12);

        builder.HasIndex(c => c.Rut)
               .IsUnique()
               .HasDatabaseName("UQ_Clientes_Rut");

        builder.Property(c => c.Nombre)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Apellido)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Email)
               .HasMaxLength(150);

        builder.Property(c => c.Telefono)
               .HasMaxLength(20);

        builder.Property(c => c.Direccion)
               .HasMaxLength(200);

        builder.Property(c => c.ComunaId);

        builder.Property(c => c.FechaNacimiento)
               .HasColumnType("date");

        builder.Property(c => c.TipoPrevision)
               .HasMaxLength(50);

        builder.HasOne<Comuna>()
               .WithMany()
               .HasForeignKey(c => c.ComunaId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Clientes_Comunas");

        builder.HasIndex(c => c.ComunaId)
               .HasDatabaseName("IX_Clientes_ComunaId");

        // ── Relaciones ────────────────────────────────────────────────────────
        builder.HasMany(c => c.Anamnesis)
               .WithOne(a => a.Cliente)
               .HasForeignKey(a => a.ClienteId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Anamnesis_Clientes");

        builder.HasMany(c => c.Recetas)
               .WithOne(r => r.Cliente)
               .HasForeignKey(r => r.ClienteId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_RecetasCristales_Clientes");

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
