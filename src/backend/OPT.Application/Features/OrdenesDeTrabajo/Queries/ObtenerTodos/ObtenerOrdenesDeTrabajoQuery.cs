using MediatR;
using OPT.Domain.Common;

namespace OPT.Application.Features.OrdenesDeTrabajo.Queries.ObtenerTodos;

/// <summary>
/// Listado paginado de Órdenes de Trabajo (12.578 filas migradas — nunca lista completa).
/// Hereda <see cref="ParametrosPaginacion"/>: pagina, tamanioPagina, busqueda (número de OT,
/// beneficiario o RUT/nombre del cliente), ordenarPor
/// (numeroOT|fechaEntrega|precio|saldo|estado|creadoEn) y direccionOrden (asc|desc).
/// Los filtros adicionales son acumulativos y opcionales.
/// </summary>
public record ObtenerOrdenesDeTrabajoQuery : ParametrosPaginacion, IRequest<PagedResult<OrdenDeTrabajoResumenDto>>
{
    public Guid? ClientePublicId { get; init; }
    public int?  SucursalId      { get; init; }
    public int?  EstadoOTId      { get; init; }

    /// <summary>Empresa convenio — es el filtro que usa el detalle de un deudor en Cobranza.</summary>
    public Guid? EmpresaPublicId { get; init; }

    /// <summary>Solo las OT con saldo pendiente (cobranza).</summary>
    public bool? SoloConSaldo    { get; init; }

    /// <summary>OT asociadas a un Operativo — filtro que pide el requerimiento del módulo Operativo (sección 6).</summary>
    public Guid? OperativoPublicId { get; init; }

    /// <summary>Solo OT de venta en Sucursal — sin ningún Operativo asociado (HU-OT-02).</summary>
    public bool? SoloSucursal { get; init; }
}
