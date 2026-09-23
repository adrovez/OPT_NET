import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import {
  AbonoRegistrar,
  AnulacionOT,
  CambioEstadoOT,
  CuotaPagar,
  FiltrosOrdenesDeTrabajo,
  OrdenDeTrabajo,
  OrdenDeTrabajoActualizar,
  OrdenDeTrabajoCrear,
  OrdenDeTrabajoResumen,
  PagoRegistrar,
  PlanCuotasGenerar,
} from '../models/orden-de-trabajo.model';

/**
 * Acceso HTTP al agregado Orden de Trabajo. Todas las rutas van por `publicId` (ADR 0004);
 * abonos, pagos, cuotas y el cambio de estado son **subrecursos** anidados bajo la OT, tal
 * como los expone el backend — no existe `/api/pagos` ni `/api/cuotas` sueltos.
 *
 * Cada comando devuelve la OT completa ya recalculada (precio, totalAbonado, saldo, estado):
 * el llamador nunca necesita releer después de actuar.
 */
@Injectable({
  providedIn: 'root',
})
export class OrdenesDeTrabajo {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/ordenes-de-trabajo`;

  /**
   * Listado paginado. `busqueda` hace match contra el número de OT, el beneficiario y el
   * RUT/nombre del cliente; los filtros son acumulativos y todos opcionales.
   */
  buscar(
    parametros: ParametrosConsultaPaginada,
    filtros: FiltrosOrdenesDeTrabajo = {},
  ): Observable<PagedResult<OrdenDeTrabajoResumen>> {
    return this.http.get<PagedResult<OrdenDeTrabajoResumen>>(this.baseUrl, {
      params: { ...aHttpParams(parametros), ...this.aParametrosFiltro(filtros) },
    });
  }

  /** Vista completa: cabecera + detalle + abonos + pagos + cuotas + bitácora. */
  obtener(publicId: string): Observable<OrdenDeTrabajo> {
    return this.http.get<OrdenDeTrabajo>(`${this.baseUrl}/${publicId}`);
  }

  crear(datos: OrdenDeTrabajoCrear): Observable<OrdenDeTrabajo> {
    return this.http.post<OrdenDeTrabajo>(this.baseUrl, datos);
  }

  actualizar(publicId: string, datos: OrdenDeTrabajoActualizar): Observable<OrdenDeTrabajo> {
    return this.http.put<OrdenDeTrabajo>(`${this.baseUrl}/${publicId}`, datos);
  }

  // ── Flujo de estados ───────────────────────────────────────────────────────

  /** Avanza o retrocede UNA etapa. Saltar etapas o mover una OT terminal devuelve 422. */
  cambiarEstado(publicId: string, datos: CambioEstadoOT): Observable<OrdenDeTrabajo> {
    return this.http.post<OrdenDeTrabajo>(`${this.baseUrl}/${publicId}/estado`, datos);
  }

  /** Anula la OT (estado terminal ANULADO). No la borra: queda en el listado con su historial. */
  anular(publicId: string, datos: AnulacionOT): Observable<OrdenDeTrabajo> {
    return this.http.post<OrdenDeTrabajo>(`${this.baseUrl}/${publicId}/anular`, datos);
  }

  // ── Dinero ─────────────────────────────────────────────────────────────────

  registrarAbono(publicId: string, datos: AbonoRegistrar): Observable<OrdenDeTrabajo> {
    return this.http.post<OrdenDeTrabajo>(`${this.baseUrl}/${publicId}/abonos`, datos);
  }

  registrarPago(publicId: string, datos: PagoRegistrar): Observable<OrdenDeTrabajo> {
    return this.http.post<OrdenDeTrabajo>(`${this.baseUrl}/${publicId}/pagos`, datos);
  }

  // ── Plan de cuotas ─────────────────────────────────────────────────────────

  generarPlanCuotas(publicId: string, datos: PlanCuotasGenerar): Observable<OrdenDeTrabajo> {
    return this.http.post<OrdenDeTrabajo>(`${this.baseUrl}/${publicId}/cuotas`, datos);
  }

  pagarCuota(publicId: string, numero: number, datos: CuotaPagar): Observable<OrdenDeTrabajo> {
    return this.http.post<OrdenDeTrabajo>(
      `${this.baseUrl}/${publicId}/cuotas/${numero}/pagar`,
      datos,
    );
  }

  anularCuota(publicId: string, numero: number): Observable<OrdenDeTrabajo> {
    return this.http.post<OrdenDeTrabajo>(
      `${this.baseUrl}/${publicId}/cuotas/${numero}/anular`,
      {},
    );
  }

  /** Omite los filtros sin valor para que la URL quede limpia y el backend use sus defaults. */
  private aParametrosFiltro(filtros: FiltrosOrdenesDeTrabajo): Record<string, string | number> {
    const params: Record<string, string | number> = {};

    if (filtros.clientePublicId) {
      params['clientePublicId'] = filtros.clientePublicId;
    }
    if (filtros.empresaPublicId) {
      params['empresaPublicId'] = filtros.empresaPublicId;
    }
    if (filtros.operativoPublicId) {
      params['operativoPublicId'] = filtros.operativoPublicId;
    }
    if (filtros.sucursalId) {
      params['sucursalId'] = filtros.sucursalId;
    }
    if (filtros.estadoOTId !== null && filtros.estadoOTId !== undefined) {
      params['estadoOTId'] = filtros.estadoOTId;
    }
    if (filtros.soloConSaldo) {
      params['soloConSaldo'] = 'true';
    }
    if (filtros.soloSucursal) {
      params['soloSucursal'] = 'true';
    }

    return params;
  }
}
