using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

public sealed class EstadoOTConfiguration : IEntityTypeConfiguration<EstadoOT>
{
    public void Configure(EntityTypeBuilder<EstadoOT> builder)
    {
        builder.ToTable("OPT_EstadoOT");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .ValueGeneratedNever(); // IDs fijos de catálogo (heredados del legacy)

        builder.Property(e => e.Nombre)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(e => e.Nombre)
               .IsUnique()
               .HasDatabaseName("UQ_EstadosOT_Nombre");

        // ── Seed: estados del ciclo de vida de una OT (mismos ids/nombres del legacy) ──
        builder.HasData(
            new { Id = 0, Nombre = "INGRESADO"   },
            new { Id = 1, Nombre = "EN PROCESO"  },
            new { Id = 2, Nombre = "MONTAJE"     },
            new { Id = 3, Nombre = "LABORATORIO" },
            new { Id = 4, Nombre = "CALIDAD"     },
            new { Id = 5, Nombre = "DESPACHO"    },
            new { Id = 6, Nombre = "ENTREGADO"   },
            // Agregado por el sistema nuevo (005_ot_publicid_estado_anulado.sql): el legacy
            // anulaba con SP_OTEliminar, sin estado en el catálogo.
            new { Id = 7, Nombre = "ANULADO"     }
        );
    }
}
