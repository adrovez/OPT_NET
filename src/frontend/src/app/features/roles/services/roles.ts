import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { Rol } from '../models/rol.model';

/** Catálogo de roles — solo lectura, sin mutadores (ver RolesController en el backend). */
@Injectable({
  providedIn: 'root',
})
export class Roles {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/roles`;

  listar(): Observable<Rol[]> {
    return this.http.get<Rol[]>(this.baseUrl);
  }
}
