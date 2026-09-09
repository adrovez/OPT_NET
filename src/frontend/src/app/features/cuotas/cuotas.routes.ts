import { Routes } from '@angular/router';

/**
 * Módulo Cuotas. Una sola pantalla: elegir la OT (o recibirla en `?ot=<publicId>` desde su
 * ficha) y administrar su plan de pago. La cuota es un subrecurso de la OT, no un recurso
 * propio de la API.
 */
export const CUOTAS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/cuotas-plan/cuotas-plan').then((m) => m.CuotasPlan),
  },
];
