import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { map } from 'rxjs';

import { Auth } from '../../core/services/auth';
import { Tema } from '../../shared/services/tema';

/** Por debajo de este ancho el navbar horizontal no tiene espacio para los grupos y colapsa a un menú de hamburguesa. */
const ANCHO_QUIEBRE_MOVIL = '(max-width: 959.98px)';

interface ItemNav {
  ruta: string;
  etiqueta: string;
  icono: string;
}

/**
 * Agrupación del menú por módulo de dominio (Clínico/Comercial/Inventario/Organización),
 * la misma división que usan OPT.Domain y este CLAUDE.md — a diferencia del legacy,
 * que agrupaba por rol de usuario ("Administrar", "Atención") y mezclaba módulos no
 * relacionados bajo un mismo ítem. Agrupar por dominio evita reordenar el menú cada vez
 * que cambie el modelo de roles.
 */
interface GrupoNav {
  etiqueta: string;
  icono: string;
  items: ItemNav[];
}

/** Layout de la zona autenticada: barra superior con navegación horizontal + contenido ruteado. */
@Component({
  selector: 'app-shell',
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    MatIconModule,
    MatMenuModule,
    MatToolbarModule,
    MatButtonModule,
    MatTooltipModule,
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  protected readonly auth = inject(Auth);
  protected readonly tema = inject(Tema);
  private readonly breakpointObserver = inject(BreakpointObserver);

  /** En pantallas angostas (tablet/celular) los grupos de navegación colapsan a un menú de hamburguesa. */
  protected readonly esMovil = toSignal(
    this.breakpointObserver
      .observe(ANCHO_QUIEBRE_MOVIL)
      .pipe(map((resultado) => resultado.matches)),
    { initialValue: false },
  );

  protected readonly gruposNav: GrupoNav[] = [
    {
      etiqueta: 'Clínico',
      icono: 'health_and_safety',
      items: [{ ruta: '/clientes', etiqueta: 'Clientes', icono: 'people' }],
    },
    {
      etiqueta: 'Comercial',
      icono: 'receipt_long',
      items: [
        { ruta: '/ordenes-de-trabajo', etiqueta: 'Órdenes de trabajo', icono: 'assignment' },
        { ruta: '/abonos', etiqueta: 'Abonos', icono: 'savings' },
        { ruta: '/pagos', etiqueta: 'Pagos', icono: 'payments' },
        { ruta: '/cuotas', etiqueta: 'Cuotas', icono: 'calendar_month' },
        { ruta: '/cobranza', etiqueta: 'Cobranza', icono: 'account_balance' },
      ],
    },
    {
      etiqueta: 'Inventario',
      icono: 'inventory_2',
      items: [{ ruta: '/inventario', etiqueta: 'Productos', icono: 'category' }],
    },
    {
      etiqueta: 'Organización',
      icono: 'apartment',
      items: [
        { ruta: '/sucursales', etiqueta: 'Sucursales', icono: 'storefront' },
        { ruta: '/empresas', etiqueta: 'Empresas', icono: 'business' },
        { ruta: '/usuarios', etiqueta: 'Usuarios', icono: 'manage_accounts' },
        { ruta: '/roles', etiqueta: 'Roles', icono: 'admin_panel_settings' },
      ],
    },
  ];

  protected cerrarSesion(): void {
    this.auth.logout();
  }
}
