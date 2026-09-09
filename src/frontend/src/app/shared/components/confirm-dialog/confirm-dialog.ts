import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmDialogData {
  titulo: string;
  mensaje: string;
  textoConfirmar?: string;
  colorConfirmar?: 'primary' | 'warn';
}

/** Diálogo de confirmación genérico — reutilizado por Sucursales/Empresas/Usuarios. */
@Component({
  selector: 'app-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.scss',
})
export class ConfirmDialog {
  private readonly dialogRef = inject<MatDialogRef<ConfirmDialog, boolean>>(MatDialogRef);
  protected readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);

  protected confirmar(): void {
    this.dialogRef.close(true);
  }

  protected cancelar(): void {
    this.dialogRef.close(false);
  }
}
