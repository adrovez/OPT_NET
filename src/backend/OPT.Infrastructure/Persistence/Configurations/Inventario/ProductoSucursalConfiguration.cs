using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Inventario;

public sealed class ProductoSucursalConfiguration : IEntityTypeConfiguration<ProductoSucursal>
{
    public void Configure(EntityTypeBuilder<ProductoSucursal> builder)
    {
        builder.ToTable("OPT_ProductoSucursal");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Id)
               .UseIdentityColumn()
               .HasColumnName("ProductoSucursalId");

        builder.Property(ps => ps.ProductoId).IsRequired();
        builder.Property(ps => ps.SucursalId).IsRequired();

        builder.Property(ps => ps.StockActual).IsRequired();
        builder.Property(ps => ps.StockMinimo).IsRequired();
        builder.Property(ps => ps.StockMaximo).IsRequired();
        builder.Property(ps => ps.PrecioVenta).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasOne<Sucursal>()
               .WithMany()
               .HasForeignKey(ps => ps.SucursalId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_ProductoSucursales_Sucursales");

        // El stock se controla por producto+sucursal de forma independiente (preservar del legacy).
        builder.HasIndex(ps => new { ps.ProductoId, ps.SucursalId })
               .IsUnique()
               .HasDatabaseName("UQ_ProductoSucursales_ProductoSucursal");

        builder.HasIndex(ps => ps.SucursalId)
               .HasDatabaseName("IX_ProductoSucursales_SucursalId");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(ps => ps.CreadoEn).IsRequired();
        builder.Property(ps => ps.CreadoPor).IsRequired();
        builder.Property(ps => ps.ModificadoEn);
        builder.Property(ps => ps.ModificadoPor);
        builder.Property(ps => ps.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(ps => ps.EliminadoEn);
        builder.Property(ps => ps.EliminadoPor);
    }
}
