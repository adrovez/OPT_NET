import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import { Bodega, BodegaCrear, BodegaFormulario, FiltrosBodegas } from '../models/bodega.model';

/** Encapsula el acceso HTTP al módulo Bodegas — siempre por `publicId` (ADR 0012). */
@Injectable({
  providedIn: 'root',
})
export class Bodegas {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/bodegas`;

  /** Listado paginado. `busqueda` hace match contra código y nombre. */
  buscar(
    parametros: ParametrosConsultaPaginada,
    filtros: FiltrosBodegas = {},
  ): Observable<PagedResult<Bodega>> {
    const params: Record<string, string | number | boolean> = { ...aHttpParams(parametros) };
    if (filtros.sucursalId !== undefined) {
      params['sucursalId'] = filtros.sucursalId;
    }
    if (filtros.soloActivas) {
      params['soloActivas'] = true;
    }
    return this.http.get<PagedResult<Bodega>>(this.baseUrl, { params });
  }

  obtener(publicId: string): Observable<Bodega> {
    return this.http.get<Bodega>(`${this.baseUrl}/${publicId}`);
  }

  crear(datos: BodegaCrear): Observable<Bodega> {
    return this.http.post<Bodega>(this.baseUrl, datos);
  }

  actualizar(publicId: string, datos: BodegaFormulario): Observable<Bodega> {
    return this.http.put<Bodega>(`${this.baseUrl}/${publicId}`, datos);
  }

  /** La anterior principal de la sucursal deja de serlo. */
  marcarPrincipal(publicId: string): Observable<Bodega> {
    return this.http.post<Bodega>(`${this.baseUrl}/${publicId}/principal`, {});
  }

  /** 422 si es la principal o si tiene stock. */
  desactivar(publicId: string): Observable<Bodega> {
    return this.http.post<Bodega>(`${this.baseUrl}/${publicId}/desactivar`, {});
  }

  activar(publicId: string): Observable<Bodega> {
    return this.http.post<Bodega>(`${this.baseUrl}/${publicId}/activar`, {});
  }

  /** 422 si es la principal o si tiene stock. */
  eliminar(publicId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${publicId}`);
  }
}
