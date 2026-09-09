import { DatePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { RecetaGraduacion } from '../../../receta-cristales/components/receta-graduacion/receta-graduacion';
import { OrdenDeTrabajo } from '../../models/orden-de-trabajo.model';

/**
 * Comprobante imprimible de una Orden de Trabajo — reemplaza al `rptTicketOT.rdlc` del legacy
 * (`Areas/Imprimir/Controllers/TicketController.cs`), que renderizaba el mismo contenido en el
 * servidor: N° de OT, cliente, fechas de atención y entrega, detalle, receta y el resumen de
 * dinero (total, abonado, saldo y el plan de cuotas).
 *
 * Es solo presentación: quien lo muestra decide cuándo llamar a `window.print()` y las reglas
 * de `@media print` viven en `styles.scss` (un `@media print` encapsulado por componente no
 * puede ocultar el resto de la aplicación).
 */
@Component({
  selector: 'app-ticket-ot',
  imports: [DatePipe, PesosPipe, RecetaGraduacion],
  templateUrl: './ticket-ot.html',
  styleUrl: './ticket-ot.scss',
})
export class TicketOT {
  readonly orden = input.required<OrdenDeTrabajo>();

  /** Cuotas vigentes: las anuladas no se imprimen porque el cliente no las debe. */
  protected readonly cuotas = computed(() =>
    this.orden().cuotas.filter((cuota) => cuota.estadoCuota !== 'ANULADA'),
  );

  /** El legacy imprimía `Saldo / NumeroCuota`; acá el valor real ya viene calculado por cuota. */
  protected readonly valorCuota = computed(() => this.cuotas()[0]?.valorCuota ?? null);
}
