using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

public sealed class BitacoraOTConfiguration : IEntityTypeConfiguration<BitacoraOT>
{
    public void Configure(EntityTypeBuilder<BitacoraOT> builder)
    {
        builder.ToTable("OPT_BitacoraOT");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
               .UseIdentityColumn()
               .HasColumnName("BitacoraOTId");

        builder.Property(b => b.OrdenDeTrabajoId).IsRequired();
        builder.Property(b => b.EstadoAnteriorId).IsRequired();
        builder.Property(b => b.EstadoNuevoId).IsRequired();
        builder.Property(b => b.Observacion).HasMaxLength(200);

        builder.HasIndex(b => b.OrdenDeTrabajoId).HasDatabaseName("IX_BitacoraOT_OrdenDeTrabajoId");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(b => b.CreadoEn).IsRequired();
        builder.Property(b => b.CreadoPor).IsRequired();
        builder.Property(b => b.ModificadoEn);
        builder.Property(b => b.ModificadoPor);
        builder.Property(b => b.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.EliminadoEn);
        builder.Property(b => b.EliminadoPor);
    }
}
