using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

public sealed class AbonoConfiguration : IEntityTypeConfiguration<Abono>
{
    public void Configure(EntityTypeBuilder<Abono> builder)
    {
        builder.ToTable("OPT_Abono");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .UseIdentityColumn()
               .HasColumnName("AbonoId");

        builder.Property(a => a.OrdenDeTrabajoId).IsRequired();
        builder.Property(a => a.Monto).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(a => a.FormaPagoId).IsRequired();
        builder.Property(a => a.Referencia).HasMaxLength(50);

        builder.HasIndex(a => a.OrdenDeTrabajoId).HasDatabaseName("IX_Abonos_OrdenDeTrabajoId");

        builder.HasOne<FormaPago>()
               .WithMany()
               .HasForeignKey(a => a.FormaPagoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Abonos_FormasPago");

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
