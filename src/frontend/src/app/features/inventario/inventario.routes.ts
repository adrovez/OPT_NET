import { Routes } from '@angular/router';

export const INVENTARIO_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/inventario-list/inventario-list').then((m) => m.InventarioList),
  },
];
