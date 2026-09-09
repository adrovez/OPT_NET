using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

public sealed class FormaPagoConfiguration : IEntityTypeConfiguration<FormaPago>
{
    public void Configure(EntityTypeBuilder<FormaPago> builder)
    {
        builder.ToTable("OPT_FormaPago");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
               .ValueGeneratedNever(); // IDs fijos de catálogo (heredados del legacy)

        builder.Property(f => f.Nombre)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(f => f.Nombre)
               .IsUnique()
               .HasDatabaseName("UQ_FormasPago_Nombre");

        // ── Seed: medios de pago aceptados (mismos ids/nombres del legacy) ──
        builder.HasData(
            new { Id = 0, Nombre = "SIN INFORMACION"   },
            new { Id = 1, Nombre = "EFECTIVO"          },
            new { Id = 2, Nombre = "TARJETA CREDITO"   },
            new { Id = 3, Nombre = "TARJETA DEBITO"    },
            new { Id = 4, Nombre = "TRANSFERENCIA"     },
            // Agregado por 004_comercial_pagos_cuotas.sql — el legacy lo usaba como texto libre.
            new { Id = 5, Nombre = "CHEQUE"            }
        );
    }
}
