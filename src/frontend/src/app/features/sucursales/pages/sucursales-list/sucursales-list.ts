import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
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
import { Sucursal } from '../../models/sucursal.model';
import { Sucursales } from '../../services/sucursales';
import { SucursalForm } from '../sucursal-form/sucursal-form';

@Component({
  selector: 'app-sucursales-list',
  imports: [
    MatButtonModule,
    MatChipsModule,
    MatIconModule,
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
  templateUrl: './sucursales-list.html',
  styleUrl: './sucursales-list.scss',
})
export class SucursalesList {
  private readonly sucursalesService = inject(Sucursales);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);

  protected readonly columnas = ['nombre', 'direccion', 'telefono', 'esMatriz', 'acciones'];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly estado = new EstadoListaPaginada<Sucursal>((p) =>
    this.sucursalesService.buscar(p),
  );

  constructor() {
    this.estado.cargar();
  }

  protected nueva(): void {
    this.dialog
      .open(SucursalForm, { data: {} })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Sucursal creada.');
          this.estado.refrescar();
        }
      });
  }

  protected editar(sucursal: Sucursal): void {
    this.dialog
      .open(SucursalForm, { data: { sucursal } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Sucursal actualizada.');
          this.estado.refrescar();
        }
      });
  }

  protected eliminar(sucursal: Sucursal): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Eliminar sucursal',
          mensaje: `¿Eliminar la sucursal "${sucursal.nombre}"?`,
          textoConfirmar: 'Eliminar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.sucursalesService.eliminar(sucursal.id).subscribe({
            next: () => {
              this.toast.exito('Sucursal eliminada.');
              this.estado.refrescar();
            },
          });
        }
      });
  }
}
