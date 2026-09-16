import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

import { imprimirConClaseBody } from '../../../../shared/utils/impresion.util';
import { OrdenDeTrabajo } from '../../models/orden-de-trabajo.model';
import { TicketOT } from '../ticket-ot/ticket-ot';

export interface ImprimirTicketDialogData {
  orden: OrdenDeTrabajo;
}

/**
 * Reimpresión del ticket de una OT ya existente — el legacy lo permitía desde
 * `Imprimir/Ticket/TicketOT/{id}` en cualquier momento. Acá es la acción "Imprimir ticket" de
 * la ficha de la orden (`OrdenDeTrabajoFicha`), disponible aunque la OT esté anulada o
 * entregada: reimprimir no modifica nada, así que no se bloquea con el resto de las acciones.
 *
 * Es la versión sin los botones "Nueva OT" / "Ver orden" de `OrdenCreadaDialog` (acá no se
 * acaba de crear nada) — comparten el mismo `<app-ticket-ot>` y la misma utilidad de impresión
 * (`shared/utils/impresion.util.ts`).
 */
@Component({
  selector: 'app-imprimir-ticket-dialog',
  imports: [MatButtonModule, MatDialogModule, MatIconModule, TicketOT],
  templateUrl: './imprimir-ticket-dialog.html',
  styleUrl: './imprimir-ticket-dialog.scss',
})
export class ImprimirTicketDialog {
  protected readonly data = inject<ImprimirTicketDialogData>(MAT_DIALOG_DATA);

  protected imprimir(): void {
    imprimirConClaseBody();
  }
}
