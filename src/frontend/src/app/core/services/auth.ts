import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { JwtClaims, LoginRequest, LoginResponse, UsuarioActual } from '../models/auth.models';
import { TokenStorage } from './token-storage';
import { decodeJwtPayload } from '../utils/jwt.util';

/** Sucursal elegida en el selector del menú — no es dato sensible (comodidad de sesión, ver `Tema`). */
const CLAVE_SUCURSAL_ACTUAL = 'opt.sucursalActual';

/**
 * Maneja la sesión del usuario. Fuente de verdad en memoria (signal), respaldada por el
 * JWT en sessionStorage para sobrevivir a un refresh de página (no hay endpoint /me
 * todavía — la sesión se reconstruye decodificando los claims del propio token).
 *
 * También mantiene la "sucursal actual" del menú (equivalente al selector del legacy):
 * el JWT emitido al iniciar sesión trae fija la primera sucursal asignada, pero el cambio
 * de sucursal en el menú es puramente de sesión en el frontend — el backend ya autoriza
 * cualquier operación contra **cualquiera** de las sucursales asignadas al usuario
 * (`AutorizacionSucursal.ValidarAcceso`, no solo la activa del token), así que no hace
 * falta reemitir el JWT para cambiarla.
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
  private readonly sucursalActualSignal = signal<number | null>(
    this.restaurarSucursalActual(this.usuarioActualSignal()),
  );

  readonly usuarioActual = this.usuarioActualSignal.asReadonly();
  readonly estaAutenticado = computed(() => this.usuarioActualSignal() !== null);

  /** Sucursal elegida en el menú — la que debe usarse para dar de alta cualquier dato nuevo. */
  readonly sucursalActualId = this.sucursalActualSignal.asReadonly();

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request).pipe(
      tap((respuesta) => {
        this.tokenStorage.guardar(respuesta.token);
        const usuario = this.usuarioDesdeToken(respuesta.token);
        this.usuarioActualSignal.set(usuario);
        // Sesión nueva: siempre arranca en la sucursal activa que fijó el backend
        // (la primera asignada, regla del legacy), descartando cualquier elección previa.
        this.fijarSucursalActual(usuario?.sucursalActivaId ?? null);
      }),
    );
  }

  logout(): void {
    this.tokenStorage.limpiar();
    this.usuarioActualSignal.set(null);
    this.fijarSucursalActual(null);
    this.router.navigateByUrl('/login');
  }

  obtenerToken(): string | null {
    return this.tokenStorage.obtener();
  }

  /**
   * Cambia la sucursal actual de la sesión — equivalente al selector del menú del legacy.
   * Solo acepta una sucursal de las asignadas al usuario; cualquier otro valor se ignora
   * silenciosamente (el selector del menú nunca debería ofrecer una que no corresponda).
   */
  cambiarSucursal(sucursalId: number): void {
    if (!this.usuarioActualSignal()?.sucursalesAsignadas.includes(sucursalId)) {
      return;
    }
    this.fijarSucursalActual(sucursalId);
  }

  private fijarSucursalActual(sucursalId: number | null): void {
    this.sucursalActualSignal.set(sucursalId);
    try {
      if (sucursalId === null) {
        sessionStorage.removeItem(CLAVE_SUCURSAL_ACTUAL);
      } else {
        sessionStorage.setItem(CLAVE_SUCURSAL_ACTUAL, String(sucursalId));
      }
    } catch {
      // Almacenamiento no disponible (incógnito, políticas del navegador) — la sesión sigue
      // funcionando, solo no sobrevive a un refresh de página.
    }
  }

  private restaurarSucursalActual(usuario: UsuarioActual | null): number | null {
    if (!usuario) {
      return null;
    }

    try {
      const guardado = Number(sessionStorage.getItem(CLAVE_SUCURSAL_ACTUAL));
      if (guardado && usuario.sucursalesAsignadas.includes(guardado)) {
        return guardado;
      }
    } catch {
      // Ídem restaurarSesion: sin almacenamiento disponible, se cae a la sucursal activa.
    }

    return usuario.sucursalActivaId;
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
        sucursalesAsignadas: claims.sucursales
          ? claims.sucursales.split(',').map(Number)
          : [Number(claims.sucursalId)],
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
