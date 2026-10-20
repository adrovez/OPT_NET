import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Bodega } from '../../models/bodega.model';
import { Bodegas } from '../../services/bodegas';

export interface BodegaFormDialogData {
  /** Sucursal dueña — obligatoria al crear; al editar se toma de la bodega. */
  sucursalId: number;
  /** Nombre de la sucursal, solo para mostrarlo en el título. */
  sucursalNombre?: string;
  bodega?: Bodega;
}

/** Diálogo de alta/edición de Bodega — la sucursal y "principal" solo se definen al crear. */
@Component({
  selector: 'app-bodega-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './bodega-form.html',
  styleUrl: './bodega-form.scss',
})
export class BodegaForm {
  private readonly fb = inject(FormBuilder);
  private readonly bodegasService = inject(Bodegas);
  private readonly dialogRef = inject<MatDialogRef<BodegaForm, Bodega | undefined>>(MatDialogRef);
  protected readonly data = inject<BodegaFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.bodega;
  protected readonly guardando = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    codigo: [this.data.bodega?.codigo ?? '', [Validators.required, Validators.maxLength(20)]],
    nombre: [this.data.bodega?.nombre ?? '', [Validators.required, Validators.maxLength(100)]],
    esPrincipal: [false],
  });

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const { codigo, nombre, esPrincipal } = this.form.getRawValue();

    const peticion = this.esEdicion
      ? this.bodegasService.actualizar(this.data.bodega!.publicId, { codigo, nombre })
      : this.bodegasService.crear({
          sucursalId: this.data.sucursalId,
          codigo,
          nombre,
          esPrincipal,
        });

    peticion.subscribe({
      next: (bodega) => this.dialogRef.close(bodega),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
