/**
 * Catálogos del módulo Comercial (OPT_EstadoOT, OPT_FormaPago, OPT_EstadoCuota). Son
 * sembrados y de solo lectura; se exponen con el Id interno porque no son datos personales
 * (mismo criterio que Roles/Regiones/Comunas).
 */
export interface EstadoOT {
  id: number;
  nombre: string;
  /** El backend lo calcula con EstadosOT.EsTerminal — no duplicar la regla en el frontend. */
  esTerminal: boolean;
}

export interface FormaPago {
  id: number;
  nombre: string;
}

export interface EstadoCuota {
  id: number;
  nombre: string;
}

/**
 * Ids fijos de OPT_EstadoOT (espejo de OPT.Domain/Entities/Comercial/EstadosOT.cs).
 * Se usan solo para navegar el flujo (siguiente/anterior) y para reconocer ANULADO;
 * la regla de qué es terminal la manda `esTerminal` del backend.
 */
export const ESTADOS_OT = {
  ingresado: 0,
  enProceso: 1,
  montaje: 2,
  laboratorio: 3,
  calidad: 4,
  despacho: 5,
  entregado: 6,
  anulado: 7,
} as const;

/** Ids de OPT_EstadoCuota (espejo de EstadosCuota del dominio). */
export const ESTADOS_CUOTA = {
  pendiente: 1,
  pagada: 2,
  anulada: 3,
} as const;
