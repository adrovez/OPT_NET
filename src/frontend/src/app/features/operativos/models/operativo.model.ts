/**
 * Refleja OperativoDto / OperativoResumenDto (OPT.Application/Features/Operativos) y el
 * catálogo OPT_EstadoOperativo. El Operativo se identifica por `publicId` (Guid) — no está
 * en el alcance de PublicId del ADR 0004 (no es dato personal), pero se creó ya así porque
 * es un recurso de primer nivel que agrupa Órdenes de Trabajo (mismo criterio que se le dio
 * a OrdenDeTrabajo en el ADR 0007).
 */
export interface EstadoOperativo {
  id: number;
  nombre: string;
  /** El backend lo calcula con EstadosOperativo.EsTerminal — no duplicar la regla aquí. */
  esTerminal: boolean;
  puedeAnularse: boolean;
}

/**
 * Ids fijos de OPT_EstadoOperativo (espejo de OPT.Domain/Entities/Operativo/EstadosOperativo.cs).
 * Solo para navegar el flujo (siguiente etapa) y reconocer ANULADO — la regla de qué es
 * terminal o anulable la manda el catálogo (`esTerminal`/`puedeAnularse`).
 */
export const ESTADOS_OPERATIVO = {
  prospecto: 1,
  ingresado: 2,
  cobranza: 3,
  cerrado: 4,
  anulado: 5,
} as const;

export interface OperativoResumen {
  publicId: string;
  correlativo: number;
  empresaPublicId: string;
  empresaNombre: string;
  sucursalId: number;
  sucursalNombre: string;
  estadoOperativoId: number;
  estadoOperativo: string;
  fecha: string;
  montoTotalVendido: number;
  montoTotalPagado: number;
  montoTotalGastos: number;
  /** Derivado por el backend: montoTotalPagado - montoTotalGastos. */
  gananciaPagado: number;
  /** Derivado por el backend: montoTotalVendido - montoTotalGastos. */
  gananciaVendido: number;
  creadoEn: string;
}

/** Orden de Trabajo asociada a un Operativo — subrecurso, sin PublicId propio (ADR 0007). */
export interface OperativoOT {
  ordenPublicId: string;
  numeroOT: number;
  clientePublicId: string;
  clienteNombre: string;
  estadoOTId: number;
  estadoOT: string;
  /** Snapshot al momento de asociar/recalcular — no se actualiza solo. */
  montoVendido: number;
  montoPagado: number;
}

/** Gasto del Operativo — subrecurso con Id interno, se identifica dentro del Operativo. */
export interface GastoOperativo {
  id: number;
  monto: number;
  numeroDocumento: string | null;
  observacion: string | null;
  fechaRegistro: string;
}

/** Vista completa: es la respuesta de TODO comando del agregado, no solo del GET. */
export interface Operativo extends OperativoResumen {
  observacion: string | null;
  ordenes: OperativoOT[];
  gastos: GastoOperativo[];
}

/** Campos de CrearOperativoCommand. Empresa y sucursal son inmutables tras crear. */
export interface OperativoCrear {
  empresaPublicId: string;
  sucursalId: number;
  fecha: string;
  observacion?: string | null;
}

/** Campos de ActualizarOperativoCommand — empresa y sucursal no son editables. */
export interface OperativoActualizar {
  fecha: string;
  observacion?: string | null;
}

/** Avance de una etapa (Prospecto→Ingresado→Cobranza→Cerrado). Sin retroceso ni salto. */
export interface CambioEstadoOperativo {
  nuevoEstadoId: number;
}

export interface AnulacionOperativo {
  motivo: string;
}

export interface AsociarOrdenOperativo {
  ordenPublicId: string;
}

export interface RegistrarGastoOperativo {
  monto: number;
  numeroDocumento?: string | null;
  observacion?: string | null;
}

/** Filtros acumulativos del listado, además de la paginación/búsqueda estándar. */
export interface FiltrosOperativos {
  empresaPublicId?: string | null;
  sucursalId?: number | null;
  estadoOperativoId?: number | null;
}
