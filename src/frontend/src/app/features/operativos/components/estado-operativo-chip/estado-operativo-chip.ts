import { Component, computed, input } from '@angular/core';

import { ESTADOS_OPERATIVO } from '../../models/operativo.model';

/**
 * Tono por Id de OPT_EstadoOperativo, reutilizando las clases globales `.opt-chip--*` de
 * `styles.scss` (fondo + texto de los roles M3 ya existentes) en vez de crear tokens de color
 * nuevos — el branding doc exige agregar cualquier color no cubierto ahí antes de usarlo, y
 * este catálogo no lo necesita: 3 tonos alcanzan para distinguir "en curso" / "cerrado" /
 * "anulado", y el nombre (texto) siempre viaja junto al color.
 */
const TONO_POR_ESTADO: Readonly<Record<number, 'info' | 'si' | 'alerta'>> = {
  [ESTADOS_OPERATIVO.prospecto]: 'info',
  [ESTADOS_OPERATIVO.ingresado]: 'info',
  [ESTADOS_OPERATIVO.cobranza]: 'info',
  [ESTADOS_OPERATIVO.cerrado]: 'si',
  [ESTADOS_OPERATIVO.anulado]: 'alerta',
};

/** Chip del estado de un Operativo. El nombre lo manda el backend, no se traduce acá. */
@Component({
  selector: 'app-estado-operativo-chip',
  imports: [],
  templateUrl: './estado-operativo-chip.html',
  styleUrl: './estado-operativo-chip.scss',
})
export class EstadoOperativoChip {
  readonly estadoId = input.required<number>();
  readonly nombre = input.required<string>();

  protected readonly tono = computed(() => TONO_POR_ESTADO[this.estadoId()] ?? 'info');
}
