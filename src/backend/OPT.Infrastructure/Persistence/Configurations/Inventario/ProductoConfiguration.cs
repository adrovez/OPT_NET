using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Inventario;

namespace OPT.Infrastructure.Persistence.Configurations.Inventario;

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("OPT_Producto");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .UseIdentityColumn()
               .HasColumnName("ProductoId");

        // Código de catálogo: atributo único, no clave primaria.
        builder.Property(p => p.Codigo)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(p => p.Codigo)
               .IsUnique()
               .HasDatabaseName("UQ_Productos_Codigo");

        builder.Property(p => p.Descripcion)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(p => p.ControlStock).IsRequired();
        builder.Property(p => p.CategoriaId).IsRequired();

        builder.HasIndex(p => p.CategoriaId)
               .HasDatabaseName("IX_Productos_CategoriaId");

        builder.HasOne<CategoriaProducto>()
               .WithMany()
               .HasForeignKey(p => p.CategoriaId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Productos_CategoriasProducto");

        builder.HasMany(p => p.Sucursales)
               .WithOne()
               .HasForeignKey(ps => ps.ProductoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_ProductoSucursales_Productos");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(p => p.CreadoEn).IsRequired();
        builder.Property(p => p.CreadoPor).IsRequired();
        builder.Property(p => p.ModificadoEn);
        builder.Property(p => p.ModificadoPor);
        builder.Property(p => p.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.EliminadoEn);
        builder.Property(p => p.EliminadoPor);
    }
}
