using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Operativo;

namespace OPT.Infrastructure.Persistence.Configurations.Operativo;

public sealed class EstadoOperativoConfiguration : IEntityTypeConfiguration<EstadoOperativo>
{
    public void Configure(EntityTypeBuilder<EstadoOperativo> builder)
    {
        builder.ToTable("OPT_EstadoOperativo");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .ValueGeneratedNever(); // IDs fijos del catálogo

        builder.Property(e => e.Nombre)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(e => e.Nombre)
               .IsUnique()
               .HasDatabaseName("UQ_EstadosOperativo_Nombre");

        // ── Seed: estados sembrados por 009_modulo_operativo.sql ──
        builder.HasData(
            new { Id = 1, Nombre = "PROSPECTO" },
            new { Id = 2, Nombre = "INGRESADO" },
            new { Id = 3, Nombre = "COBRANZA"  },
            new { Id = 4, Nombre = "CERRADO"   },
            new { Id = 5, Nombre = "ANULADO"   }
        );
    }
}
