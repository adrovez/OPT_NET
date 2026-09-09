import { Routes } from '@angular/router';

/** Módulo Cobranza: una sola pantalla con la deuda consolidada por empresa. */
export const COBRANZA_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/cobranza-deudores/cobranza-deudores').then((m) => m.CobranzaDeudores),
  },
];
