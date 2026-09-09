import { Component, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';

/**
 * Encabezado de página estándar: título (+ subtítulo opcional), botón de volver opcional y
 * un slot a la derecha para la acción primaria de la pantalla (proyectado como contenido).
 *
 * Reemplaza el `<div class="encabezado">` que cada feature reimplementaba por separado
 * (Clientes/Empresas/Usuarios/Sucursales/Roles) — ver src/frontend/CLAUDE.md.
 */
@Component({
  selector: 'app-page-header',
  imports: [MatButtonModule, MatIconModule, MatTooltipModule, RouterLink],
  templateUrl: './page-header.html',
  styleUrl: './page-header.scss',
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
  /** Ruta a la que vuelve el botón de flecha; si no se pasa, no se muestra el botón. */
  readonly backTo = input<string>();
  readonly backLabel = input('Volver');
}
