import { DatePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { OrdenDeTrabajo } from '../../models/orden-de-trabajo.model';

/**
 * Datos fijos del encabezado impreso y del texto de autorización del ticket — el legacy
 * (`_ParcialTicketOT.cshtml`) los tenía escritos a fuego en el HTML, igual que acá. No viven en
 * el DTO de la OT porque hoy son los mismos para toda la empresa; quedan reunidos en un solo
 * lugar porque se sabe que van a cambiar pronto (nombre y RUT de la razón social — pendiente de
 * definir con Alexis, conversación 2026-09-09) y este es el único punto que hay que tocar.
 */
const EMPRESA = {
  nombre: 'Centro Óptico BL',
  rut: '76.352.228-8',
  fono: '+56 9 3408 2615',
  direccion: 'Agustinas 853 Of. 507',
};

/** Cuántas copias se imprimen — el legacy generaba 3 para que el cliente firme el compromiso de pago. */
const NUMERO_DE_COPIAS = 3;

/**
 * Comprobante imprimible de una Orden de Trabajo — reemplaza al `rptTicketOT.rdlc` /
 * `_ParcialTicketOT.cshtml` del legacy (`Areas/Imprimir`), replicando su formato: encabezado de
 * la empresa, "Nota de venta y autorización de descuento", datos de la OT, detalle, totales,
 * receta (RX Lejos/Cerca) y el texto de autorización con las líneas de firma.
 *
 * En pantalla se ve una sola copia (para revisar antes de imprimir); al imprimir se repite
 * `NUMERO_DE_COPIAS` veces con salto de página entre cada una — igual que el legacy, que lo
 * lograba clonando el DOM por JavaScript al momento de imprimir. Acá es directamente el `@for`
 * del template, sin JS de por medio: las reglas de "una sola visible en pantalla, todas en el
 * papel" viven en `ticket-ot.scss`.
 *
 * Es solo presentación: quien lo muestra decide cuándo llamar a `window.print()`
 * (`shared/utils/impresion.util.ts`) y las reglas de `@media print` que ocultan el resto de la
 * aplicación viven en `styles.scss` global (un `@media print` encapsulado por componente no
 * puede ocultar el resto de la app).
 */
@Component({
  selector: 'app-ticket-ot',
  imports: [DatePipe, PesosPipe],
  templateUrl: './ticket-ot.html',
  styleUrl: './ticket-ot.scss',
})
export class TicketOT {
  readonly orden = input.required<OrdenDeTrabajo>();

  protected readonly empresa = EMPRESA;
  protected readonly textoAutorizacion =
    `Solicito a ${EMPRESA.nombre} proceder a la elaboración del despacho de mi receta, ya que ` +
    'estoy de acuerdo con el valor y las facilidades de pago aquí descritas. Del mismo modo, ' +
    'autorizo a mi empleador para que realice los descuentos acordados en forma mensual de mi ' +
    'liquidación de sueldo.';

  /** Una entrada por copia a imprimir — ver NUMERO_DE_COPIAS. */
  protected readonly copias = Array.from({ length: NUMERO_DE_COPIAS }, (_, i) => i + 1);

  /** La receta con la que se despachan estos cristales (legacy: `OPT_RecetaCristales.FirstOrDefault()`). */
  protected readonly receta = computed(() => this.orden().recetas[0] ?? null);

  /** Cuotas vigentes: las anuladas no se imprimen porque el cliente no las debe. */
  protected readonly cuotas = computed(() =>
    this.orden().cuotas.filter((cuota) => cuota.estadoCuota !== 'ANULADA'),
  );

  /** El legacy imprimía `Saldo / NumeroCuota`; acá el valor real ya viene calculado por cuota. */
  protected readonly valorCuota = computed(() => this.cuotas()[0]?.valorCuota ?? null);
}
