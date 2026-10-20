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

        // ── Seed: el legacy no modela categorías de producto. "General" se conserva; las 4 reales
        //    se agregaron en el script 014 (ADR 0013). Los ids viven en CategoriaProductoIds. ──
        builder.HasData(
            new { Id = CategoriaProductoIds.General,     Nombre = "General" },
            new { Id = CategoriaProductoIds.Marcos,      Nombre = "Marcos" },
            new { Id = CategoriaProductoIds.Cristales,   Nombre = "Cristales" },
            new { Id = CategoriaProductoIds.LentesDeSol, Nombre = "Lentes de Sol" },
            new { Id = CategoriaProductoIds.Otros,       Nombre = "Otros" }
        );
    }
}
