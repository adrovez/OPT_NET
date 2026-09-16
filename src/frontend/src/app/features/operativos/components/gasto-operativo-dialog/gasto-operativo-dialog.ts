import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { RegistrarGastoOperativo } from '../../models/operativo.model';

/**
 * Registrar un gasto de Operativo (N° de boleta/factura + monto + observación). No llama a
 * `Operativos.registrarGasto`: la ficha decide y se queda con el Operativo recalculado que
 * devuelve el backend, igual que todo comando del agregado.
 */
@Component({
  selector: 'app-gasto-operativo-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './gasto-operativo-dialog.html',
  styleUrl: './gasto-operativo-dialog.scss',
})
export class GastoOperativoDialog {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef =
    inject<MatDialogRef<GastoOperativoDialog, RegistrarGastoOperativo | undefined>>(MatDialogRef);

  protected readonly form = this.fb.nonNullable.group({
    monto: this.fb.control<number | null>(null, [Validators.required, Validators.min(1)]),
    numeroDocumento: ['', [Validators.maxLength(50)]],
    observacion: ['', [Validators.maxLength(500)]],
  });

  protected guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { monto, numeroDocumento, observacion } = this.form.getRawValue();
    this.dialogRef.close({
      monto: monto!,
      numeroDocumento: numeroDocumento || null,
      observacion: observacion || null,
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
