import { Routes } from '@angular/router';

export const BODEGAS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/bodegas-list/bodegas-list').then((m) => m.BodegasList),
  },
];
