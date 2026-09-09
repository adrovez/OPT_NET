/**
 * Refleja OPT.Application/Features/Clientes/ClienteDto.cs.
 * Se identifica por `publicId` (Guid), nunca por el Id interno (ADR 0004).
 */
export interface Cliente {
  publicId: string;
  rut: string;
  nombre: string;
  apellido: string;
  email: string | null;
  telefono: string | null;
  direccion: string | null;
  comunaId: number | null;
  fechaNacimiento: string | null;
  tipoPrevision: string | null;
}

/** Campos que acepta CrearClienteCommand — Rut solo se puede fijar al crear. */
export interface ClienteCrear {
  rut: string;
  nombre: string;
  apellido: string;
  email?: string | null;
  telefono?: string | null;
  direccion?: string | null;
  comunaId?: number | null;
  fechaNacimiento?: string | null;
  tipoPrevision?: string | null;
}

/** Campos que acepta ActualizarClienteCommand — sin Rut (inmutable tras la creación). */
export type ClienteActualizar = Omit<ClienteCrear, 'rut'>;
