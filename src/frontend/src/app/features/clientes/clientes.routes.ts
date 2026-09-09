import { Routes } from '@angular/router';

export const CLIENTES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/clientes-list/clientes-list').then((m) => m.ClientesList),
  },
  {
    path: ':publicId',
    loadComponent: () => import('./pages/cliente-ficha/cliente-ficha').then((m) => m.ClienteFicha),
  },
];
