import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

import { Sucursal } from '../../../sucursales/models/sucursal.model';
import { Usuario } from '../../models/usuario.model';
import { Usuarios } from '../../services/usuarios';

export interface UsuarioSucursalDialogData {
  usuario: Usuario;
  sucursales: Sucursal[];
}

/**
 * Asignar/quitar sucursal. El backend no expone un endpoint para listar las sucursales
 * ya asignadas a un usuario (UsuarioDto solo trae `sucursalActivaId`) — este diálogo
 * opera sin esa información, tal como lo permite la API actual.
 */
@Component({
  selector: 'app-usuario-sucursal-dialog',
  imports: [MatButtonModule, MatDialogModule, MatFormFieldModule, MatSelectModule],
  templateUrl: './usuario-sucursal-dialog.html',
  styleUrl: './usuario-sucursal-dialog.scss',
})
export class UsuarioSucursalDialog {
  private readonly usuariosService = inject(Usuarios);
  private readonly dialogRef = inject<MatDialogRef<UsuarioSucursalDialog, boolean>>(MatDialogRef);
  protected readonly data = inject<UsuarioSucursalDialogData>(MAT_DIALOG_DATA);

  protected readonly sucursalActivaNombre =
    this.data.sucursales.find((s) => s.id === this.data.usuario.sucursalActivaId)?.nombre ?? null;

  protected readonly sucursalId = signal<number | null>(null);
  protected readonly procesando = signal(false);

  protected asignar(): void {
    const id = this.sucursalId();
    if (!id || this.procesando()) {
      return;
    }

    this.procesando.set(true);
    this.usuariosService.asignarSucursal(this.data.usuario.publicId, id).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => this.procesando.set(false),
    });
  }

  protected quitar(): void {
    const id = this.sucursalId();
    if (!id || this.procesando()) {
      return;
    }

    this.procesando.set(true);
    this.usuariosService.quitarSucursal(this.data.usuario.publicId, id).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => this.procesando.set(false),
    });
  }

  protected cerrar(): void {
    this.dialogRef.close(false);
  }
}
