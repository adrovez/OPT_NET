import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

/** Notificaciones breves al usuario (reemplaza al SweetAlert del legacy). */
@Injectable({
  providedIn: 'root',
})
export class Toast {
  private readonly snackBar = inject(MatSnackBar);

  exito(mensaje: string): void {
    this.snackBar.open(mensaje, 'Cerrar', { duration: 3000, panelClass: 'toast-exito' });
  }

  error(mensaje: string): void {
    this.snackBar.open(mensaje, 'Cerrar', { duration: 5000, panelClass: 'toast-error' });
  }
}
