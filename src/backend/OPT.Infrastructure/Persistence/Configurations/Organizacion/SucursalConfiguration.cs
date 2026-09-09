using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Organizacion;

public sealed class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("OPT_Sucursal");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
               .UseIdentityColumn()
               .HasColumnName("SucursalId");

        builder.Property(s => s.Nombre)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.Direccion)
               .HasMaxLength(200)
               .HasDefaultValue(string.Empty);

        builder.Property(s => s.Telefono)
               .HasMaxLength(20)
               .HasDefaultValue(string.Empty);

        // EsMatriz: equivalente al flag Matriz del legacy OPT_Sucursal
        builder.Property(s => s.EsMatriz)
               .IsRequired()
               .HasDefaultValue(false);

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(s => s.CreadoEn).IsRequired();
        builder.Property(s => s.CreadoPor).IsRequired();
        builder.Property(s => s.ModificadoEn);
        builder.Property(s => s.ModificadoPor);
        builder.Property(s => s.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(s => s.EliminadoEn);
        builder.Property(s => s.EliminadoPor);

        // Índice: solo puede haber una sucursal matriz activa
        builder.HasIndex(s => s.EsMatriz)
               .HasFilter("[EsMatriz] = 1 AND [Eliminado] = 0")
               .IsUnique()
               .HasDatabaseName("UQ_Sucursales_UnicaMatriz");

        // No seed — la sucursal inicial la crea el administrador durante el setup
    }
}
