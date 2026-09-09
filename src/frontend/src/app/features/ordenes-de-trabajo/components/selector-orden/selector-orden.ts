import { Component, inject, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';

import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { SearchBox } from '../../../../shared/components/search-box/search-box';
import { crearMatPaginatorIntlEs } from '../../../../shared/i18n/mat-paginator-intl-es';
import { OPCIONES_TAMANIO_PAGINA } from '../../../../shared/models/parametros-consulta-paginada.model';
import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { EstadoListaPaginada } from '../../../../shared/utils/estado-lista-paginada';
import { OrdenDeTrabajoResumen } from '../../models/orden-de-trabajo.model';
import { OrdenesDeTrabajo } from '../../services/ordenes-de-trabajo';
import { EstadoOtChip } from '../estado-ot-chip/estado-ot-chip';

/**
 * Buscador de OT reutilizable: es el primer paso de las pantallas de Abonos, Pagos y Cuotas,
 * que en el legacy se abrían con el N° de OT en la URL. Acá el operador la busca por número,
 * RUT o nombre y la elige de la lista — mismo listado paginado del backend, sin traer todo.
 *
 * No modifica nada: solo emite la OT elegida (`seleccionar`); quien lo usa decide qué hacer.
 */
@Component({
  selector: 'app-selector-orden',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    EmptyState,
    EstadoOtChip,
    ListSkeleton,
    PesosPipe,
    SearchBox,
  ],
  providers: [{ provide: MatPaginatorIntl, useFactory: crearMatPaginatorIntlEs }],
  templateUrl: './selector-orden.html',
  styleUrl: './selector-orden.scss',
})
export class SelectorOrden {
  /** Acota la búsqueda a las OT con saldo pendiente (cobranza: Pagos y Cuotas). */
  readonly soloConSaldo = input(false);
  readonly etiqueta = input('Buscar por N° de OT, RUT o nombre del cliente');

  readonly seleccionar = output<OrdenDeTrabajoResumen>();

  private readonly ordenesService = inject(OrdenesDeTrabajo);

  protected readonly columnas = ['numeroOT', 'cliente', 'estado', 'precio', 'saldo'];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly estado = new EstadoListaPaginada<OrdenDeTrabajoResumen>((p) =>
    this.ordenesService.buscar(p, { soloConSaldo: this.soloConSaldo() }),
  );

  constructor() {
    this.estado.cargar();
  }
}
