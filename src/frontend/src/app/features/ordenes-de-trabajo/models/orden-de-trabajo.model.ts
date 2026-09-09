import { RecetaCristales } from '../../receta-cristales/models/receta-cristales.model';

/**
 * Refleja OPT.Application/Features/OrdenesDeTrabajo/OrdenDeTrabajoDto.cs.
 *
 * La OT se identifica por `publicId` (Guid), nunca por el Id interno (ADR 0004). `numeroOT`
 * es el correlativo visible que se comunica al cliente: se muestra, pero no se usa como
 * identificador de ruta. Los hijos (detalle, abonos, pagos, cuotas, bitácora) sí llevan su
 * Id interno — solo son alcanzables anidados bajo la OT, que ya está protegida (ADR 0007).
 */
export interface OrdenDeTrabajoResumen {
  publicId: string;
  numeroOT: number;
  clientePublicId: string;
  clienteRut: string;
  clienteNombre: string;
  sucursalId: number;
  sucursalNombre: string;
  estadoOTId: number;
  estadoOT: string;
  precio: number;
  totalAbonado: number;
  saldo: number;
  fechaEntrega: string;
  creadoEn: string;
}

/**
 * Ficha del cliente embebida en la OT — es lo que muestra la pestaña Cliente de la orden
 * (equivale a la pestaña del mismo nombre del modal "Detalle Orden de Trabajo" del legacy).
 * Viene dentro de la OT y no en una llamada aparte: `comunaNombre`/`regionNombre` los
 * resuelve el backend porque el catálogo de comunas son 346 filas.
 */
export interface ClienteOT {
  publicId: string;
  rut: string;
  nombre: string;
  email: string | null;
  telefono: string | null;
  direccion: string | null;
  comunaId: number | null;
  comunaNombre: string | null;
  regionNombre: string | null;
  fechaNacimiento: string | null;
  tipoPrevision: string | null;
}

export interface DetalleOT {
  id: number;
  productoId: number;
  productoCodigo: string | null;
  productoDescripcion: string | null;
  cantidad: number;
  valorUnitario: number;
  total: number;
  /** Anotación libre de la línea (modelo/color del armazón) — legacy `Comentario`. */
  comentario: string | null;
}

/** Abono inicial de la OT. `fechaRegistro` viene del audit `CreadoEn` del backend. */
export interface Abono {
  id: number;
  monto: number;
  formaPagoId: number;
  formaPago: string;
  referencia: string | null;
  fechaRegistro: string;
}

/** Cobro posterior al abono inicial (ADR 0006) — además imputa las cuotas pendientes. */
export interface Pago {
  id: number;
  monto: number;
  formaPagoId: number;
  formaPago: string;
  referencia: string | null;
  fechaPago: string;
}

export interface Cuota {
  id: number;
  numero: number;
  valorCuota: number;
  fechaVencimiento: string;
  fechaPago: string | null;
  formaPagoId: number | null;
  formaPago: string | null;
  estadoCuotaId: number;
  estadoCuota: string;
}

export interface BitacoraOT {
  id: number;
  estadoAnteriorId: number;
  estadoAnterior: string;
  estadoNuevoId: number;
  estadoNuevo: string;
  observacion: string | null;
  fecha: string;
  usuarioId: number;
}

/** Vista completa: es la respuesta de TODO comando del agregado, no solo del GET. */
export interface OrdenDeTrabajo extends OrdenDeTrabajoResumen {
  empresaPublicId: string | null;
  empresaNombre: string | null;
  observaciones: string | null;
  beneficiario: string | null;
  fechaAtencion: string | null;
  horaEntrega: string | null;
  numeroCuotas: number | null;
  cliente: ClienteOT;
  /**
   * Recetas materializadas en esta OT (legacy `OPT_RecetaCristales.idOT`): la prescripción
   * con la que se fabricaron **estos** cristales, no la última del cliente. Normalmente una;
   * es lista porque 2 órdenes migradas tienen dos.
   */
  recetas: RecetaCristales[];
  detalles: DetalleOT[];
  abonos: Abono[];
  pagos: Pago[];
  cuotas: Cuota[];
  bitacora: BitacoraOT[];
}

/** Línea del detalle tal como la envía el cliente de la API (alta y edición). */
export interface LineaDetalleOT {
  productoId: number;
  cantidad: number;
  valorUnitario: number;
  comentario?: string | null;
}

/**
 * Campos de CrearOrdenDeTrabajoCommand. El precio NO se envía: lo calcula el backend como
 * la suma de las líneas. El abono inicial y el plan de cuotas son opcionales y se registran
 * en la misma transacción — es el flujo real del mesón.
 */
export interface OrdenDeTrabajoCrear {
  clientePublicId: string;
  sucursalId: number;
  fechaEntrega: string;
  detalles: LineaDetalleOT[];
  empresaPublicId?: string | null;
  /** Receta ya tomada en la ficha clínica que se materializa en esta orden. */
  recetaPublicId?: string | null;
  observaciones?: string | null;
  beneficiario?: string | null;
  fechaAtencion?: string | null;
  horaEntrega?: string | null;
  abonoInicial?: number | null;
  formaPagoAbono?: number | null;
  referenciaAbono?: string | null;
  numeroCuotas?: number | null;
  primerVencimiento?: string | null;
}

/**
 * Campos de ActualizarOrdenDeTrabajoCommand. Sin cliente ni sucursal (no son editables).
 * Si `detalles` viene, reemplaza TODAS las líneas y el precio se recalcula; si se omite,
 * el detalle no se toca.
 */
export interface OrdenDeTrabajoActualizar {
  fechaEntrega: string;
  detalles?: LineaDetalleOT[] | null;
  empresaPublicId?: string | null;
  /** Estado final, igual que `empresaPublicId`: si va null la orden queda sin receta. */
  recetaPublicId?: string | null;
  observaciones?: string | null;
  beneficiario?: string | null;
  fechaAtencion?: string | null;
  horaEntrega?: string | null;
}

/** Avance/retroceso de una etapa. La observación es obligatoria al retroceder. */
export interface CambioEstadoOT {
  nuevoEstadoId: number;
  observacion?: string | null;
}

export interface AnulacionOT {
  motivo: string;
}

export interface AbonoRegistrar {
  monto: number;
  formaPagoId: number;
  referencia?: string | null;
}

export interface PagoRegistrar {
  monto: number;
  formaPagoId: number;
  fechaPago?: string | null;
  referencia?: string | null;
}

export interface PlanCuotasGenerar {
  numeroCuotas: number;
  primerVencimiento: string;
}

/** Pago manual de una cuota (regularización) — el flujo normal es registrar un Pago. */
export interface CuotaPagar {
  formaPagoId?: number | null;
  fechaPago?: string | null;
}

/** Filtros acumulativos del listado, además de la paginación/búsqueda estándar. */
export interface FiltrosOrdenesDeTrabajo {
  clientePublicId?: string | null;
  empresaPublicId?: string | null;
  sucursalId?: number | null;
  estadoOTId?: number | null;
  soloConSaldo?: boolean | null;
}
