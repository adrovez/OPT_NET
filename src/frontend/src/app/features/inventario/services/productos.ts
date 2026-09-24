import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import { Producto, ProductoFormulario } from '../models/producto.model';

/**
 * Catálogo de productos: listado paginado (también lo usa el selector de producto del detalle
 * de una Orden de Trabajo) y alta / edición / baja lógica. Stock y traslados siguen sin API.
 */
@Injectable({
  providedIn: 'root',
})
export class Productos {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/productos`;

  /** Búsqueda paginada — 4.018 productos migrados, nunca se pide la lista completa. */
  buscar(parametros: ParametrosConsultaPaginada): Observable<PagedResult<Producto>> {
    return this.http.get<PagedResult<Producto>>(this.baseUrl, { params: aHttpParams(parametros) });
  }

  crear(datos: ProductoFormulario): Observable<Producto> {
    return this.http.post<Producto>(this.baseUrl, datos);
  }

  actualizar(id: number, datos: ProductoFormulario): Observable<Producto> {
    return this.http.put<Producto>(`${this.baseUrl}/${id}`, datos);
  }

  darDeBaja(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
