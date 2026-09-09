import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { DeudorEmpresa } from '../models/deudor-empresa.model';

/**
 * Cobranza — vista consolidada de la deuda. Reemplaza al `sp_ListaDeudores` del legacy.
 * El detalle de cada deudor no tiene endpoint propio: es el listado de OT filtrado por
 * empresa y saldo, así que esta pantalla navega a `/ordenes-de-trabajo` con query params.
 */
@Injectable({
  providedIn: 'root',
})
export class Cobranza {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/cobranza`;

  /** Lista completa (una fila por empresa con deuda), ya ordenada de mayor a menor saldo. */
  listarDeudores(): Observable<DeudorEmpresa[]> {
    return this.http.get<DeudorEmpresa[]>(`${this.baseUrl}/deudores`);
  }
}
