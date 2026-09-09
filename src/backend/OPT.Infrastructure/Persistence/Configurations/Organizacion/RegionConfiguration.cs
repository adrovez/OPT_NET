using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Organizacion;

public sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("OPT_Region");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
               .ValueGeneratedNever(); // IDs fijos según datos INE

        builder.Property(r => r.Nombre)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(r => r.CodigoOficial)
               .IsRequired()
               .HasMaxLength(10);

        builder.HasIndex(r => r.CodigoOficial)
               .IsUnique()
               .HasDatabaseName("UQ_Regiones_CodigoOficial");

        builder.HasMany(r => r.Comunas)
               .WithOne(c => c.Region)
               .HasForeignKey(c => c.RegionId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Comunas_Regiones");

        // ── Seed: 16 regiones oficiales de Chile (orden número de región) ──
        builder.HasData(
            new { Id =  1, Nombre = "Región de Tarapacá",                              CodigoOficial = "I"     },
            new { Id =  2, Nombre = "Región de Antofagasta",                           CodigoOficial = "II"    },
            new { Id =  3, Nombre = "Región de Atacama",                               CodigoOficial = "III"   },
            new { Id =  4, Nombre = "Región de Coquimbo",                              CodigoOficial = "IV"    },
            new { Id =  5, Nombre = "Región de Valparaíso",                            CodigoOficial = "V"     },
            new { Id =  6, Nombre = "Región del Libertador General Bernardo O'Higgins",CodigoOficial = "VI"    },
            new { Id =  7, Nombre = "Región del Maule",                                CodigoOficial = "VII"   },
            new { Id =  8, Nombre = "Región del Biobío",                               CodigoOficial = "VIII"  },
            new { Id =  9, Nombre = "Región de La Araucanía",                          CodigoOficial = "IX"    },
            new { Id = 10, Nombre = "Región de Los Lagos",                             CodigoOficial = "X"     },
            new { Id = 11, Nombre = "Región de Aysén del General Carlos Ibáñez del Campo", CodigoOficial = "XI" },
            new { Id = 12, Nombre = "Región de Magallanes y de la Antártica Chilena",  CodigoOficial = "XII"   },
            new { Id = 13, Nombre = "Región Metropolitana de Santiago",                CodigoOficial = "XIII"  },
            new { Id = 14, Nombre = "Región de Los Ríos",                              CodigoOficial = "XIV"   },
            new { Id = 15, Nombre = "Región de Arica y Parinacota",                   CodigoOficial = "XV"    },
            new { Id = 16, Nombre = "Región de Ñuble",                                CodigoOficial = "XVI"   }
        );
    }
}
