using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Organizacion;

public sealed class EmpresaSucursalConfiguration : IEntityTypeConfiguration<EmpresaSucursal>
{
    public void Configure(EntityTypeBuilder<EmpresaSucursal> builder)
    {
        builder.ToTable("OPT_EmpresaSucursal");

        builder.HasKey(es => es.Id);

        builder.Property(es => es.Id)
               .UseIdentityColumn()
               .HasColumnName("EmpresaSucursalId");

        builder.Property(es => es.EmpresaId).IsRequired();
        builder.Property(es => es.SucursalId).IsRequired();

        builder.HasOne(es => es.Empresa)
               .WithMany(e => e.Sucursales)
               .HasForeignKey(es => es.EmpresaId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_EmpresaSucursales_Empresas");

        builder.HasOne(es => es.Sucursal)
               .WithMany()
               .HasForeignKey(es => es.SucursalId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_EmpresaSucursales_Sucursales");

        builder.HasIndex(es => new { es.EmpresaId, es.SucursalId })
               .IsUnique()
               .HasDatabaseName("UQ_EmpresaSucursales_EmpresaSucursal");

        builder.HasIndex(es => es.SucursalId)
               .HasDatabaseName("IX_EmpresaSucursales_SucursalId");
    }
}
