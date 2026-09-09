import { Injectable, computed, effect, signal } from '@angular/core';

/** Preferencia de tema elegida por la persona. `system` = seguir al sistema operativo. */
export type PreferenciaTema = 'light' | 'dark' | 'system';

const CLAVE_ALMACEN = 'opt.tema';
const ATRIBUTO_HTML = 'data-opt-theme';

/**
 * Tema claro/oscuro de la aplicación.
 *
 * A diferencia del token de sesión (sessionStorage, ver `TokenStorage`), la preferencia de
 * tema es una comodidad de interfaz sin datos sensibles — se guarda en `localStorage` para
 * que persista entre pestañas y sesiones. Todo acceso va envuelto en try/catch: en modo
 * incógnito o con almacenamiento bloqueado la app debe seguir funcionando con el tema del
 * sistema.
 *
 * El servicio escribe `data-opt-theme` en `<html>`; `styles.scss` reacciona a ese atributo
 * (y, en su ausencia, a `prefers-color-scheme`).
 */
@Injectable({ providedIn: 'root' })
export class Tema {
  private readonly consultaOscuro =
    typeof window !== 'undefined' && typeof window.matchMedia === 'function'
      ? window.matchMedia('(prefers-color-scheme: dark)')
      : null;

  /** Lo que el sistema operativo reporta ahora mismo (se actualiza si cambia en vivo). */
  private readonly sistemaOscuro = signal(this.consultaOscuro?.matches ?? false);

  /** Preferencia elegida por la persona; `system` por defecto. */
  readonly preferencia = signal<PreferenciaTema>(this.leerPreferencia());

  /** `true` si el tema efectivo (resuelta la opción `system`) es oscuro. */
  readonly oscuro = computed(() => {
    const pref = this.preferencia();
    return pref === 'system' ? this.sistemaOscuro() : pref === 'dark';
  });

  constructor() {
    // Aplica de inmediato para no arrastrar el tema equivocado hasta el primer tick.
    this.aplicarAlDom(this.preferencia());

    this.consultaOscuro?.addEventListener('change', (evento) =>
      this.sistemaOscuro.set(evento.matches),
    );

    // Sincroniza el DOM y el almacén cada vez que cambia la preferencia.
    effect(() => {
      const pref = this.preferencia();
      this.aplicarAlDom(pref);
      this.guardarPreferencia(pref);
    });
  }

  private aplicarAlDom(preferencia: PreferenciaTema): void {
    const raiz = document.documentElement;
    if (preferencia === 'system') {
      raiz.removeAttribute(ATRIBUTO_HTML);
    } else {
      raiz.setAttribute(ATRIBUTO_HTML, preferencia);
    }
  }

  /** Fija una preferencia explícita. */
  fijar(preferencia: PreferenciaTema): void {
    this.preferencia.set(preferencia);
    // Aplicación síncrona: el atributo de <html> no debe esperar al próximo tick de
    // detección de cambios (evita un parpadeo del tema anterior). El `effect` del
    // constructor cubre cualquier `preferencia.set(...)` hecho desde fuera del servicio.
    this.aplicarAlDom(preferencia);
    this.guardarPreferencia(preferencia);
  }

  /** Alterna claro ↔ oscuro tomando como base el tema efectivo actual. */
  alternar(): void {
    this.preferencia.set(this.oscuro() ? 'light' : 'dark');
  }

  private leerPreferencia(): PreferenciaTema {
    try {
      const guardado = localStorage.getItem(CLAVE_ALMACEN);
      if (guardado === 'light' || guardado === 'dark' || guardado === 'system') {
        return guardado;
      }
    } catch {
      // Almacenamiento no disponible (incógnito, políticas del navegador) — sin preferencia.
    }
    return 'system';
  }

  private guardarPreferencia(preferencia: PreferenciaTema): void {
    try {
      localStorage.setItem(CLAVE_ALMACEN, preferencia);
    } catch {
      // Ídem: si no se puede persistir, el tema igual queda aplicado en esta sesión.
    }
  }
}
