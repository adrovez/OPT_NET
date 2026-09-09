import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { SearchBox } from '../../../../shared/components/search-box/search-box';
import { crearMatPaginatorIntlEs } from '../../../../shared/i18n/mat-paginator-intl-es';
import { OPCIONES_TAMANIO_PAGINA } from '../../../../shared/models/parametros-consulta-paginada.model';
import { EstadoListaPaginada } from '../../../../shared/utils/estado-lista-paginada';
import { Toast } from '../../../../shared/services/toast';
import { Sucursal } from '../../../sucursales/models/sucursal.model';
import { Sucursales } from '../../../sucursales/services/sucursales';
import { Usuario } from '../../models/usuario.model';
import { Usuarios } from '../../services/usuarios';
import { UsuarioClaveDialog } from '../usuario-clave-dialog/usuario-clave-dialog';
import { UsuarioForm } from '../usuario-form/usuario-form';
import { UsuarioSucursalDialog } from '../usuario-sucursal-dialog/usuario-sucursal-dialog';

@Component({
  selector: 'app-usuarios-list',
  imports: [
    MatButtonModule,
    MatChipsModule,
    MatIconModule,
    MatMenuModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    MatTooltipModule,
    EmptyState,
    ListSkeleton,
    PageHeader,
    SearchBox,
  ],
  providers: [{ provide: MatPaginatorIntl, useFactory: crearMatPaginatorIntlEs }],
  templateUrl: './usuarios-list.html',
  styleUrl: './usuarios-list.scss',
})
export class UsuariosList {
  private readonly usuariosService = inject(Usuarios);
  private readonly sucursalesService = inject(Sucursales);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);

  protected readonly columnas = [
    'rut',
    'nombre',
    'rolNombre',
    'sucursalActiva',
    'activo',
    'acciones',
  ];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly estado = new EstadoListaPaginada<Usuario>((p) =>
    this.usuariosService.buscar(p),
  );

  /** Catálogo completo de sucursales (para el nombre de la sucursal activa y el diálogo de asignación). */
  protected readonly sucursales = signal<Sucursal[]>([]);

  private readonly sucursalesPorId = computed(
    () => new Map(this.sucursales().map((sucursal) => [sucursal.id, sucursal.nombre])),
  );

  constructor() {
    this.estado.cargar();
    this.sucursalesService.listar().subscribe((sucursales) => this.sucursales.set(sucursales));
  }

  protected nombreSucursal(id: number | null): string {
    if (id === null) {
      return '—';
    }
    return this.sucursalesPorId().get(id) ?? `#${id}`;
  }

  protected nuevo(): void {
    this.dialog
      .open(UsuarioForm, { data: {} })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Usuario creado. Asígnale una sucursal para que pueda iniciar sesión.');
          this.estado.refrescar();
        }
      });
  }

  protected editar(usuario: Usuario): void {
    this.dialog
      .open(UsuarioForm, { data: { usuario } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Usuario actualizado.');
          this.estado.refrescar();
        }
      });
  }

  protected cambiarClave(usuario: Usuario): void {
    this.dialog
      .open(UsuarioClaveDialog, { data: { usuario } })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.toast.exito('Clave actualizada.');
        }
      });
  }

  protected gestionarSucursales(usuario: Usuario): void {
    this.dialog
      .open(UsuarioSucursalDialog, { data: { usuario, sucursales: this.sucursales() } })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.toast.exito('Sucursales del usuario actualizadas.');
        }
      });
  }

  protected alternarActivo(usuario: Usuario): void {
    const peticion = usuario.activo
      ? this.usuariosService.desactivar(usuario.publicId)
      : this.usuariosService.activar(usuario.publicId);

    peticion.subscribe({
      next: () => {
        this.toast.exito(usuario.activo ? 'Usuario desactivado.' : 'Usuario activado.');
        this.estado.refrescar();
      },
    });
  }

  protected eliminar(usuario: Usuario): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Eliminar usuario',
          mensaje: `¿Eliminar al usuario "${usuario.nombre} ${usuario.apellido}"?`,
          textoConfirmar: 'Eliminar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.usuariosService.eliminar(usuario.publicId).subscribe({
            next: () => {
              this.toast.exito('Usuario eliminado.');
              this.estado.refrescar();
            },
          });
        }
      });
  }
}
