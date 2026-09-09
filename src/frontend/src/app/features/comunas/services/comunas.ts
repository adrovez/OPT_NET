import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { Comuna } from '../models/comuna.model';

/** Encapsula el acceso HTTP al catálogo de Comunas (solo lectura, filtrado por región). */
@Injectable({
  providedIn: 'root',
})
export class Comunas {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/comunas`;

  listarPorRegion(regionId: number): Observable<Comuna[]> {
    return this.http.get<Comuna[]>(this.baseUrl, { params: { regionId } });
  }
}
