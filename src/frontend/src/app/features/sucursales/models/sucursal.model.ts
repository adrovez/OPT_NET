/**
 * Refleja SucursalDto (OPT.Application/Features/Sucursales). Se identifica por `id`
 * interno, no por `publicId` — Sucursal no está en el alcance de PublicId del ADR 0004
 * (no es un dato personal/sensible).
 */
export interface Sucursal {
  id: number;
  nombre: string;
  direccion: string | null;
  telefono: string | null;
  esMatriz: boolean;
}

/** Campos que acepta ActualizarSucursalCommand — EsMatriz no es editable tras la creación. */
export interface SucursalFormulario {
  nombre: string;
  direccion?: string | null;
  telefono?: string | null;
}

/** Campos que acepta CrearSucursalCommand. */
export interface SucursalCrear extends SucursalFormulario {
  esMatriz: boolean;
}
