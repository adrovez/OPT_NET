import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { EstadoCuota, EstadoOT, FormaPago } from '../models/catalogos-comercial.model';

/**
 * Catálogos del módulo Comercial (estados de OT, formas de pago, estados de cuota).
 * Son sembrados e inmutables durante la sesión: se piden una sola vez y se comparten con
 * `shareReplay` — las pantallas de OT, Abonos, Pagos, Cuotas y Cobranza los consumen todas.
 */
@Injectable({
  providedIn: 'root',
})
export class CatalogosComercial {
  private readonly http = inject(HttpClient);

  private readonly estadosOT$ = this.http
    .get<EstadoOT[]>(`${environment.apiUrl}/estados-ot`)
    .pipe(shareReplay({ bufferSize: 1, refCount: false }));

  private readonly formasPago$ = this.http
    .get<FormaPago[]>(`${environment.apiUrl}/formas-pago`)
    .pipe(shareReplay({ bufferSize: 1, refCount: false }));

  private readonly estadosCuota$ = this.http
    .get<EstadoCuota[]>(`${environment.apiUrl}/estados-cuota`)
    .pipe(shareReplay({ bufferSize: 1, refCount: false }));

  listarEstadosOT(): Observable<EstadoOT[]> {
    return this.estadosOT$;
  }

  listarFormasPago(): Observable<FormaPago[]> {
    return this.formasPago$;
  }

  listarEstadosCuota(): Observable<EstadoCuota[]> {
    return this.estadosCuota$;
  }
}
