using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Entities.Comercial;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Entities.Organizacion;
using OPT.Infrastructure.Persistence.Interceptors;

namespace OPT.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly AuditInterceptor _auditInterceptor;

    public AppDbContext(DbContextOptions<AppDbContext> options, AuditInterceptor auditInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    // ── Organización ─────────────────────────────────────────────────────────
    public DbSet<Region>          Regiones          => Set<Region>();
    public DbSet<Comuna>          Comunas            => Set<Comuna>();
    public DbSet<Empresa>         Empresas           => Set<Empresa>();
    public DbSet<EmpresaSucursal> EmpresaSucursales  => Set<EmpresaSucursal>();
    public DbSet<Sucursal>        Sucursales         => Set<Sucursal>();
    public DbSet<Rol>             Roles              => Set<Rol>();
    public DbSet<Usuario>         Usuarios           => Set<Usuario>();
    public DbSet<UsuarioSucursal> UsuarioSucursales  => Set<UsuarioSucursal>();

    // ── Clínico ──────────────────────────────────────────────────────────────
    public DbSet<Cliente>          Clientes          => Set<Cliente>();
    public DbSet<Anamnesis>        Anamnesis         => Set<Anamnesis>();
    public DbSet<RecetaCristales>  RecetasCristales  => Set<RecetaCristales>();

    // ── Comercial ────────────────────────────────────────────────────────────
    public DbSet<OrdenDeTrabajo>  OrdenesDeTrabajo  => Set<OrdenDeTrabajo>();
    public DbSet<DetalleOT>       DetallesOT        => Set<DetalleOT>();
    public DbSet<Abono>           Abonos            => Set<Abono>();
    public DbSet<BitacoraOT>      BitacoraOT        => Set<BitacoraOT>();
    public DbSet<Pago>            Pagos             => Set<Pago>();
    public DbSet<Cuota>           Cuotas            => Set<Cuota>();
    public DbSet<EstadoOT>        EstadosOT         => Set<EstadoOT>();
    public DbSet<FormaPago>       FormasPago        => Set<FormaPago>();
    public DbSet<EstadoCuota>     EstadosCuota      => Set<EstadoCuota>();

    // ── Inventario ───────────────────────────────────────────────────────────
    public DbSet<Producto>          Productos          => Set<Producto>();
    public DbSet<ProductoSucursal>  ProductoSucursal   => Set<ProductoSucursal>();
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // NumeroOT se genera en la BD vía SEQUENCE — nunca calculado en la capa de aplicación (ADR 0003).
        modelBuilder.HasSequence<int>("SEQ_NumeroOT", schema: "dbo")
                    .StartsAt(1)
                    .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
