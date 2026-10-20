import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import { Auth } from '../../../../core/services/auth';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { SearchBox } from '../../../../shared/components/search-box/search-box';
import { crearMatPaginatorIntlEs } from '../../../../shared/i18n/mat-paginator-intl-es';
import { OPCIONES_TAMANIO_PAGINA } from '../../../../shared/models/parametros-consulta-paginada.model';
import { Toast } from '../../../../shared/services/toast';
import { EstadoListaPaginada } from '../../../../shared/utils/estado-lista-paginada';
import { Sucursal } from '../../../sucursales/models/sucursal.model';
import { Sucursales } from '../../../sucursales/services/sucursales';
import { Bodega } from '../../models/bodega.model';
import { Bodegas } from '../../services/bodegas';
import { BodegaForm } from '../bodega-form/bodega-form';

/** Rol Administrador: alcance nacional — puede ver las bodegas de todas las sucursales. */
const ROL_ADMINISTRADOR = 1;

/** Valor del `mat-select` para "todas las sucursales" (solo Administrador). */
const TODAS = 0;

/**
 * Listado de Bodegas (ADR 0012). Siempre se mira una sucursal a la vez: por defecto la activa de
 * la sesión, o la que llegue en `?sucursalId=` (enlace desde el listado de Sucursales).
 */
@Component({
  selector: 'app-bodegas-list',
  imports: [
    MatButtonModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatMenuModule,
    MatPaginatorModule,
    MatSelectModule,
    MatSortModule,
    MatTableModule,
    MatTooltipModule,
    EmptyState,
    ListSkeleton,
    PageHeader,
    SearchBox,
  ],
  providers: [{ provide: MatPaginatorIntl, useFactory: crearMatPaginatorIntlEs }],
  templateUrl: './bodegas-list.html',
  styleUrl: './bodegas-list.scss',
})
export class BodegasList {
  private readonly bodegasService = inject(Bodegas);
  private readonly sucursalesService = inject(Sucursales);
  private readonly auth = inject(Auth);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);

  protected readonly TODAS = TODAS;
  protected readonly columnas = ['codigo', 'nombre', 'estado', 'acciones'];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly esAdministrador = computed(
    () => this.auth.usuarioActual()?.rolId === ROL_ADMINISTRADOR,
  );

  /** Sucursales elegibles: las asignadas al usuario, o todas si es Administrador. */
  protected readonly sucursales = signal<Sucursal[]>([]);

  /** Sucursal cuyo listado se muestra; `undefined` = todas (solo Administrador). */
  protected readonly sucursalId = signal<number | undefined>(this.sucursalInicial());

  protected readonly sucursalSeleccionada = computed(() =>
    this.sucursales().find((s) => s.id === this.sucursalId()),
  );

  protected readonly estado = new EstadoListaPaginada<Bodega>((p) =>
    this.bodegasService.buscar(p, { sucursalId: this.sucursalId() }),
  );

  constructor() {
    this.sucursalesService.listar().subscribe((todas) => {
      const asignadas = new Set(this.auth.usuarioActual()?.sucursalesAsignadas ?? []);
      this.sucursales.set(this.esAdministrador() ? todas : todas.filter((s) => asignadas.has(s.id)));
    });
    this.estado.cargar();
  }

  private sucursalInicial(): number | undefined {
    const deLaUrl = Number(this.route.snapshot.queryParamMap.get('sucursalId'));
    return deLaUrl > 0 ? deLaUrl : (this.auth.sucursalActualId() ?? undefined);
  }

  protected nombreSucursal(id: number): string {
    return this.sucursales().find((s) => s.id === id)?.nombre ?? `Sucursal ${id}`;
  }

  protected cambiarSucursal(id: number): void {
    this.sucursalId.set(id === TODAS ? undefined : id);
    this.estado.pagina.set(1);
    this.estado.cargar();
  }

  protected nueva(): void {
    const sucursalId = this.sucursalId();
    if (sucursalId === undefined) {
      return;
    }
    this.dialog
      .open(BodegaForm, {
        data: { sucursalId, sucursalNombre: this.sucursalSeleccionada()?.nombre },
      })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Bodega creada.');
          this.estado.refrescar();
        }
      });
  }

  protected editar(bodega: Bodega): void {
    this.dialog
      .open(BodegaForm, {
        data: {
          sucursalId: bodega.sucursalId,
          sucursalNombre: this.nombreSucursal(bodega.sucursalId),
          bodega,
        },
      })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Bodega actualizada.');
          this.estado.refrescar();
        }
      });
  }

  protected marcarPrincipal(bodega: Bodega): void {
    this.bodegasService.marcarPrincipal(bodega.publicId).subscribe({
      next: () => {
        this.toast.exito(`"${bodega.nombre}" es ahora la bodega principal.`);
        this.estado.refrescar();
      },
    });
  }

  protected alternarActiva(bodega: Bodega): void {
    if (!bodega.activa) {
      this.bodegasService.activar(bodega.publicId).subscribe({
        next: () => {
          this.toast.exito('Bodega activada.');
          this.estado.refrescar();
        },
      });
      return;
    }

    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Desactivar bodega',
          mensaje: `¿Desactivar la bodega "${bodega.nombre}"? Solo es posible si no tiene stock.`,
          textoConfirmar: 'Desactivar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.bodegasService.desactivar(bodega.publicId).subscribe({
            next: () => {
              this.toast.exito('Bodega desactivada.');
              this.estado.refrescar();
            },
          });
        }
      });
  }

  protected eliminar(bodega: Bodega): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Eliminar bodega',
          mensaje: `¿Eliminar la bodega "${bodega.nombre}"? Solo es posible si no es la principal y no tiene stock.`,
          textoConfirmar: 'Eliminar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.bodegasService.eliminar(bodega.publicId).subscribe({
            next: () => {
              this.toast.exito('Bodega eliminada.');
              this.estado.refrescar();
            },
          });
        }
      });
  }
}
