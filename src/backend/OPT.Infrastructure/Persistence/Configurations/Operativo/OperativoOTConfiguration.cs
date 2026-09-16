using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Comercial;
using OPT.Domain.Entities.Operativo;

namespace OPT.Infrastructure.Persistence.Configurations.Operativo;

/// <summary>
/// Relación pura Operativo↔OrdenDeTrabajo, sin auditoría — mismo patrón que
/// <c>EmpresaSucursalConfiguration</c>. Una OT pertenece a lo sumo un Operativo: índice único
/// en <see cref="OperativoOT.OrdenDeTrabajoId"/>. La relación con <c>Operativo</c> (FK
/// <see cref="OperativoOT.OperativoId"/>) se configura desde <c>OperativoConfiguration</c>
/// (lado <c>HasMany(o => o.Ordenes)</c>) — no se repite acá, mismo criterio que
/// <c>DetalleOTConfiguration</c>/<c>OrdenDeTrabajoConfiguration</c>.
/// </summary>
public sealed class OperativoOTConfiguration : IEntityTypeConfiguration<OperativoOT>
{
    private const string PrecisionMonto = "decimal(18,2)";

    public void Configure(EntityTypeBuilder<OperativoOT> builder)
    {
        builder.ToTable("OPT_OperativoOT");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
               .UseIdentityColumn()
               .HasColumnName("OperativoOTId");

        builder.Property(r => r.OperativoId).IsRequired();
        builder.Property(r => r.OrdenDeTrabajoId).IsRequired();

        builder.Property(r => r.MontoVendidoSnapshot).HasColumnType(PrecisionMonto).IsRequired();
        builder.Property(r => r.MontoPagadoSnapshot).HasColumnType(PrecisionMonto).IsRequired();

        builder.HasOne<OrdenDeTrabajo>()
               .WithMany()
               .HasForeignKey(r => r.OrdenDeTrabajoId)
               .OnDelete(DeleteBehavior.NoAction)
               .HasConstraintName("FK_OperativoOT_OrdenesDeTrabajo");

        builder.HasIndex(r => r.OrdenDeTrabajoId)
               .IsUnique()
               .HasDatabaseName("UQ_OperativoOT_OrdenDeTrabajoId");

        builder.HasIndex(r => r.OperativoId)
               .HasDatabaseName("IX_OperativoOT_OperativoId");
    }
}
