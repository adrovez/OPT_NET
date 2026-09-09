import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import { Cliente, ClienteActualizar, ClienteCrear } from '../models/cliente.model';

/**
 * Encapsula el acceso HTTP al módulo Clientes — los componentes nunca llaman a la API
 * directamente (CLAUDE.md, "Frontend"). Rutas expresadas por `publicId` (ADR 0004).
 */
@Injectable({
  providedIn: 'root',
})
export class Clientes {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/clientes`;

  /**
   * Búsqueda paginada del lado del servidor — Cliente tiene ~12.000 filas, no expone un
   * listado completo. `busqueda` hace match contra RUT, nombre y apellido.
   */
  buscar(parametros: ParametrosConsultaPaginada): Observable<PagedResult<Cliente>> {
    return this.http.get<PagedResult<Cliente>>(this.baseUrl, { params: aHttpParams(parametros) });
  }

  obtener(publicId: string): Observable<Cliente> {
    return this.http.get<Cliente>(`${this.baseUrl}/${publicId}`);
  }

  crear(datos: ClienteCrear): Observable<Cliente> {
    return this.http.post<Cliente>(this.baseUrl, datos);
  }

  actualizar(publicId: string, datos: ClienteActualizar): Observable<Cliente> {
    return this.http.put<Cliente>(`${this.baseUrl}/${publicId}`, datos);
  }

  eliminar(publicId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${publicId}`);
  }
}
