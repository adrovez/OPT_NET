using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Organizacion;

public sealed class UsuarioSucursalConfiguration : IEntityTypeConfiguration<UsuarioSucursal>
{
    public void Configure(EntityTypeBuilder<UsuarioSucursal> builder)
    {
        builder.ToTable("OPT_UsuarioSucursal");

        builder.HasKey(us => us.Id);

        builder.Property(us => us.Id)
               .UseIdentityColumn()
               .HasColumnName("UsuarioSucursalId");

        // FK por int UsuarioId (corrección crítica: legacy usaba string RutUsuario como FK)
        builder.Property(us => us.UsuarioId).IsRequired();
        builder.Property(us => us.SucursalId).IsRequired();

        builder.HasOne(us => us.Usuario)
               .WithMany(u => u.Sucursales)
               .HasForeignKey(us => us.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_UsuarioSucursales_Usuarios");

        builder.HasOne(us => us.Sucursal)
               .WithMany(s => s.Usuarios)
               .HasForeignKey(us => us.SucursalId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_UsuarioSucursales_Sucursales");

        // Un usuario no puede estar asignado dos veces a la misma sucursal
        builder.HasIndex(us => new { us.UsuarioId, us.SucursalId })
               .IsUnique()
               .HasDatabaseName("UQ_UsuarioSucursales_UsuarioSucursal");

        builder.HasIndex(us => us.SucursalId)
               .HasDatabaseName("IX_UsuarioSucursales_SucursalId");
    }
}
