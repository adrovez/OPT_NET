/**
 * Refleja UsuarioDto (OPT.Application/Features/Usuarios). Se identifica por `publicId`
 * (Guid), nunca por el Id interno (ADR 0004).
 *
 * NOTA: el backend solo expone `sucursalActivaId` (la sucursal activa actual), no la
 * lista completa de sucursales asignadas — no hay endpoint para listarlas. Las acciones
 * de asignar/quitar sucursal operan "a ciegas" respecto de ese estado.
 */
export interface Usuario {
  publicId: string;
  rut: string;
  nombre: string;
  apellido: string;
  email: string | null;
  rolId: number;
  rolNombre: string;
  sucursalActivaId: number | null;
  activo: boolean;
}

/** Campos que acepta CrearUsuarioCommand. */
export interface UsuarioCrear {
  rut: string;
  nombre: string;
  apellido: string;
  clave: string;
  rolId: number;
  email?: string | null;
}

/** Campos que acepta ActualizarUsuarioCommand — la clave se cambia aparte (CambiarClave). */
export interface UsuarioActualizar {
  rut: string;
  nombre: string;
  apellido: string;
  rolId: number;
  email?: string | null;
}

/** Campos que acepta CambiarClaveUsuarioCommand. */
export interface UsuarioCambiarClave {
  claveActual: string;
  claveNueva: string;
}
