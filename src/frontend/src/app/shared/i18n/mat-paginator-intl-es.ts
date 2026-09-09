import { MatPaginatorIntl } from '@angular/material/paginator';

/** Traducción al español de las etiquetas fijas de MatPaginator (la librería solo trae inglés). */
export function crearMatPaginatorIntlEs(): MatPaginatorIntl {
  const intl = new MatPaginatorIntl();

  intl.itemsPerPageLabel = 'Elementos por página:';
  intl.nextPageLabel = 'Página siguiente';
  intl.previousPageLabel = 'Página anterior';
  intl.firstPageLabel = 'Primera página';
  intl.lastPageLabel = 'Última página';
  intl.getRangeLabel = (pagina: number, tamanioPagina: number, total: number): string => {
    if (total === 0 || tamanioPagina === 0) {
      return `0 de ${total}`;
    }

    const inicio = pagina * tamanioPagina;
    const fin = Math.min(inicio + tamanioPagina, total);
    return `${inicio + 1} – ${fin} de ${total}`;
  };

  return intl;
}
