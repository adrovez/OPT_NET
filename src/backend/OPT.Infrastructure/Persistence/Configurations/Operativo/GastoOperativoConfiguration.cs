using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Operativo;

namespace OPT.Infrastructure.Persistence.Configurations.Operativo;

/// <summary>
/// La relación con <c>Operativo</c> (FK <see cref="GastoOperativo.OperativoId"/>) se configura
/// desde <c>OperativoConfiguration</c> (lado <c>HasMany(o => o.Gastos)</c>).
/// </summary>
public sealed class GastoOperativoConfiguration : IEntityTypeConfiguration<GastoOperativo>
{
    public void Configure(EntityTypeBuilder<GastoOperativo> builder)
    {
        builder.ToTable("OPT_GastoOperativo");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
               .UseIdentityColumn()
               .HasColumnName("GastoOperativoId");

        builder.Property(g => g.OperativoId).IsRequired();
        builder.Property(g => g.Monto).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(g => g.NumeroDocumento).HasMaxLength(50);
        builder.Property(g => g.Observacion).HasMaxLength(500);

        builder.HasIndex(g => g.OperativoId).HasDatabaseName("IX_GastosOperativo_OperativoId");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(g => g.CreadoEn).IsRequired();
        builder.Property(g => g.CreadoPor).IsRequired();
        builder.Property(g => g.ModificadoEn);
        builder.Property(g => g.ModificadoPor);
        builder.Property(g => g.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(g => g.EliminadoEn);
        builder.Property(g => g.EliminadoPor);
    }
}
