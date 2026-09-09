import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
// Alias porque el nombre del modelo colisiona con el de esta clase de servicio —
// mismo problema (y misma solución) que OPT.Application.Features.Anamnesis en el backend.
import { Anamnesis as AnamnesisModel, AnamnesisFormulario } from '../models/anamnesis.model';

/**
 * Encapsula el acceso HTTP al módulo Anamnesis — los componentes nunca llaman a la API
 * directamente (CLAUDE.md, "Frontend"). Rutas expresadas por `publicId` (ADR 0004).
 */
@Injectable({
  providedIn: 'root',
})
export class Anamnesis {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/anamnesis`;

  /** Historial de fichas de anamnesis de un cliente (más reciente primero). */
  obtenerPorCliente(clientePublicId: string): Observable<AnamnesisModel[]> {
    return this.http.get<AnamnesisModel[]>(this.baseUrl, { params: { clientePublicId } });
  }

  crear(clientePublicId: string, datos: AnamnesisFormulario): Observable<AnamnesisModel> {
    return this.http.post<AnamnesisModel>(this.baseUrl, { clientePublicId, ...datos });
  }

  // Regla de negocio: la anamnesis es inmutable una vez creada — no existe endpoint de actualización.

  eliminar(publicId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${publicId}`);
  }
}
