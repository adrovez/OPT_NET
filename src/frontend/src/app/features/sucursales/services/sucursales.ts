import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import { Sucursal, SucursalCrear, SucursalFormulario } from '../models/sucursal.model';

/** Encapsula el acceso HTTP al módulo Sucursales — Id interno en las rutas (ver modelo). */
@Injectable({
  providedIn: 'root',
})
export class Sucursales {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/sucursales`;

  /**
   * Listado completo — lo consumen los combos de otros módulos (p. ej. el mapa de nombres
   * de sucursal y el diálogo de sucursales del usuario). El backend devuelve una página;
   * se pide un tamaño amplio porque Sucursal es un catálogo chico.
   */
  listar(): Observable<Sucursal[]> {
    return this.http
      .get<PagedResult<Sucursal>>(this.baseUrl, { params: { pagina: 1, tamanioPagina: 100 } })
      .pipe(map((r) => r.items));
  }

  /** Listado paginado del lado del servidor. `busqueda` hace match contra nombre y dirección. */
  buscar(parametros: ParametrosConsultaPaginada): Observable<PagedResult<Sucursal>> {
    return this.http.get<PagedResult<Sucursal>>(this.baseUrl, { params: aHttpParams(parametros) });
  }

  obtener(id: number): Observable<Sucursal> {
    return this.http.get<Sucursal>(`${this.baseUrl}/${id}`);
  }

  crear(datos: SucursalCrear): Observable<Sucursal> {
    return this.http.post<Sucursal>(this.baseUrl, datos);
  }

  actualizar(id: number, datos: SucursalFormulario): Observable<Sucursal> {
    return this.http.put<Sucursal>(`${this.baseUrl}/${id}`, datos);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
