using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Entities.Comercial;

namespace OPT.Infrastructure.Persistence.Configurations.Clinico;

public sealed class RecetaCristalesConfiguration : IEntityTypeConfiguration<RecetaCristales>
{
    // Rango típico de graduación óptica: -30.00 a +30.00 en pasos de 0.25.
    private const string PrecisionGraduacion = "decimal(5,2)";

    public void Configure(EntityTypeBuilder<RecetaCristales> builder)
    {
        builder.ToTable("OPT_RecetaCristales");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
               .UseIdentityColumn()
               .HasColumnName("RecetaCristalesId");

        // Identificador público no enumerable — expuesto en API/URLs en vez del Id interno (ADR 0004).
        builder.Property(r => r.PublicId)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(r => r.PublicId)
               .IsUnique()
               .HasDatabaseName("UQ_RecetasCristales_PublicId");

        builder.Property(r => r.ClienteId).IsRequired();

        builder.Property(r => r.OdEsferaLejos).HasColumnType(PrecisionGraduacion);
        builder.Property(r => r.OdCilindroLejos).HasColumnType(PrecisionGraduacion);
        builder.Property(r => r.OdEjeLejos);

        builder.Property(r => r.OdEsferaCerca).HasColumnType(PrecisionGraduacion);
        builder.Property(r => r.OdCilindroCerca).HasColumnType(PrecisionGraduacion);
        builder.Property(r => r.OdEjeCerca);

        builder.Property(r => r.OiEsferaLejos).HasColumnType(PrecisionGraduacion);
        builder.Property(r => r.OiCilindroLejos).HasColumnType(PrecisionGraduacion);
        builder.Property(r => r.OiEjeLejos);

        builder.Property(r => r.OiEsferaCerca).HasColumnType(PrecisionGraduacion);
        builder.Property(r => r.OiCilindroCerca).HasColumnType(PrecisionGraduacion);
        builder.Property(r => r.OiEjeCerca);

        builder.Property(r => r.Urgente).IsRequired();
        builder.Property(r => r.RequiereLab).IsRequired();
        builder.Property(r => r.Observaciones).HasMaxLength(500);

        // Texto libre — ver comentario en RecetaCristales.cs sobre por qué DP/ADD no son decimal.
        builder.Property(r => r.DpLejos).HasMaxLength(20);
        builder.Property(r => r.DpCerca).HasMaxLength(20);
        builder.Property(r => r.AddLejos).HasMaxLength(20);

        // "Incluir Cristales Lejos/Cerca" — ver comentario de RecetaCristales.IncluirLejos/IncluirCerca.
        builder.Property(r => r.IncluirLejos).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.IncluirCerca).IsRequired().HasDefaultValue(false);

        // Observación por ojo/DP para Lejos y Cerca — mismo largo que el legacy (maxlength 50 en
        // OPT_RecetaCristales.LejosODObservacion y análogas).
        builder.Property(r => r.ObservacionOdLejos).HasMaxLength(50);
        builder.Property(r => r.ObservacionOiLejos).HasMaxLength(50);
        builder.Property(r => r.ObservacionDpLejos).HasMaxLength(50);
        builder.Property(r => r.ObservacionOdCerca).HasMaxLength(50);
        builder.Property(r => r.ObservacionOiCerca).HasMaxLength(50);
        builder.Property(r => r.ObservacionDpCerca).HasMaxLength(50);

        // Receta materializada en una OT (legacy OPT_RecetaCristales.idOT). Nullable: la ficha
        // clínica puede dejar una receta sin orden todavía.
        builder.Property(r => r.OrdenDeTrabajoId);

        builder.HasOne<OrdenDeTrabajo>()
               .WithMany()
               .HasForeignKey(r => r.OrdenDeTrabajoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_RecetasCristales_OrdenesDeTrabajo");

        builder.HasIndex(r => r.ClienteId)
               .HasDatabaseName("IX_RecetasCristales_ClienteId");

        builder.HasIndex(r => r.OrdenDeTrabajoId)
               .HasDatabaseName("IX_RecetasCristales_OrdenDeTrabajoId");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(r => r.CreadoEn).IsRequired();
        builder.Property(r => r.CreadoPor).IsRequired();
        builder.Property(r => r.ModificadoEn);
        builder.Property(r => r.ModificadoPor);
        builder.Property(r => r.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.EliminadoEn);
        builder.Property(r => r.EliminadoPor);
    }
}
