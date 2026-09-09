import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

import { OrdenDeTrabajo } from '../../models/orden-de-trabajo.model';
import { TicketOT } from '../ticket-ot/ticket-ot';

export interface OrdenCreadaDialogData {
  orden: OrdenDeTrabajo;
}

/** Qué quiere hacer el operador después de crear la orden. */
export type OrdenCreadaAccion = 'nueva' | 'ver';

/**
 * Cierre del alta de una Orden de Trabajo — equivale a la vista `Finaliza.cshtml` del legacy
 * ("OT N° X, Cliente…" con los botones Nueva OT e Imprimir), con la diferencia de que acá el
 * ticket se ve antes de mandarlo a la impresora.
 *
 * La impresión marca el `<body>` mientras dura: las reglas de `@media print` de `styles.scss`
 * ocultan la aplicación y dejan solo este diálogo en el papel.
 */
@Component({
  selector: 'app-orden-creada-dialog',
  imports: [MatButtonModule, MatDialogModule, MatIconModule, TicketOT],
  templateUrl: './orden-creada-dialog.html',
  styleUrl: './orden-creada-dialog.scss',
})
export class OrdenCreadaDialog {
  private readonly dialogRef =
    inject<MatDialogRef<OrdenCreadaDialog, OrdenCreadaAccion | undefined>>(MatDialogRef);
  protected readonly data = inject<OrdenCreadaDialogData>(MAT_DIALOG_DATA);

  protected imprimir(): void {
    const body = document.body;
    body.classList.add('opt-imprimiendo');

    const limpiar = () => {
      body.classList.remove('opt-imprimiendo');
      window.removeEventListener('afterprint', limpiar);
    };
    // `afterprint` no dispara en todos los navegadores si el usuario cancela desde el diálogo
    // nativo; el `finally` del print síncrono es la red de seguridad.
    window.addEventListener('afterprint', limpiar);

    try {
      window.print();
    } finally {
      limpiar();
    }
  }

  protected cerrarCon(accion: OrdenCreadaAccion): void {
    this.dialogRef.close(accion);
  }
}
