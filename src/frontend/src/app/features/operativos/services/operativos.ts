import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { aHttpParams } from '../../../core/utils/paginacion.util';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ParametrosConsultaPaginada } from '../../../shared/models/parametros-consulta-paginada.model';
import {
  AnulacionOperativo,
  AsociarOrdenOperativo,
  CambioEstadoOperativo,
  FiltrosOperativos,
  Operativo,
  OperativoActualizar,
  OperativoCrear,
  OperativoResumen,
  RegistrarGastoOperativo,
} from '../models/operativo.model';

/**
 * Acceso HTTP al agregado Operativo. Órdenes asociadas y gastos son **subrecursos** anidados
 * bajo el Operativo, tal como los expone el backend — no existe `/api/gastos-operativo` suelto.
 *
 * Cada comando devuelve el Operativo completo ya recalculado (montos, órdenes, gastos): el
 * llamador nunca necesita releer después de actuar (mismo patrón que OrdenDeTrabajo, ADR 0007).
 */
@Injectable({
  providedIn: 'root',
})
export class Operativos {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/operativos`;

  /** Listado paginado. `busqueda` hace match contra el correlativo y la observación. */
  buscar(
    parametros: ParametrosConsultaPaginada,
    filtros: FiltrosOperativos = {},
  ): Observable<PagedResult<OperativoResumen>> {
    return this.http.get<PagedResult<OperativoResumen>>(this.baseUrl, {
      params: { ...aHttpParams(parametros), ...this.aParametrosFiltro(filtros) },
    });
  }

  /** Vista completa: cabecera + órdenes asociadas + gastos. */
  obtener(publicId: string): Observable<Operativo> {
    return this.http.get<Operativo>(`${this.baseUrl}/${publicId}`);
  }

  crear(datos: OperativoCrear): Observable<Operativo> {
    return this.http.post<Operativo>(this.baseUrl, datos);
  }

  actualizar(publicId: string, datos: OperativoActualizar): Observable<Operativo> {
    return this.http.put<Operativo>(`${this.baseUrl}/${publicId}`, datos);
  }

  // ── Flujo de estados ───────────────────────────────────────────────────────

  /** Avanza UNA etapa (Prospecto→Ingresado→Cobranza→Cerrado). No hay retroceso ni salto. */
  cambiarEstado(publicId: string, datos: CambioEstadoOperativo): Observable<Operativo> {
    return this.http.post<Operativo>(`${this.baseUrl}/${publicId}/estado`, datos);
  }

  /** Solo posible desde Prospecto o Ingresado — un Operativo en Cobranza ya no se anula. */
  anular(publicId: string, datos: AnulacionOperativo): Observable<Operativo> {
    return this.http.post<Operativo>(`${this.baseUrl}/${publicId}/anular`, datos);
  }

  // ── Órdenes asociadas ────────────────────────────────────────────────────────

  asociarOrden(publicId: string, datos: AsociarOrdenOperativo): Observable<Operativo> {
    return this.http.post<Operativo>(`${this.baseUrl}/${publicId}/ordenes`, datos);
  }

  quitarOrden(publicId: string, ordenPublicId: string): Observable<Operativo> {
    return this.http.delete<Operativo>(`${this.baseUrl}/${publicId}/ordenes/${ordenPublicId}`);
  }

  /** Refresca montoTotalVendido/Pagado desde el Precio/TotalAbonado real de cada OT asociada. */
  recalcularMontos(publicId: string): Observable<Operativo> {
    return this.http.post<Operativo>(`${this.baseUrl}/${publicId}/recalcular-montos`, {});
  }

  // ── Gastos ─────────────────────────────────────────────────────────────────

  registrarGasto(publicId: string, datos: RegistrarGastoOperativo): Observable<Operativo> {
    return this.http.post<Operativo>(`${this.baseUrl}/${publicId}/gastos`, datos);
  }

  eliminarGasto(publicId: string, gastoId: number): Observable<Operativo> {
    return this.http.delete<Operativo>(`${this.baseUrl}/${publicId}/gastos/${gastoId}`);
  }

  /** Omite los filtros sin valor para que la URL quede limpia y el backend use sus defaults. */
  private aParametrosFiltro(filtros: FiltrosOperativos): Record<string, string | number> {
    const params: Record<string, string | number> = {};

    if (filtros.empresaPublicId) {
      params['empresaPublicId'] = filtros.empresaPublicId;
    }
    if (filtros.sucursalId) {
      params['sucursalId'] = filtros.sucursalId;
    }
    if (filtros.estadoOperativoId !== null && filtros.estadoOperativoId !== undefined) {
      params['estadoOperativoId'] = filtros.estadoOperativoId;
    }

    return params;
  }
}
