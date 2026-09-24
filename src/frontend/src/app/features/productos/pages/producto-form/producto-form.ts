import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Producto } from '../../../inventario/models/producto.model';
import { Productos } from '../../../inventario/services/productos';

export interface ProductoFormDialogData {
  producto?: Producto;
}

/** Diálogo de alta/edición de Producto. */
@Component({
  selector: 'app-producto-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './producto-form.html',
  styleUrl: './producto-form.scss',
})
export class ProductoForm {
  private readonly fb = inject(FormBuilder);
  private readonly productosService = inject(Productos);
  private readonly dialogRef =
    inject<MatDialogRef<ProductoForm, Producto | undefined>>(MatDialogRef);
  protected readonly data = inject<ProductoFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.producto;
  protected readonly guardando = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    codigo: [this.data.producto?.codigo ?? '', [Validators.required, Validators.maxLength(50)]],
    descripcion: [
      this.data.producto?.descripcion ?? '',
      [Validators.required, Validators.maxLength(200)],
    ],
    controlStock: [this.data.producto?.controlStock ?? true],
  });

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const datos = this.form.getRawValue();

    const peticion = this.esEdicion
      ? this.productosService.actualizar(this.data.producto!.id, datos)
      : this.productosService.crear(datos);

    peticion.subscribe({
      next: (producto) => this.dialogRef.close(producto),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
