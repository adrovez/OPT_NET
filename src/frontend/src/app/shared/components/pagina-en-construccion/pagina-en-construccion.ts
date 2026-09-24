import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { EmptyState } from '../empty-state/empty-state';
import { PageHeader } from '../page-header/page-header';

/** Pantalla provisoria para ítems de menú cuyo módulo aún no existe. El título llega por `data.titulo` de la ruta. */
@Component({
  selector: 'app-pagina-en-construccion',
  imports: [EmptyState, PageHeader],
  template: `
    <app-page-header [title]="titulo" />
    <app-empty-state
      icon="construction"
      title="Módulo en construcción"
      message="Esta sección estará disponible en una próxima etapa."
    />
  `,
})
export class PaginaEnConstruccion {
  protected readonly titulo: string = inject(ActivatedRoute).snapshot.data['titulo'] ?? '';
}
