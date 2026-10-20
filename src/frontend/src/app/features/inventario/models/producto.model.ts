/**
 * Refleja OPT.Application/Features/Productos/ProductoDto.cs. Se identifica por el `id`
 * interno (igual que Sucursal): Producto no es un dato personal/sensible, así que queda
 * fuera del alcance de `PublicId` del ADR 0004.
 *
 * En Marcos y Lentes de Sol la `descripcion` es calculada por el backend ("Marca-Modelo[-Color]",
 * ADR 0013); `marca`/`modelo`/`color` solo vienen informados en esas categorías.
 */
export interface Producto {
  id: number;
  codigo: string;
  descripcion: string;
  controlStock: boolean;
  categoriaId: number;
  marca: string | null;
  modelo: string | null;
  color: string | null;
}

/** Cuerpo de alta/edición. `descripcion` se ignora en Marcos y Lentes de Sol (la calcula el backend). */
export interface ProductoFormulario {
  codigo: string;
  descripcion: string | null;
  controlStock: boolean;
  categoriaId: number;
  marca: string | null;
  modelo: string | null;
  color: string | null;
}

/** Refleja OPT.Application/Features/CategoriasProducto/CategoriaProductoDto.cs. */
export interface CategoriaProducto {
  id: number;
  nombre: string;
}

/** Ids fijos del catálogo OPT_CategoriaProducto (script 014 / CategoriaProductoIds en el backend). */
export const CATEGORIA_PRODUCTO = {
  general: 1,
  marcos: 2,
  cristales: 3,
  lentesDeSol: 4,
  otros: 5,
} as const;

/** Marcos y Lentes de Sol se identifican por Marca + Modelo y su descripción es calculada. */
export function categoriaExigeMarcaYModelo(categoriaId: number): boolean {
  return (
    categoriaId === CATEGORIA_PRODUCTO.marcos || categoriaId === CATEGORIA_PRODUCTO.lentesDeSol
  );
}
