import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import { Rol } from '../../../roles/models/rol.model';
import { Roles } from '../../../roles/services/roles';
import { Usuario } from '../../models/usuario.model';
import { Usuarios } from '../../services/usuarios';

export interface UsuarioFormDialogData {
  usuario?: Usuario;
}

/** Diálogo de alta/edición de Usuario. La clave solo se pide al crear (ver CambiarClave para editarla). */
@Component({
  selector: 'app-usuario-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './usuario-form.html',
  styleUrl: './usuario-form.scss',
})
export class UsuarioForm {
  private readonly fb = inject(FormBuilder);
  private readonly usuariosService = inject(Usuarios);
  private readonly rolesService = inject(Roles);
  private readonly dialogRef = inject<MatDialogRef<UsuarioForm, Usuario | undefined>>(MatDialogRef);
  protected readonly data = inject<UsuarioFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.usuario;
  protected readonly guardando = signal(false);
  protected readonly ocultarClave = signal(true);
  protected readonly roles = signal<Rol[]>([]);

  protected readonly form = this.fb.nonNullable.group({
    rut: [this.data.usuario?.rut ?? '', [Validators.required, Validators.maxLength(12)]],
    nombre: [this.data.usuario?.nombre ?? '', [Validators.required, Validators.maxLength(100)]],
    apellido: [this.data.usuario?.apellido ?? '', [Validators.required, Validators.maxLength(100)]],
    email: [this.data.usuario?.email ?? '', [Validators.maxLength(150), Validators.email]],
    rolId: [this.data.usuario?.rolId ?? 0, [Validators.min(1)]],
    clave: ['', this.esEdicion ? [] : [Validators.required, Validators.minLength(6)]],
  });

  constructor() {
    this.rolesService.listar().subscribe((roles) => this.roles.set(roles));
  }

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const { rut, nombre, apellido, email, rolId, clave } = this.form.getRawValue();

    const peticion = this.esEdicion
      ? this.usuariosService.actualizar(this.data.usuario!.publicId, {
          rut,
          nombre,
          apellido,
          email,
          rolId,
        })
      : this.usuariosService.crear({ rut, nombre, apellido, email, rolId, clave });

    peticion.subscribe({
      next: (usuario) => this.dialogRef.close(usuario),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
