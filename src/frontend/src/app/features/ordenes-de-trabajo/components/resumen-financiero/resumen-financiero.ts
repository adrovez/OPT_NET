import { Component, input } from '@angular/core';

import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';

/**
 * Tríptico Total / Abonado / Saldo de una OT — el mismo bloque que el legacy repetía en las
 * pantallas de Ingreso, Pago y Deuda. Las tres cifras las calcula el backend dentro de la
 * transacción del movimiento (ADR 0003/0006): acá nunca se recalculan ni se suman a mano.
 *
 * El saldo negativo es válido: el negocio acepta sobrepago (decisión 2026-08-27).
 */
@Component({
  selector: 'app-resumen-financiero',
  imports: [PesosPipe],
  templateUrl: './resumen-financiero.html',
  styleUrl: './resumen-financiero.scss',
})
export class ResumenFinanciero {
  readonly precio = input.required<number>();
  readonly totalAbonado = input.required<number>();
  readonly saldo = input.required<number>();
}
