import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import { Producto } from '../models/producto.model';

/**
 * Catálogo de productos (solo lectura). Su único consumidor hoy es el selector de producto
 * del detalle de una Orden de Trabajo — el resto del módulo Inventario (stock, traslados,
 * alta de productos) sigue sin API en el backend.
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
}
