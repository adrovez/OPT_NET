/**
 * Parámetros que todo listado paginado envía a la API como query params. Coincide 1:1
 * con `OPT.Domain/Common/ParametrosPaginacion` del backend.
 *
 * `GET /api/{recurso}?pagina=1&tamanioPagina=20&busqueda=texto&ordenarPor=nombre&direccionOrden=asc`
 */
export interface ParametrosConsultaPaginada {
  pagina: number;
  tamanioPagina: number;
  /** Búsqueda tipo "Google": un solo término contra varios campos (los define el backend por recurso). */
  busqueda?: string;
  ordenarPor?: string;
  direccionOrden?: 'asc' | 'desc';
}

export const TAMANIO_PAGINA_DEFECTO = 20;

/** Opciones de tamaño de página para `<mat-paginator>` — mismas en todos los listados. */
export const OPCIONES_TAMANIO_PAGINA: readonly number[] = [10, 20, 50];
