import { Routes } from '@angular/router';

/**
 * Módulo Pagos. Una sola pantalla: elegir la OT con saldo (o recibirla en `?ot=<publicId>`
 * desde su ficha) y registrar el cobro. El pago es un subrecurso de la OT, no un recurso
 * propio de la API.
 */
export const PAGOS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/pagos-registro/pagos-registro').then((m) => m.PagosRegistro),
  },
];
