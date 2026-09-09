import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { FormaPago } from '../../../ordenes-de-trabajo/models/catalogos-comercial.model';
import { CuotaPagar } from '../../../ordenes-de-trabajo/models/orden-de-trabajo.model';

export interface CuotaPagoDialogData {
  numero: number;
  valorCuota: number;
  formasPago: FormaPago[];
}

/**
 * Marca una cuota como pagada **manualmente** (regularización de un cobro hecho fuera del
 * sistema). Es la excepción, no el flujo normal: registrar el pago en el módulo Pagos imputa
 * las cuotas solo y además mueve el saldo de la OT — este comando no lo hace, por eso el
 * diálogo lo advierte en pantalla.
 *
 * Cierra con los datos del pago, o `undefined` si se cancela.
 */
@Component({
  selector: 'app-cuota-pago-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  providers: [provideNativeDateAdapter(), { provide: MAT_DATE_LOCALE, useValue: 'es-CL' }],
  templateUrl: './cuota-pago-dialog.html',
  styleUrl: './cuota-pago-dialog.scss',
})
export class CuotaPagoDialog {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef =
    inject<MatDialogRef<CuotaPagoDialog, CuotaPagar | undefined>>(MatDialogRef);
  protected readonly data = inject<CuotaPagoDialogData>(MAT_DIALOG_DATA);

  protected readonly form = this.fb.nonNullable.group({
    formaPagoId: this.fb.control<number | null>(null),
    fechaPago: this.fb.control<Date | null>(new Date()),
  });

  protected confirmar(): void {
    const { formaPagoId, fechaPago } = this.form.getRawValue();
    this.dialogRef.close({
      formaPagoId,
      fechaPago: fechaPago ? fechaPago.toISOString() : null,
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
