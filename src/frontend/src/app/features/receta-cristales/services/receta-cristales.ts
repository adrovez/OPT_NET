import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
// Alias porque el nombre del modelo colisiona con el de esta clase de servicio —
// mismo problema (y misma solución) que OPT.Application.Features.RecetaCristales en el backend.
import {
  RecetaCristales as RecetaCristalesModel,
  RecetaCristalesFormulario,
} from '../models/receta-cristales.model';

/**
 * Encapsula el acceso HTTP al módulo RecetaCristales — los componentes nunca llaman a la API
 * directamente (CLAUDE.md, "Frontend"). Rutas expresadas por `publicId` (ADR 0004).
 */
@Injectable({
  providedIn: 'root',
})
export class RecetaCristales {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/recetacristales`;

  /** Historial de recetas de un cliente (más reciente primero). */
  obtenerPorCliente(clientePublicId: string): Observable<RecetaCristalesModel[]> {
    return this.http.get<RecetaCristalesModel[]>(this.baseUrl, { params: { clientePublicId } });
  }

  crear(
    clientePublicId: string,
    datos: RecetaCristalesFormulario,
  ): Observable<RecetaCristalesModel> {
    return this.http.post<RecetaCristalesModel>(this.baseUrl, { clientePublicId, ...datos });
  }

  actualizar(publicId: string, datos: RecetaCristalesFormulario): Observable<RecetaCristalesModel> {
    return this.http.put<RecetaCristalesModel>(`${this.baseUrl}/${publicId}`, datos);
  }

  eliminar(publicId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${publicId}`);
  }
}
