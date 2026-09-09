import { Component, input } from '@angular/core';

import { RecetaCristales } from '../../models/receta-cristales.model';

/**
 * Vista de solo lectura de una receta de cristales: graduación de lejos y de cerca por ojo,
 * más DP/ADD y observaciones. Es la misma pieza en los dos lugares donde el negocio la mira
 * —el historial de la ficha del cliente y la pestaña Receta de una OT— y por eso vive acá y
 * no duplicada en cada página.
 *
 * No incluye la cabecera (fecha, flags, acciones): esa la pone quien lo usa, porque en la
 * ficha del cliente lleva botones de editar/eliminar y en la OT no.
 */
@Component({
  selector: 'app-receta-graduacion',
  imports: [],
  templateUrl: './receta-graduacion.html',
  styleUrl: './receta-graduacion.scss',
})
export class RecetaGraduacion {
  readonly receta = input.required<RecetaCristales>();
}
