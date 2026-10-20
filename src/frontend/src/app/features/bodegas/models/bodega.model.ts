/**
 * Refleja BodegaDto (OPT.Application/Features/Bodegas). Se identifica por `publicId`
 * (ADR 0004 / 0012); `sucursalId` es el Id interno de la sucursal, igual que en el resto del
 * frontend (Sucursal no se expone por PublicId).
 */
export interface Bodega {
  publicId: string;
  sucursalId: number;
  codigo: string;
  nombre: string;
  esPrincipal: boolean;
  activa: boolean;
}

/** Campos que acepta ActualizarBodegaCommand — la sucursal no se puede cambiar. */
export interface BodegaFormulario {
  codigo: string;
  nombre: string;
}

/** Campos que acepta CrearBodegaCommand. */
export interface BodegaCrear extends BodegaFormulario {
  sucursalId: number;
  /** Si la sucursal aún no tiene bodegas, el backend la fuerza a principal. */
  esPrincipal: boolean;
}

/** Filtros propios del listado de bodegas, además de los de paginación. */
export interface FiltrosBodegas {
  sucursalId?: number;
  soloActivas?: boolean;
}
