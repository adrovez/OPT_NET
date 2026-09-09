/** Refleja OPT.Domain/Common/PagedResult.cs (los 3 últimos campos los calcula el backend). */
export interface PagedResult<T> {
  items: T[];
  pagina: number;
  tamanioPagina: number;
  total: number;
  totalPaginas: number;
  tienePaginaAnterior: boolean;
  tienePaginaSiguiente: boolean;
}
