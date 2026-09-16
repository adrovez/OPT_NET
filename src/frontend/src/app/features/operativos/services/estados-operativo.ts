import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { EstadoOperativo } from '../models/operativo.model';

/**
 * Catálogo de estados del Operativo (OPT_EstadoOperativo). Sembrado e inmutable durante la
 * sesión: se pide una sola vez y se comparte con `shareReplay` (mismo patrón que
 * `CatalogosComercial` del módulo Orden de Trabajo).
 */
@Injectable({
  providedIn: 'root',
})
export class EstadosOperativo {
  private readonly http = inject(HttpClient);

  private readonly estados$ = this.http
    .get<EstadoOperativo[]>(`${environment.apiUrl}/estados-operativo`)
    .pipe(shareReplay({ bufferSize: 1, refCount: false }));

  listar(): Observable<EstadoOperativo[]> {
    return this.estados$;
  }
}
