import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import { Empresa, EmpresaFormulario } from '../models/empresa.model';

/** Encapsula el acceso HTTP al módulo Empresas — PublicId en las rutas (ADR 0004). */
@Injectable({
  providedIn: 'root',
})
export class Empresas {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/empresas`;

  /** Listado paginado del lado del servidor. `busqueda` hace match contra nombre, RUT y razón social. */
  buscar(parametros: ParametrosConsultaPaginada): Observable<PagedResult<Empresa>> {
    return this.http.get<PagedResult<Empresa>>(this.baseUrl, { params: aHttpParams(parametros) });
  }

  obtener(publicId: string): Observable<Empresa> {
    return this.http.get<Empresa>(`${this.baseUrl}/${publicId}`);
  }

  crear(datos: EmpresaFormulario): Observable<Empresa> {
    return this.http.post<Empresa>(this.baseUrl, datos);
  }

  actualizar(publicId: string, datos: EmpresaFormulario): Observable<Empresa> {
    return this.http.put<Empresa>(`${this.baseUrl}/${publicId}`, datos);
  }

  eliminar(publicId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${publicId}`);
  }
}
