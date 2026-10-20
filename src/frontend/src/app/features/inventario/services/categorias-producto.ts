import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { CategoriaProducto } from '../models/producto.model';

/** Catálogo de categorías de producto (solo lectura). */
@Injectable({
  providedIn: 'root',
})
export class CategoriasProducto {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/categorias-producto`;

  listar(): Observable<CategoriaProducto[]> {
    return this.http.get<CategoriaProducto[]>(this.baseUrl);
  }
}
