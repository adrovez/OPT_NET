import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Usuario } from '../../models/usuario.model';
import { Usuarios } from '../../services/usuarios';

export interface UsuarioClaveDialogData {
  usuario: Usuario;
}

/**
 * CambiarClaveUsuarioCommand verifica `claveActual` contra el hash del usuario objetivo
 * (no del administrador que ejecuta la acción) — no es un "reseteo" sin conocer la clave
 * vigente. Se lo indica explícitamente en el diálogo para no inducir a error.
 */
@Component({
  selector: 'app-usuario-clave-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './usuario-clave-dialog.html',
  styleUrl: './usuario-clave-dialog.scss',
})
export class UsuarioClaveDialog {
  private readonly fb = inject(FormBuilder);
  private readonly usuariosService = inject(Usuarios);
  private readonly dialogRef = inject<MatDialogRef<UsuarioClaveDialog, boolean>>(MatDialogRef);
  protected readonly data = inject<UsuarioClaveDialogData>(MAT_DIALOG_DATA);

  protected readonly guardando = signal(false);
  protected readonly ocultarActual = signal(true);
  protected readonly ocultarNueva = signal(true);

  protected readonly form = this.fb.nonNullable.group({
    claveActual: ['', [Validators.required]],
    claveNueva: ['', [Validators.required, Validators.minLength(6)]],
  });

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    this.usuariosService
      .cambiarClave(this.data.usuario.publicId, this.form.getRawValue())
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: () => this.guardando.set(false),
      });
  }

  protected cancelar(): void {
    this.dialogRef.close(false);
  }
}
