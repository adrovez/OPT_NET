import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { Region } from '../models/region.model';

/** Encapsula el acceso HTTP al catálogo de Regiones (solo lectura). */
@Injectable({
  providedIn: 'root',
})
export class Regiones {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/regiones`;

  listar(): Observable<Region[]> {
    return this.http.get<Region[]>(this.baseUrl);
  }
}
