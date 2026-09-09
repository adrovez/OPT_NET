import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Anamnesis as AnamnesisModel } from '../../models/anamnesis.model';
import { Anamnesis } from '../../services/anamnesis';

export interface AnamnesisFormDialogData {
  clientePublicId: string;
}

/** Diálogo de alta de Anamnesis — inmutable una vez creada, no admite edición. */
@Component({
  selector: 'app-anamnesis-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './anamnesis-form.html',
  styleUrl: './anamnesis-form.scss',
})
export class AnamnesisForm {
  private readonly fb = inject(FormBuilder);
  private readonly anamnesisService = inject(Anamnesis);
  private readonly dialogRef =
    inject<MatDialogRef<AnamnesisForm, AnamnesisModel | undefined>>(MatDialogRef);
  protected readonly data = inject<AnamnesisFormDialogData>(MAT_DIALOG_DATA);

  protected readonly guardando = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    hipertension: [false],
    diabetes: [false],
    alergias: [false],
    detalleAlergias: ['', [Validators.maxLength(500)]],
    usaLentesPrevio: [false],
    observaciones: ['', [Validators.maxLength(500)]],
  });

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const { hipertension, diabetes, alergias, detalleAlergias, usaLentesPrevio, observaciones } =
      this.form.getRawValue();
    const datos = {
      hipertension,
      diabetes,
      alergias,
      detalleAlergias: alergias ? detalleAlergias || null : null,
      usaLentesPrevio,
      observaciones: observaciones || null,
    };

    this.anamnesisService.crear(this.data.clientePublicId, datos).subscribe({
      next: (anamnesis) => this.dialogRef.close(anamnesis),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
