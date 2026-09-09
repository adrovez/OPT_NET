import { Injectable } from '@angular/core';

const TOKEN_KEY = 'opt.token';

/**
 * Encapsula el acceso al token de sesión en el navegador.
 *
 * Se usa sessionStorage (no localStorage): el token se pierde al cerrar la pestaña,
 * lo que acota la ventana de exposición para un sistema que maneja datos clínicos
 * sensibles (Ley 21.719). No se guarda nada más que el token (AGENTS.md, regla 12
 * y sección "Frontend").
 */
@Injectable({
  providedIn: 'root',
})
export class TokenStorage {
  obtener(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  guardar(token: string): void {
    sessionStorage.setItem(TOKEN_KEY, token);
  }

  limpiar(): void {
    sessionStorage.removeItem(TOKEN_KEY);
  }
}
