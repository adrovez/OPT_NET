/**
 * Refleja OPT.Application/Features/Cobranza/DeudorEmpresaDto.cs — deuda vigente consolidada
 * por empresa convenio (OT con saldo > 0 que no estén anuladas).
 *
 * `empresaPublicId` es null en la fila que agrupa las OT particulares. La empresa se
 * referencia por `publicId`, nunca por su Id interno (ADR 0004).
 */
export interface DeudorEmpresa {
  empresaPublicId: string | null;
  empresaNombre: string;
  empresaRut: string | null;
  cantidadOT: number;
  totalPrecio: number;
  totalAbonado: number;
  saldo: number;
}
