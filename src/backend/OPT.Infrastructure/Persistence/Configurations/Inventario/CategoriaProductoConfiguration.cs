using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Inventario;

namespace OPT.Infrastructure.Persistence.Configurations.Inventario;

public sealed class CategoriaProductoConfiguration : IEntityTypeConfiguration<CategoriaProducto>
{
    public void Configure(EntityTypeBuilder<CategoriaProducto> builder)
    {
        builder.ToTable("OPT_CategoriaProducto");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .ValueGeneratedNever(); // IDs fijos de catálogo

        builder.Property(c => c.Nombre)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(c => c.Nombre)
               .IsUnique()
               .HasDatabaseName("UQ_CategoriasProducto_Nombre");

        // ── Seed: el legacy no modela categorías de producto — se parte con una sola ──
        builder.HasData(
            new { Id = 1, Nombre = "General" }
        );
    }
}
