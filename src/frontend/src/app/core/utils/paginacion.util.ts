import { ParametrosConsultaPaginada } from '../../shared/models/parametros-consulta-paginada.model';

/**
 * Convierte los parámetros de una consulta paginada en el objeto `params` de `HttpClient`,
 * omitiendo `busqueda` / `ordenarPor` / `direccionOrden` cuando están vacíos (así la URL
 * queda limpia y el backend aplica sus valores por defecto).
 */
export function aHttpParams(p: ParametrosConsultaPaginada): Record<string, string | number> {
  const params: Record<string, string | number> = {
    pagina: p.pagina,
    tamanioPagina: p.tamanioPagina,
  };

  const busqueda = p.busqueda?.trim();
  if (busqueda) {
    params['busqueda'] = busqueda;
  }
  if (p.ordenarPor) {
    params['ordenarPor'] = p.ordenarPor;
    params['direccionOrden'] = p.direccionOrden ?? 'asc';
  }

  return params;
}
