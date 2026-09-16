import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';

import { SelectorOrden } from '../../../ordenes-de-trabajo/components/selector-orden/selector-orden';
import { OrdenDeTrabajoResumen } from '../../../ordenes-de-trabajo/models/orden-de-trabajo.model';

/**
 * Envoltorio de `<app-selector-orden>` (ya usado por Abonos/Pagos/Cuotas) para elegir la OT
 * que se va a asociar a un Operativo. No llama a `Operativos.asociarOrden` acá: solo emite la
 * OT elegida cerrando el diálogo — la ficha del Operativo hace la llamada y refresca, igual
 * que con cualquier otro comando del agregado (ADR 0007).
 */
@Component({
  selector: 'app-asociar-orden-dialog',
  imports: [MatButtonModule, MatDialogModule, SelectorOrden],
  templateUrl: './asociar-orden-dialog.html',
  styleUrl: './asociar-orden-dialog.scss',
})
export class AsociarOrdenDialog {
  private readonly dialogRef =
    inject<MatDialogRef<AsociarOrdenDialog, OrdenDeTrabajoResumen | undefined>>(MatDialogRef);

  protected elegir(orden: OrdenDeTrabajoResumen): void {
    this.dialogRef.close(orden);
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
