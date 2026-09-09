using OPT.Application.Features.RecetaCristales;
using OPT.Domain.Entities.Inventario;
using OPT.Domain.Interfaces.Repositories;
using EntidadOT = OPT.Domain.Entities.Comercial.OrdenDeTrabajo;

namespace OPT.Application.Features.OrdenesDeTrabajo;

/// <summary>
/// Ensambla la vista completa de una OT (cabecera + hijos + nombres de catálogo).
/// Existe porque los nueve comandos del agregado devuelven exactamente la misma forma:
/// duplicar el mapeo en cada handler sería la vía rápida a que se desincronicen.
///
/// Se registra explícitamente en <c>AddApplication</c> (no lo alcanza el escaneo de MediatR).
/// </summary>
public sealed class OrdenDeTrabajoDtoFactory(
    IClienteRepositorio     clienteRepo,
    ISucursalRepositorio    sucursalRepo,
    IEmpresaRepositorio     empresaRepo,
    IProductoRepositorio    productoRepo,
    IEstadoOTRepositorio    estadoOTRepo,
    IFormaPagoRepositorio   formaPagoRepo,
    IEstadoCuotaRepositorio estadoCuotaRepo,
    IComunaRepositorio      comunaRepo,
    IRecetaCristalesRepositorio recetaRepo)
{
    public async Task<OrdenDeTrabajoDto> CrearAsync(EntidadOT ot, CancellationToken ct)
    {
        var cliente   = await clienteRepo.ObtenerPorIdAsync(ot.ClienteId, ct);
        var sucursal  = await sucursalRepo.ObtenerPorIdAsync(ot.SucursalId, ct);
        var empresa   = ot.EmpresaId is null ? null : await empresaRepo.ObtenerPorIdAsync(ot.EmpresaId.Value, ct);

        var estados      = (await estadoOTRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id, e => e.Nombre);
        var formasPago   = (await formaPagoRepo.ObtenerTodosAsync(ct)).ToDictionary(f => f.Id, f => f.Nombre);
        var estadosCuota = (await estadoCuotaRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id, e => e.Nombre);

        // Comuna/Región del cliente: la pestaña Cliente las muestra por nombre, y resolverlas
        // en el frontend obligaría a bajar el catálogo completo de las 346 comunas.
        var comuna = cliente?.ComunaId is null
            ? null
            : await comunaRepo.ObtenerPorIdAsync(cliente.ComunaId.Value, ct);

        // Recetas materializadas en esta OT (legacy idOT) — no "las del cliente": es la
        // prescripción con la que se fabricaron estos cristales.
        var recetas = await recetaRepo.ObtenerPorOrdenAsync(ot.Id, ct);

        var detalles = ot.Detalles.Where(d => !d.Eliminado).ToList();
        var productoIds = detalles.Select(d => d.ProductoId).Distinct().ToList();
        var productos = new Dictionary<int, Producto>();
        if (productoIds.Count > 0)
            productos = (await productoRepo.BuscarAsync(p => productoIds.Contains(p.Id), ct))
                .ToDictionary(p => p.Id);

        return new OrdenDeTrabajoDto(
            ot.PublicId, ot.NumeroOT,
            cliente?.PublicId ?? Guid.Empty, cliente?.Rut ?? string.Empty,
            cliente is null ? string.Empty : $"{cliente.Nombre} {cliente.Apellido}".Trim(),
            ot.SucursalId, sucursal?.Nombre ?? string.Empty,
            empresa?.PublicId, empresa?.Nombre,
            ot.EstadoOTId, Nombre(estados, ot.EstadoOTId),
            ot.Precio, ot.TotalAbonado, ot.Saldo,
            ot.Observaciones, ot.FechaEntrega, ot.Beneficiario,
            ot.FechaAtencion, ot.HoraEntrega, ot.NumeroCuotas,

            new ClienteOTDto(
                cliente?.PublicId ?? Guid.Empty,
                cliente?.Rut ?? string.Empty,
                cliente is null ? string.Empty : $"{cliente.Nombre} {cliente.Apellido}".Trim(),
                cliente?.Email, cliente?.Telefono, cliente?.Direccion,
                cliente?.ComunaId, comuna?.Nombre, comuna?.Region?.Nombre,
                cliente?.FechaNacimiento, cliente?.TipoPrevision),

            recetas
                .Select(r => RecetaCristalesMapper.Mapear(r, cliente?.PublicId ?? Guid.Empty))
                .ToList(),

            detalles
                .OrderBy(d => d.Id)
                .Select(d => new DetalleOTDto(
                    d.Id, d.ProductoId,
                    productos.TryGetValue(d.ProductoId, out var p) ? p.Codigo : null,
                    productos.TryGetValue(d.ProductoId, out var p2) ? p2.Descripcion : null,
                    d.Cantidad, d.ValorUnitario, d.Total, d.Comentario))
                .ToList(),

            ot.Abonos.Where(a => !a.Eliminado)
                .OrderBy(a => a.CreadoEn)
                .Select(a => new AbonoDto(a.Id, a.Monto, a.FormaPagoId,
                    Nombre(formasPago, a.FormaPagoId), a.Referencia, a.CreadoEn))
                .ToList(),

            ot.Pagos.Where(p => !p.Eliminado)
                .OrderBy(p => p.FechaPago)
                .Select(p => new PagoDto(p.Id, p.Monto, p.FormaPagoId,
                    Nombre(formasPago, p.FormaPagoId), p.Referencia, p.FechaPago))
                .ToList(),

            ot.Cuotas.Where(c => !c.Eliminado)
                .OrderBy(c => c.Numero)
                .Select(c => new CuotaDto(c.Id, c.Numero, c.ValorCuota, c.FechaVencimiento,
                    c.FechaPago, c.FormaPagoId,
                    c.FormaPagoId is null ? null : Nombre(formasPago, c.FormaPagoId.Value),
                    c.EstadoCuotaId, Nombre(estadosCuota, c.EstadoCuotaId)))
                .ToList(),

            ot.Bitacora.Where(b => !b.Eliminado)
                .OrderByDescending(b => b.CreadoEn)
                .Select(b => new BitacoraOTDto(b.Id,
                    b.EstadoAnteriorId, Nombre(estados, b.EstadoAnteriorId),
                    b.EstadoNuevoId,    Nombre(estados, b.EstadoNuevoId),
                    b.Observacion, b.CreadoEn, b.CreadoPor))
                .ToList());
    }

    private static string Nombre(IReadOnlyDictionary<int, string> catalogo, int id)
        => catalogo.TryGetValue(id, out var nombre) ? nombre : string.Empty;
}
