/**
 * Refleja OPT.Application/Features/RecetaCristales/RecetaCristalesDto.cs.
 * Se identifica por `publicId` (Guid), nunca por el Id interno (ADR 0004).
 * OD = ojo derecho, OI = ojo izquierdo. DpLejos/DpCerca/AddLejos son texto libre
 * (ver 003_extras_cliente_receta.sql en CLAUDE.md raíz — sin formato fijo en el legacy).
 */
export interface RecetaCristales {
  publicId: string;
  clientePublicId: string;
  odEsferaLejos: number | null;
  odCilindroLejos: number | null;
  odEjeLejos: number | null;
  odEsferaCerca: number | null;
  odCilindroCerca: number | null;
  odEjeCerca: number | null;
  oiEsferaLejos: number | null;
  oiCilindroLejos: number | null;
  oiEjeLejos: number | null;
  oiEsferaCerca: number | null;
  oiCilindroCerca: number | null;
  oiEjeCerca: number | null;
  urgente: boolean;
  requiereLab: boolean;
  observaciones: string | null;
  dpLejos: string | null;
  dpCerca: string | null;
  addLejos: string | null;
  /**
   * "Incluir Cristales Lejos/Cerca" del legacy (`CheckLejos`/`CheckCerca`): indican si el bloque
   * aplica a esta receta, independiente de si sus campos numéricos están completos. Cuando están
   * en `true`, las tres observaciones del bloque (OD/OI/DP) son obligatorias (007_receta_incluir_observaciones_detalle.sql).
   */
  incluirLejos: boolean;
  incluirCerca: boolean;
  /** Observación por ojo/DP — separadas de `observaciones` (que sigue siendo la nota general). */
  observacionOdLejos: string | null;
  observacionOiLejos: string | null;
  observacionDpLejos: string | null;
  observacionOdCerca: string | null;
  observacionOiCerca: string | null;
  observacionDpCerca: string | null;
  /** Fecha en que se registró la receta (audit `CreadoEn`; en datos migrados = fecha del legacy). ISO 8601. */
  fechaRegistro: string;
}

/** Campos que acepta CrearRecetaCristalesCommand/ActualizarRecetaCristalesCommand. */
export type RecetaCristalesFormulario = Omit<
  RecetaCristales,
  'publicId' | 'clientePublicId' | 'fechaRegistro'
>;
