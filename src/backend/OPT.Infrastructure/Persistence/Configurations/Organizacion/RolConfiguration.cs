using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Organizacion;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("OPT_Rol");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
               .ValueGeneratedNever(); // IDs fijos de catálogo

        builder.Property(r => r.Nombre)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(r => r.Nombre)
               .IsUnique()
               .HasDatabaseName("UQ_Roles_Nombre");

        // ── Seed: roles del sistema (1-3 genéricos + 4-8 roles reales heredados del legacy) ──
        builder.HasData(
            new { Id = 1, Nombre = "Administrador"  },
            new { Id = 2, Nombre = "Supervisor"     },
            new { Id = 3, Nombre = "Operador"       },
            new { Id = 4, Nombre = "Jefe Sucursal"  },
            new { Id = 5, Nombre = "Vendedor"       },
            new { Id = 6, Nombre = "Tecnico Medico" },
            new { Id = 7, Nombre = "Control Calidad"},
            new { Id = 8, Nombre = "Externo"        }
        );
    }
}
