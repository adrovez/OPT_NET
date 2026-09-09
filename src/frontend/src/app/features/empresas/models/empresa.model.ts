/**
 * Refleja EmpresaDto (OPT.Application/Features/Empresas). Se identifica por `publicId`
 * (Guid), nunca por el Id interno (ADR 0004).
 */
export interface Empresa {
  publicId: string;
  nombre: string;
  rut: string;
  razonSocial: string;
  giro: string;
  direccion: string;
  telefono: string;
  email: string;
  contacto: string;
}

/** Campos que aceptan CrearEmpresaCommand / ActualizarEmpresaCommand (mismo shape). */
export type EmpresaFormulario = Omit<Empresa, 'publicId'>;
