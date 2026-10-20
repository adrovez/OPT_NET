using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Inventario;

namespace OPT.Infrastructure.Persistence.Configurations.Inventario;

public sealed class ProductoBodegaConfiguration : IEntityTypeConfiguration<ProductoBodega>
{
    public void Configure(EntityTypeBuilder<ProductoBodega> builder)
    {
        builder.ToTable("OPT_ProductoBodega", t =>
            t.HasCheckConstraint("CK_ProductoBodegas_StockNoNegativo", "[StockActual] >= 0"));

        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.Id)
               .UseIdentityColumn()
               .HasColumnName("ProductoBodegaId");

        builder.Property(pb => pb.ProductoId).IsRequired();
        builder.Property(pb => pb.BodegaId).IsRequired();
        builder.Property(pb => pb.StockActual).IsRequired().HasDefaultValue(0);
        builder.Property(pb => pb.StockMinimo).IsRequired().HasDefaultValue(0);
        builder.Property(pb => pb.StockMaximo).IsRequired().HasDefaultValue(0);

        builder.HasOne<Producto>()
               .WithMany()
               .HasForeignKey(pb => pb.ProductoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_ProductoBodegas_Productos");

        builder.HasOne<Bodega>()
               .WithMany()
               .HasForeignKey(pb => pb.BodegaId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_ProductoBodegas_Bodegas");

        // El stock se controla por producto+bodega de forma independiente.
        builder.HasIndex(pb => new { pb.ProductoId, pb.BodegaId })
               .IsUnique()
               .HasDatabaseName("UQ_ProductoBodegas_ProductoBodega");

        builder.HasIndex(pb => pb.BodegaId)
               .HasDatabaseName("IX_ProductoBodegas_BodegaId");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(pb => pb.CreadoEn).IsRequired();
        builder.Property(pb => pb.CreadoPor).IsRequired();
        builder.Property(pb => pb.ModificadoEn);
        builder.Property(pb => pb.ModificadoPor);
        builder.Property(pb => pb.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(pb => pb.EliminadoEn);
        builder.Property(pb => pb.EliminadoPor);
    }
}
