import { Component, computed, inject, signal } from '@angular/core';
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
import { Sucursal } from '../../features/sucursales/models/sucursal.model';
import { Sucursales } from '../../features/sucursales/services/sucursales';

/** Por debajo de este ancho el navbar horizontal no tiene espacio para los grupos y colapsa a un menú de hamburguesa. */
const ANCHO_QUIEBRE_MOVIL = '(max-width: 959.98px)';

interface ItemNav {
  ruta: string;
  etiqueta: string;
  icono: string;
}

/**
 * Agrupación del menú en 4 grupos (Administración/Comercial/Inventario/Reportes), en orden
 * alfabético, con los ítems también alfabéticos dentro de cada grupo — a diferencia del legacy,
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
  private readonly sucursalesService = inject(Sucursales);
  private readonly breakpointObserver = inject(BreakpointObserver);

  /**
   * Sucursales asignadas al usuario conectado, con nombre — el legacy las mostraba en un
   * `<select>` del menú (ver captura adjunta al pedido). El cambio de sucursal no vuelve a
   * pedir nada al backend: `Auth.cambiarSucursal` ya valida contra la lista de asignadas.
   */
  protected readonly misSucursales = signal<Sucursal[]>([]);

  protected readonly sucursalActual = computed(() => {
    const id = this.auth.sucursalActualId();
    return this.misSucursales().find((s) => s.id === id) ?? null;
  });

  /** En pantallas angostas (tablet/celular) los grupos de navegación colapsan a un menú de hamburguesa. */
  protected readonly esMovil = toSignal(
    this.breakpointObserver
      .observe(ANCHO_QUIEBRE_MOVIL)
      .pipe(map((resultado) => resultado.matches)),
    { initialValue: false },
  );

  /** Menú definido en `doc_Cliente/Menu-Roles.xlsx`. Sin filtro por rol por ahora: todo visible para las pruebas. */
  protected readonly gruposNav: GrupoNav[] = [
    {
      etiqueta: 'Administración',
      icono: 'apartment',
      items: [
        { ruta: '/empresas', etiqueta: 'Empresas', icono: 'business' },
        { ruta: '/sucursales', etiqueta: 'Sucursales', icono: 'storefront' },
        { ruta: '/usuarios', etiqueta: 'Usuarios', icono: 'manage_accounts' },
      ],
    },
    {
      etiqueta: 'Comercial',
      icono: 'point_of_sale',
      items: [
        { ruta: '/clientes', etiqueta: 'Clientes', icono: 'people' },
        { ruta: '/cobranza', etiqueta: 'Cobranza', icono: 'account_balance' },
        { ruta: '/cobranza/reporte', etiqueta: 'Cobranza: Reporte', icono: 'summarize' },
        { ruta: '/operativos', etiqueta: 'Operativos', icono: 'event_note' },
        { ruta: '/ordenes-de-trabajo', etiqueta: 'Órdenes de Trabajo', icono: 'assignment' },
      ],
    },
    {
      etiqueta: 'Inventario',
      icono: 'inventory_2',
      items: [
        { ruta: '/inventario/ajustes', etiqueta: 'Ajustes', icono: 'tune' },
        { ruta: '/compras', etiqueta: 'Compras', icono: 'shopping_cart' },
        { ruta: '/inventario/enviar', etiqueta: 'Enviar', icono: 'outbox' },
        { ruta: '/productos', etiqueta: 'Productos', icono: 'category' },
        { ruta: '/inventario/recibir', etiqueta: 'Recibir', icono: 'move_to_inbox' },
        { ruta: '/inventario', etiqueta: 'Stock', icono: 'inventory' },
      ],
    },
    {
      etiqueta: 'Reportes',
      icono: 'assessment',
      items: [{ ruta: '/reportes', etiqueta: 'Reportes', icono: 'assessment' }],
    },
  ];

  constructor() {
    // Catálogo chico (unas pocas sucursales por usuario) — se filtra en el cliente en vez
    // de exponer un endpoint "mis sucursales" para un dato que ya viaja en el JWT.
    this.sucursalesService.listar().subscribe((todas) => {
      const asignadas = new Set(this.auth.usuarioActual()?.sucursalesAsignadas ?? []);
      this.misSucursales.set(todas.filter((s) => asignadas.has(s.id)));
    });
  }

  protected cambiarSucursal(sucursal: Sucursal): void {
    this.auth.cambiarSucursal(sucursal.id);
  }

  protected cerrarSesion(): void {
    this.auth.logout();
  }
}
