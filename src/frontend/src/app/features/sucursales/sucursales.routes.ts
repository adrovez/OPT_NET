import { Routes } from '@angular/router';

export const SUCURSALES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/sucursales-list/sucursales-list').then((m) => m.SucursalesList),
  },
];
