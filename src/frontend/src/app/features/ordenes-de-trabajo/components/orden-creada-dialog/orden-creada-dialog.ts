import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

import { imprimirConClaseBody } from '../../../../shared/utils/impresion.util';
import { OrdenDeTrabajo } from '../../models/orden-de-trabajo.model';
import { TicketOT } from '../ticket-ot/ticket-ot';

export interface OrdenCreadaDialogData {
  orden: OrdenDeTrabajo;
}

/**
 * Cierre del alta de una Orden de Trabajo — equivale a la vista `Finaliza.cshtml` del legacy
 * ("OT N° X, Cliente…" con el ticket a la vista), con la diferencia de que acá el ticket se ve
 * antes de mandarlo a la impresora.
 *
 * La lógica de impresión (marcar el `<body>` mientras dura, limpiar al terminar) vive en
 * `shared/utils/impresion.util.ts` — la comparte con `ImprimirTicketDialog`, que reimprime el
 * ticket de una OT ya existente desde su ficha.
 */
@Component({
  selector: 'app-orden-creada-dialog',
  imports: [MatButtonModule, MatDialogModule, MatIconModule, TicketOT],
  templateUrl: './orden-creada-dialog.html',
  styleUrl: './orden-creada-dialog.scss',
})
export class OrdenCreadaDialog {
  protected readonly data = inject<OrdenCreadaDialogData>(MAT_DIALOG_DATA);

  protected imprimir(): void {
    imprimirConClaseBody();
  }
}
