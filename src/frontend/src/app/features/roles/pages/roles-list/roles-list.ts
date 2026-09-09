import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';

import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { Rol } from '../../models/rol.model';
import { Roles } from '../../services/roles';

/** Catálogo de roles — solo lectura, sin acciones (RolesController no tiene mutadores). */
@Component({
  selector: 'app-roles-list',
  imports: [MatButtonModule, MatIconModule, MatTableModule, EmptyState, ListSkeleton, PageHeader],
  templateUrl: './roles-list.html',
  styleUrl: './roles-list.scss',
})
export class RolesList {
  private readonly rolesService = inject(Roles);

  protected readonly columnas = ['nombre', 'descripcion'];
  protected readonly roles = signal<Rol[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal(false);

  constructor() {
    this.cargar();
  }

  protected cargar(): void {
    this.cargando.set(true);
    this.error.set(false);
    this.rolesService.listar().subscribe({
      next: (roles) => {
        this.roles.set(roles);
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
        this.error.set(true);
      },
    });
  }
}
