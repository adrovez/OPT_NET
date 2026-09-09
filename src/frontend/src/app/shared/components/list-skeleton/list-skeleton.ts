import { Component, computed, input } from '@angular/core';

/**
 * Placeholder de carga para listados/tablas. Reemplaza el `<mat-spinner>` suelto que
 * quedaba flotando arriba a la izquierda sin reservar alto — con el skeleton el layout no
 * "salta" cuando llegan los datos. Ver src/frontend/CLAUDE.md "Estados de listado".
 */
@Component({
  selector: 'app-list-skeleton',
  templateUrl: './list-skeleton.html',
  styleUrl: './list-skeleton.scss',
})
export class ListSkeleton {
  /** Cantidad de filas simuladas. */
  readonly rows = input(6);
  /** Muestra una barra de cabecera (para skeletons de tabla). */
  readonly header = input(true);

  protected readonly filas = computed(() => Array.from({ length: Math.max(1, this.rows()) }));
}
