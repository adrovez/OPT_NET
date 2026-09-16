using Microsoft.EntityFrameworkCore;
using OPT.Domain.Entities.Clinico;
using OPT.Domain.Entities.Comercial;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Entities.Organizacion;
using OPT.Infrastructure.Persistence.Interceptors;
using EntidadOperativo = OPT.Domain.Entities.Operativo.Operativo;
using OperativoOT = OPT.Domain.Entities.Operativo.OperativoOT;
using GastoOperativo = OPT.Domain.Entities.Operativo.GastoOperativo;
using EstadoOperativo = OPT.Domain.Entities.Operativo.EstadoOperativo;

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

    // ── Operativo ────────────────────────────────────────────────────────────
    public DbSet<EntidadOperativo> Operativos         => Set<EntidadOperativo>();
    public DbSet<OperativoOT>      OperativoOT        => Set<OperativoOT>();
    public DbSet<GastoOperativo>   GastosOperativo    => Set<GastoOperativo>();
    public DbSet<EstadoOperativo>  EstadosOperativo   => Set<EstadoOperativo>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SEQ_NumeroOT ya no se usa: NumeroOT se ingresa manualmente (decisión 2026-09-11).
        // Se mantiene declarada porque el objeto sigue existiendo en la BD (008_numero_ot_manual.sql
        // le quita el DEFAULT a la columna pero no elimina la SEQUENCE, por si se necesita de respaldo).
        modelBuilder.HasSequence<int>("SEQ_NumeroOT", schema: "dbo")
                    .StartsAt(1)
                    .IncrementsBy(1);

        // Correlativo del Operativo — autogenerado (decisión 2026-09-15, punto 8.5 del
        // requerimiento), script 009_modulo_operativo.sql.
        modelBuilder.HasSequence<int>("SEQ_CorrelativoOperativo", schema: "dbo")
                    .StartsAt(1)
                    .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
