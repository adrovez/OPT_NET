import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { JwtClaims, LoginRequest, LoginResponse, UsuarioActual } from '../models/auth.models';
import { TokenStorage } from './token-storage';
import { decodeJwtPayload } from '../utils/jwt.util';

/**
 * Maneja la sesión del usuario. Fuente de verdad en memoria (signal), respaldada por el
 * JWT en sessionStorage para sobrevivir a un refresh de página (no hay endpoint /me
 * todavía — la sesión se reconstruye decodificando los claims del propio token).
 */
@Injectable({
  providedIn: 'root',
})
export class Auth {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokenStorage = inject(TokenStorage);

  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly usuarioActualSignal = signal<UsuarioActual | null>(this.restaurarSesion());

  readonly usuarioActual = this.usuarioActualSignal.asReadonly();
  readonly estaAutenticado = computed(() => this.usuarioActualSignal() !== null);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request).pipe(
      tap((respuesta) => {
        this.tokenStorage.guardar(respuesta.token);
        this.usuarioActualSignal.set(this.usuarioDesdeToken(respuesta.token));
      }),
    );
  }

  logout(): void {
    this.tokenStorage.limpiar();
    this.usuarioActualSignal.set(null);
    this.router.navigateByUrl('/login');
  }

  obtenerToken(): string | null {
    return this.tokenStorage.obtener();
  }

  private restaurarSesion(): UsuarioActual | null {
    const token = this.tokenStorage.obtener();
    if (!token) {
      return null;
    }

    const usuario = this.usuarioDesdeToken(token);
    if (!usuario || this.tokenExpirado(token)) {
      this.tokenStorage.limpiar();
      return null;
    }

    return usuario;
  }

  private usuarioDesdeToken(token: string): UsuarioActual | null {
    try {
      const claims = decodeJwtPayload<JwtClaims>(token);
      return {
        usuarioId: Number(claims.sub),
        nombreCompleto: claims.nombre,
        rut: claims.rut,
        rolId: Number(claims.rolId),
        sucursalActivaId: Number(claims.sucursalId),
      };
    } catch {
      return null;
    }
  }

  private tokenExpirado(token: string): boolean {
    const claims = decodeJwtPayload<JwtClaims>(token);
    return claims.exp * 1000 <= Date.now();
  }
}
