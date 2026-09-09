using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Clinico;

namespace OPT.Infrastructure.Persistence.Configurations.Clinico;

public sealed class AnamnesisConfiguration : IEntityTypeConfiguration<Anamnesis>
{
    public void Configure(EntityTypeBuilder<Anamnesis> builder)
    {
        builder.ToTable("OPT_Anamnesis");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .UseIdentityColumn()
               .HasColumnName("AnamnesisId");

        // Identificador público no enumerable — expuesto en API/URLs en vez del Id interno (ADR 0004).
        builder.Property(a => a.PublicId)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(a => a.PublicId)
               .IsUnique()
               .HasDatabaseName("UQ_Anamnesis_PublicId");

        builder.Property(a => a.ClienteId).IsRequired();

        builder.Property(a => a.Hipertension).IsRequired();
        builder.Property(a => a.Diabetes).IsRequired();
        builder.Property(a => a.Alergias).IsRequired();
        builder.Property(a => a.DetalleAlergias).HasMaxLength(500);
        builder.Property(a => a.UsaLentesPrevio).IsRequired();
        builder.Property(a => a.Observaciones).HasMaxLength(500);

        builder.HasIndex(a => a.ClienteId)
               .HasDatabaseName("IX_Anamnesis_ClienteId");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(a => a.CreadoEn).IsRequired();
        builder.Property(a => a.CreadoPor).IsRequired();
        builder.Property(a => a.ModificadoEn);
        builder.Property(a => a.ModificadoPor);
        builder.Property(a => a.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(a => a.EliminadoEn);
        builder.Property(a => a.EliminadoPor);
    }
}
