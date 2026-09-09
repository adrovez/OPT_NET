import { Routes } from '@angular/router';

/**
 * Módulo Abonos. Una sola pantalla: elegir la OT (o recibirla en `?ot=<publicId>` desde su
 * ficha) y registrar el abono. No hay ruta de detalle — el abono es un subrecurso de la OT.
 */
export const ABONOS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/abonos-registro/abonos-registro').then((m) => m.AbonosRegistro),
  },
];
