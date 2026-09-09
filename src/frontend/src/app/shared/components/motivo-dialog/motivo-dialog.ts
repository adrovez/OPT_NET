import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface MotivoDialogData {
  titulo: string;
  mensaje: string;
  etiqueta: string;
  textoConfirmar?: string;
  colorConfirmar?: 'primary' | 'warn';
  maxLength?: number;
}

/**
 * Confirmación que además **exige un texto** (motivo de anulación, observación al retroceder
 * una etapa del flujo de la OT). `ConfirmDialog` no sirve para esto: el backend rechaza con
 * 422 el retroceso sin observación y la anulación sin motivo, así que el campo es requerido
 * acá también — no se envía una llamada que se sabe que va a fallar.
 *
 * Cierra con el texto ingresado, o `undefined` si se cancela.
 */
@Component({
  selector: 'app-motivo-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './motivo-dialog.html',
  styleUrl: './motivo-dialog.scss',
})
export class MotivoDialog {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<MatDialogRef<MotivoDialog, string | undefined>>(MatDialogRef);
  protected readonly data = inject<MotivoDialogData>(MAT_DIALOG_DATA);

  protected readonly maxLength = this.data.maxLength ?? 250;

  protected readonly form = this.fb.nonNullable.group({
    motivo: ['', [Validators.required, Validators.maxLength(this.maxLength)]],
  });

  protected confirmar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue().motivo.trim());
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
