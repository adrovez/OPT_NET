import { Routes } from '@angular/router';

/**
 * Rutas del módulo Operativo. Alta y edición son un diálogo (`OperativoForm`) desde el
 * listado — es un formulario chico (empresa/sucursal/fecha/observación), no la excepción que
 * justificó una página ruteada para Orden de Trabajo. La ficha sí es ruteada: agrupa dos
 * historiales de subrecurso a la vez (Órdenes asociadas + Gastos), igual criterio que
 * `cliente-ficha`/`orden-de-trabajo-ficha`.
 */
export const OPERATIVOS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/operativos-list/operativos-list').then((m) => m.OperativosList),
  },
  {
    path: ':publicId',
    loadComponent: () =>
      import('./pages/operativo-ficha/operativo-ficha').then((m) => m.OperativoFicha),
  },
];
