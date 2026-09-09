/**
 * Refleja OPT.Application/Features/Productos/ProductoDto.cs. Se identifica por el `id`
 * interno (igual que Sucursal): Producto no es un dato personal/sensible, así que queda
 * fuera del alcance de `PublicId` del ADR 0004.
 */
export interface Producto {
  id: number;
  codigo: string;
  descripcion: string;
  controlStock: boolean;
  categoriaId: number;
}
