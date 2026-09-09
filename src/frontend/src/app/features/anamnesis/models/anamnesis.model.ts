/**
 * Refleja OPT.Application/Features/Anamnesis/AnamnesisDto.cs.
 * Se identifica por `publicId` (Guid), nunca por el Id interno (ADR 0004).
 */
export interface Anamnesis {
  publicId: string;
  clientePublicId: string;
  hipertension: boolean;
  diabetes: boolean;
  alergias: boolean;
  detalleAlergias: string | null;
  usaLentesPrevio: boolean;
  observaciones: string | null;
  /** Fecha en que se registró la ficha (audit `CreadoEn`; en datos migrados = fecha del legacy). ISO 8601. */
  fechaRegistro: string;
}

/** Campos que acepta CrearAnamnesisCommand/ActualizarAnamnesisCommand. */
export interface AnamnesisFormulario {
  hipertension: boolean;
  diabetes: boolean;
  alergias: boolean;
  detalleAlergias?: string | null;
  usaLentesPrevio: boolean;
  observaciones?: string | null;
}
