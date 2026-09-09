import { Routes } from '@angular/router';

/**
 * Rutas del módulo Orden de Trabajo. `nueva` va **antes** que `:publicId` para que no la
 * capture el parámetro. La edición es una ruta propia (`:publicId/editar`) y no un diálogo:
 * el formulario incluye la tabla de detalle, que no cabe en el panel de MatDialog — es la
 * excepción documentada al patrón "CRUD simple = diálogo" de src/frontend/CLAUDE.md.
 */
export const ORDENES_DE_TRABAJO_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/ordenes-de-trabajo-list/ordenes-de-trabajo-list').then(
        (m) => m.OrdenesDeTrabajoList,
      ),
  },
  {
    path: 'nueva',
    loadComponent: () =>
      import('./pages/orden-de-trabajo-form/orden-de-trabajo-form').then(
        (m) => m.OrdenDeTrabajoForm,
      ),
  },
  {
    path: ':publicId/editar',
    loadComponent: () =>
      import('./pages/orden-de-trabajo-form/orden-de-trabajo-form').then(
        (m) => m.OrdenDeTrabajoForm,
      ),
  },
  {
    path: ':publicId',
    loadComponent: () =>
      import('./pages/orden-de-trabajo-ficha/orden-de-trabajo-ficha').then(
        (m) => m.OrdenDeTrabajoFicha,
      ),
  },
];
