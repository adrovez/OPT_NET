import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Empresa } from '../../models/empresa.model';
import { Empresas } from '../../services/empresas';

export interface EmpresaFormDialogData {
  empresa?: Empresa;
}

/** Diálogo de alta/edición de Empresa — mismo shape de campos para crear y actualizar. */
@Component({
  selector: 'app-empresa-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './empresa-form.html',
  styleUrl: './empresa-form.scss',
})
export class EmpresaForm {
  private readonly fb = inject(FormBuilder);
  private readonly empresasService = inject(Empresas);
  private readonly dialogRef = inject<MatDialogRef<EmpresaForm, Empresa | undefined>>(MatDialogRef);
  protected readonly data = inject<EmpresaFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.empresa;
  protected readonly guardando = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    nombre: [this.data.empresa?.nombre ?? '', [Validators.required, Validators.maxLength(100)]],
    rut: [this.data.empresa?.rut ?? '', [Validators.required, Validators.maxLength(12)]],
    razonSocial: [this.data.empresa?.razonSocial ?? '', [Validators.maxLength(150)]],
    giro: [this.data.empresa?.giro ?? '', [Validators.maxLength(150)]],
    direccion: [this.data.empresa?.direccion ?? '', [Validators.maxLength(200)]],
    telefono: [this.data.empresa?.telefono ?? '', [Validators.maxLength(20)]],
    email: [this.data.empresa?.email ?? '', [Validators.maxLength(150), Validators.email]],
    contacto: [this.data.empresa?.contacto ?? '', [Validators.maxLength(100)]],
  });

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const datos = this.form.getRawValue();

    const peticion = this.esEdicion
      ? this.empresasService.actualizar(this.data.empresa!.publicId, datos)
      : this.empresasService.crear(datos);

    peticion.subscribe({
      next: (empresa) => this.dialogRef.close(empresa),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
