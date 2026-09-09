using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Entities.Comercial;
using OPT.Domain.Entities.Organizacion;

namespace OPT.Infrastructure.Persistence.Configurations.Comercial;

public sealed class OrdenDeTrabajoConfiguration : IEntityTypeConfiguration<OrdenDeTrabajo>
{
    private const string PrecisionMonto = "decimal(18,2)";

    public void Configure(EntityTypeBuilder<OrdenDeTrabajo> builder)
    {
        builder.ToTable("OPT_OrdenDeTrabajo");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
               .UseIdentityColumn()
               .HasColumnName("OrdenDeTrabajoId");

        // Identificador no enumerable expuesto en API/URLs — generado por la BD (ADR 0004).
        builder.Property(o => o.PublicId)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(o => o.PublicId)
               .IsUnique()
               .HasDatabaseName("UQ_OrdenesDeTrabajo_PublicId");

        // Número visible al cliente — generado por SEQUENCE en la BD, nunca en la capa de aplicación (ADR 0003).
        builder.Property(o => o.NumeroOT)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasDefaultValueSql("NEXT VALUE FOR [dbo].[SEQ_NumeroOT]");

        builder.HasIndex(o => o.NumeroOT)
               .IsUnique()
               .HasDatabaseName("UQ_OrdenesDeTrabajo_NumeroOT");

        builder.Property(o => o.ClienteId).IsRequired();
        builder.Property(o => o.SucursalId).IsRequired();
        builder.Property(o => o.EstadoOTId).IsRequired();
        builder.Property(o => o.EmpresaId);

        builder.Property(o => o.Precio).HasColumnType(PrecisionMonto).IsRequired();
        builder.Property(o => o.TotalAbonado).HasColumnType(PrecisionMonto).IsRequired();
        builder.Property(o => o.Saldo).HasColumnType(PrecisionMonto).IsRequired();

        builder.Property(o => o.Observaciones).HasMaxLength(500);
        builder.Property(o => o.FechaEntrega).IsRequired();

        // ── Campos heredados del legacy (004_comercial_pagos_cuotas.sql) ──────
        builder.Property(o => o.Beneficiario).HasMaxLength(100);
        builder.Property(o => o.FechaAtencion).HasColumnType("date");
        builder.Property(o => o.HoraEntrega).HasColumnType("time(0)");
        builder.Property(o => o.NumeroCuotas);

        // Derivada de EstadoOTId — no se persiste.
        builder.Ignore(o => o.EstaAnulada);

        // ── Relaciones ────────────────────────────────────────────────────────
        builder.HasOne<Cliente>()
               .WithMany()
               .HasForeignKey(o => o.ClienteId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_OrdenesDeTrabajo_Clientes");

        builder.HasOne<Sucursal>()
               .WithMany()
               .HasForeignKey(o => o.SucursalId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_OrdenesDeTrabajo_Sucursales");

        builder.HasOne<Empresa>()
               .WithMany()
               .HasForeignKey(o => o.EmpresaId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_OrdenesDeTrabajo_Empresas");

        builder.HasOne<EstadoOT>()
               .WithMany()
               .HasForeignKey(o => o.EstadoOTId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_OrdenesDeTrabajo_EstadosOT");

        builder.HasMany(o => o.Detalles)
               .WithOne()
               .HasForeignKey(d => d.OrdenDeTrabajoId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_DetallesOT_OrdenesDeTrabajo");

        builder.HasMany(o => o.Abonos)
               .WithOne()
               .HasForeignKey(a => a.OrdenDeTrabajoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Abonos_OrdenesDeTrabajo");

        builder.HasMany(o => o.Pagos)
               .WithOne()
               .HasForeignKey(p => p.OrdenDeTrabajoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Pagos_OrdenesDeTrabajo");

        builder.HasMany(o => o.Cuotas)
               .WithOne()
               .HasForeignKey(c => c.OrdenDeTrabajoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Cuotas_OrdenesDeTrabajo");

        builder.HasMany(o => o.Bitacora)
               .WithOne()
               .HasForeignKey(b => b.OrdenDeTrabajoId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_BitacoraOT_OrdenesDeTrabajo");

        builder.HasIndex(o => o.ClienteId).HasDatabaseName("IX_OrdenesDeTrabajo_ClienteId");
        builder.HasIndex(o => o.SucursalId).HasDatabaseName("IX_OrdenesDeTrabajo_SucursalId");
        builder.HasIndex(o => o.EstadoOTId).HasDatabaseName("IX_OrdenesDeTrabajo_EstadoOTId");

        // ── Campos de auditoría ───────────────────────────────────────────────
        builder.Property(o => o.CreadoEn).IsRequired();
        builder.Property(o => o.CreadoPor).IsRequired();
        builder.Property(o => o.ModificadoEn);
        builder.Property(o => o.ModificadoPor);
        builder.Property(o => o.Eliminado).IsRequired().HasDefaultValue(false);
        builder.Property(o => o.EliminadoEn);
        builder.Property(o => o.EliminadoPor);
    }
}
