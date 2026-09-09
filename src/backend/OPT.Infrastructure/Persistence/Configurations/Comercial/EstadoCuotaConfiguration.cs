using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

public sealed class EstadoCuotaConfiguration : IEntityTypeConfiguration<EstadoCuota>
{
    public void Configure(EntityTypeBuilder<EstadoCuota> builder)
    {
        builder.ToTable("OPT_EstadoCuota");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .ValueGeneratedNever(); // IDs fijos de catálogo

        builder.Property(e => e.Nombre)
               .IsRequired()
               .HasMaxLength(100);

        // ── Seed: mismos ids/nombres sembrados por 004_comercial_pagos_cuotas.sql ──
        builder.HasData(
            new { Id = EstadosCuota.Pendiente, Nombre = "PENDIENTE" },
            new { Id = EstadosCuota.Pagada,    Nombre = "PAGADA"    },
            new { Id = EstadosCuota.Anulada,   Nombre = "ANULADA"   }
        );
    }
}
