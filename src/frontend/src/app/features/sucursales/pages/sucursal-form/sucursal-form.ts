import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Sucursal } from '../../models/sucursal.model';
import { Sucursales } from '../../services/sucursales';

export interface SucursalFormDialogData {
  sucursal?: Sucursal;
}

/** Diálogo de alta/edición de Sucursal — EsMatriz solo es editable al crear. */
@Component({
  selector: 'app-sucursal-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './sucursal-form.html',
  styleUrl: './sucursal-form.scss',
})
export class SucursalForm {
  private readonly fb = inject(FormBuilder);
  private readonly sucursalesService = inject(Sucursales);
  private readonly dialogRef =
    inject<MatDialogRef<SucursalForm, Sucursal | undefined>>(MatDialogRef);
  protected readonly data = inject<SucursalFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.sucursal;
  protected readonly guardando = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    nombre: [this.data.sucursal?.nombre ?? '', [Validators.required, Validators.maxLength(100)]],
    direccion: [this.data.sucursal?.direccion ?? '', [Validators.maxLength(200)]],
    telefono: [this.data.sucursal?.telefono ?? '', [Validators.maxLength(20)]],
    esMatriz: [this.data.sucursal?.esMatriz ?? false],
  });

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const { nombre, direccion, telefono, esMatriz } = this.form.getRawValue();

    const peticion = this.esEdicion
      ? this.sucursalesService.actualizar(this.data.sucursal!.id, { nombre, direccion, telefono })
      : this.sucursalesService.crear({ nombre, direccion, telefono, esMatriz });

    peticion.subscribe({
      next: (sucursal) => this.dialogRef.close(sucursal),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
