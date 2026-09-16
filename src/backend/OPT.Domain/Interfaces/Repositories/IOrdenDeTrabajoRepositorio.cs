using OPT.Domain.Common;
using OPT.Domain.Entities.Comercial;

namespace OPT.Domain.Interfaces.Repositories;

/// <summary>
/// La OT es la raíz del agregado Comercial: detalles, abonos, pagos, cuotas y bitácora
/// se cargan y modifican siempre a través de ella, nunca con un repositorio propio.
/// La API la direcciona por <see cref="OrdenDeTrabajo.PublicId"/> (ADR 0004).
/// </summary>
public interface IOrdenDeTrabajoRepositorio : IRepositorioBase<OrdenDeTrabajo>
{
    /// <summary>Cabecera sola, por identificador público — para lecturas que no tocan las colecciones.</summary>
    Task<OrdenDeTrabajo?> ObtenerPorPublicIdAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>
    /// Agregado completo (detalles + abonos + pagos + cuotas + bitácora) por identificador
    /// público. Es la carga que necesita cualquier comando que modifique dinero o estado:
    /// el recálculo de totales se hace sobre las colecciones en memoria.
    /// </summary>
    Task<OrdenDeTrabajo?> ObtenerCompletaPorPublicIdAsync(Guid publicId, CancellationToken ct = default);

    Task<OrdenDeTrabajo?> ObtenerConDetallesAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// True si ya existe una OT con ese <paramref name="numeroOT"/>, creada en
    /// <paramref name="anio"/>, que no esté anulada — es la validación de duplicado del alta
    /// manual del N° de OT (regla 2026-09-11): una OT anulada libera su número para el mismo año.
    /// </summary>
    Task<bool> ExisteNumeroOTVigenteAsync(int numeroOT, int anio, CancellationToken ct = default);
    Task<IReadOnlyList<OrdenDeTrabajo>> ObtenerPorClienteAsync(int clienteId, CancellationToken ct = default);
    Task<IReadOnlyList<OrdenDeTrabajo>> ObtenerPorSucursalAsync(int sucursalId,
                                                                  int? estadoId = null,
                                                                  CancellationToken ct = default);

    /// <summary>
    /// Listado paginado. <see cref="ParametrosPaginacion.Busqueda"/> hace match contra el
    /// NumeroOT (si el término es numérico), el Beneficiario y el RUT/nombre/apellido del cliente.
    /// Los filtros son acumulativos y todos opcionales. <paramref name="operativoId"/> filtra por
    /// las OT asociadas a un Operativo (módulo Operativo, requerimiento sección 6 — Cobranza
    /// filtrada por Operativo reusa este mismo listado, igual que ya hace con empresaId).
    /// </summary>
    Task<(IReadOnlyList<OrdenDeTrabajo> Items, int Total)> BuscarPaginadoAsync(
        ParametrosPaginacion parametros,
        int? clienteId = null,
        int? sucursalId = null,
        int? estadoOTId = null,
        bool? soloConSaldo = null,
        int? empresaId = null,
        int? operativoId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Deuda vigente agrupada por empresa convenio (OT con saldo &gt; 0 que no estén anuladas).
    /// Sustituye al <c>sp_ListaDeudores</c> del legacy; la agrupación la hace la base de datos,
    /// no la capa de aplicación.
    /// </summary>
    Task<IReadOnlyList<ResumenDeudaEmpresa>> ObtenerDeudaPorEmpresaAsync(CancellationToken ct = default);
}
