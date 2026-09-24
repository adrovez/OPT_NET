import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./layout/auth-layout/auth-layout').then((m) => m.AuthLayout),
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell/shell').then((m) => m.Shell),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'clientes', pathMatch: 'full' },
      {
        path: 'clientes',
        loadChildren: () =>
          import('./features/clientes/clientes.routes').then((m) => m.CLIENTES_ROUTES),
      },
      {
        path: 'ordenes-de-trabajo',
        loadChildren: () =>
          import('./features/ordenes-de-trabajo/ordenes-de-trabajo.routes').then(
            (m) => m.ORDENES_DE_TRABAJO_ROUTES,
          ),
      },
      {
        path: 'abonos',
        loadChildren: () => import('./features/abonos/abonos.routes').then((m) => m.ABONOS_ROUTES),
      },
      {
        path: 'pagos',
        loadChildren: () => import('./features/pagos/pagos.routes').then((m) => m.PAGOS_ROUTES),
      },
      {
        path: 'cuotas',
        loadChildren: () => import('./features/cuotas/cuotas.routes').then((m) => m.CUOTAS_ROUTES),
      },
      {
        path: 'cobranza/reporte',
        data: { titulo: 'Cobranza · Reporte' },
        loadComponent: () =>
          import('./shared/components/pagina-en-construccion/pagina-en-construccion').then(
            (m) => m.PaginaEnConstruccion,
          ),
      },
      {
        path: 'compras',
        data: { titulo: 'Compras' },
        loadComponent: () =>
          import('./shared/components/pagina-en-construccion/pagina-en-construccion').then(
            (m) => m.PaginaEnConstruccion,
          ),
      },
      {
        path: 'inventario/ajustes',
        data: { titulo: 'Inventario · Ajustes' },
        loadComponent: () =>
          import('./shared/components/pagina-en-construccion/pagina-en-construccion').then(
            (m) => m.PaginaEnConstruccion,
          ),
      },
      {
        path: 'inventario/enviar',
        data: { titulo: 'Inventario · Enviar' },
        loadComponent: () =>
          import('./shared/components/pagina-en-construccion/pagina-en-construccion').then(
            (m) => m.PaginaEnConstruccion,
          ),
      },
      {
        path: 'inventario/recibir',
        data: { titulo: 'Inventario · Recibir' },
        loadComponent: () =>
          import('./shared/components/pagina-en-construccion/pagina-en-construccion').then(
            (m) => m.PaginaEnConstruccion,
          ),
      },
      {
        path: 'productos',
        loadChildren: () =>
          import('./features/productos/productos.routes').then((m) => m.PRODUCTOS_ROUTES),
      },
      {
        path: 'reportes',
        data: { titulo: 'Reportes' },
        loadComponent: () =>
          import('./shared/components/pagina-en-construccion/pagina-en-construccion').then(
            (m) => m.PaginaEnConstruccion,
          ),
      },
      {
        path: 'cobranza',
        loadChildren: () =>
          import('./features/cobranza/cobranza.routes').then((m) => m.COBRANZA_ROUTES),
      },
      {
        path: 'operativos',
        loadChildren: () =>
          import('./features/operativos/operativos.routes').then((m) => m.OPERATIVOS_ROUTES),
      },
      {
        path: 'inventario',
        loadChildren: () =>
          import('./features/inventario/inventario.routes').then((m) => m.INVENTARIO_ROUTES),
      },
      {
        path: 'sucursales',
        loadChildren: () =>
          import('./features/sucursales/sucursales.routes').then((m) => m.SUCURSALES_ROUTES),
      },
      {
        path: 'empresas',
        loadChildren: () =>
          import('./features/empresas/empresas.routes').then((m) => m.EMPRESAS_ROUTES),
      },
      {
        path: 'usuarios',
        loadChildren: () =>
          import('./features/usuarios/usuarios.routes').then((m) => m.USUARIOS_ROUTES),
      },
      {
        path: 'roles',
        loadChildren: () => import('./features/roles/roles.routes').then((m) => m.ROLES_ROUTES),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
