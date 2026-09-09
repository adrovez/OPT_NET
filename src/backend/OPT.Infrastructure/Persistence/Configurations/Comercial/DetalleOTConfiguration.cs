using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;
using OPT.Domain.Entities.Inventario;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

public sealed class DetalleOTConfiguration : IEntityTypeConfiguration<DetalleOT>
{
    public void Configure(EntityTypeBuilder<DetalleOT> builder)
    {
        builder.ToTable("OPT_DetalleOT");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
               .UseIdentityColumn()
               .HasColumnName("DetalleOTId");

        builder.Property(d => d.OrdenDeTrabajoId).IsRequired();
        builder.Property(d => d.ProductoId).IsRequired();
        builder.Property(d => d.Cantidad).IsRequired();
        builder.Property(d => d.ValorUnitario).HasColumnType("decimal(18,2)").IsRequired();

        // Anotación libre de la línea (armazón, color) — legacy OPT_OrdenDeTrabajoDetalle.Comentario.
        builder.Property(d => d.Comentario).HasMaxLength(200);

        // Total = Cantidad * ValorUnitario — propiedad calculada en memoria, no persistida.
        builder.Ignore(d => d.Total);

        builder.HasOne<Producto>()
               .WithMany()
               .HasForeignKey(d => d.ProductoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_DetallesOT_Productos");

        builder.HasIndex(d => d.OrdenDeTrabajoId).HasDatabaseName("IX_DetallesOT_OrdenDeTrabajoId");
        builder.HasIndex(d => d.ProductoId).HasDatabaseName("IX_DetallesOT_ProductoId");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(d => d.CreadoEn).IsRequired();
        builder.Property(d => d.CreadoPor).IsRequired();
        builder.Property(d => d.ModificadoEn);
        builder.Property(d => d.ModificadoPor);
        builder.Property(d => d.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(d => d.EliminadoEn);
        builder.Property(d => d.EliminadoPor);
    }
}
