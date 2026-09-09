import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import {
  Usuario,
  UsuarioActualizar,
  UsuarioCambiarClave,
  UsuarioCrear,
} from '../models/usuario.model';

/** Encapsula el acceso HTTP al módulo Usuarios — PublicId en las rutas (ADR 0004). */
@Injectable({
  providedIn: 'root',
})
export class Usuarios {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/usuarios`;

  /** Listado paginado del lado del servidor. `busqueda` hace match contra nombre, apellido, RUT y email. */
  buscar(parametros: ParametrosConsultaPaginada): Observable<PagedResult<Usuario>> {
    return this.http.get<PagedResult<Usuario>>(this.baseUrl, { params: aHttpParams(parametros) });
  }

  obtener(publicId: string): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.baseUrl}/${publicId}`);
  }

  /** El usuario creado queda sin sucursal asignada (ver asignarSucursal). */
  crear(datos: UsuarioCrear): Observable<Usuario> {
    return this.http.post<Usuario>(this.baseUrl, datos);
  }

  actualizar(publicId: string, datos: UsuarioActualizar): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.baseUrl}/${publicId}`, datos);
  }

  eliminar(publicId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${publicId}`);
  }

  cambiarClave(publicId: string, datos: UsuarioCambiarClave): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${publicId}/clave`, datos);
  }

  activar(publicId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${publicId}/activar`, {});
  }

  desactivar(publicId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${publicId}/desactivar`, {});
  }

  asignarSucursal(publicId: string, sucursalId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${publicId}/sucursales/${sucursalId}`, {});
  }

  quitarSucursal(publicId: string, sucursalId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${publicId}/sucursales/${sucursalId}`);
  }
}
