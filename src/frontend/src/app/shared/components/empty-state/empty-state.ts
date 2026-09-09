import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/**
 * Estado vacío/error genérico para listados — reemplaza el `<p class="vacio">` repetido en
 * cada feature. `tone="error"` se usa cuando la carga falló (distinto de "no hay datos"),
 * para que el usuario no confunda un error de red con una lista realmente vacía.
 * La acción (p. ej. "Reintentar" o "Crear el primero") se proyecta como contenido.
 */
@Component({
  selector: 'app-empty-state',
  imports: [MatIconModule],
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.scss',
})
export class EmptyState {
  readonly icon = input('inbox');
  readonly title = input<string>();
  readonly message = input.required<string>();
  readonly tone = input<'neutral' | 'error'>('neutral');
}
