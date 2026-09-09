import { Component, computed, input } from '@angular/core';

import { ESTADOS_OT } from '../../models/catalogos-comercial.model';

/** Clase CSS por Id de OPT_EstadoOT — los colores viven en theme-tokens.scss, no acá. */
const CLASE_POR_ESTADO: Readonly<Record<number, string>> = {
  [ESTADOS_OT.ingresado]: 'ingresado',
  [ESTADOS_OT.enProceso]: 'en-proceso',
  [ESTADOS_OT.montaje]: 'montaje',
  [ESTADOS_OT.laboratorio]: 'laboratorio',
  [ESTADOS_OT.calidad]: 'calidad',
  [ESTADOS_OT.despacho]: 'despacho',
  [ESTADOS_OT.entregado]: 'entregado',
  [ESTADOS_OT.anulado]: 'anulado',
};

/**
 * Chip del estado de una OT. Usa los tokens `--opt-estado-ot-*` de `theme-tokens.scss`
 * (fondo + texto explícitos, "isla de color": se ve igual en tema claro y oscuro porque
 * el estado es semántico, no decorativo — ver branding-ux-ui.md).
 *
 * El nombre se recibe del backend (`estadoOT`), no se traduce ni se reinterpreta acá.
 */
@Component({
  selector: 'app-estado-ot-chip',
  imports: [],
  templateUrl: './estado-ot-chip.html',
  styleUrl: './estado-ot-chip.scss',
})
export class EstadoOtChip {
  readonly estadoId = input.required<number>();
  readonly nombre = input.required<string>();

  protected readonly clase = computed(() => CLASE_POR_ESTADO[this.estadoId()] ?? 'ingresado');
}
