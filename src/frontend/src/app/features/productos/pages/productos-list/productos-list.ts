import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
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
import { Toast } from '../../../../shared/services/toast';
import { EstadoListaPaginada } from '../../../../shared/utils/estado-lista-paginada';
import { Producto } from '../../../inventario/models/producto.model';
import { Productos } from '../../../inventario/services/productos';
import { ProductoForm } from '../producto-form/producto-form';

/** Lista del catálogo de productos con alta, edición y baja lógica. */
@Component({
  selector: 'app-productos-list',
  imports: [
    MatButtonModule,
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
  templateUrl: './productos-list.html',
  styleUrl: './productos-list.scss',
})
export class ProductosList {
  private readonly productosService = inject(Productos);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);

  protected readonly columnas = ['codigo', 'descripcion', 'controlStock', 'acciones'];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly estado = new EstadoListaPaginada<Producto>((p) =>
    this.productosService.buscar(p),
  );

  constructor() {
    this.estado.cargar();
  }

  protected nuevo(): void {
    this.dialog
      .open(ProductoForm, { data: {} })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Producto creado.');
          this.estado.refrescar();
        }
      });
  }

  protected editar(producto: Producto): void {
    this.dialog
      .open(ProductoForm, { data: { producto } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Producto actualizado.');
          this.estado.refrescar();
        }
      });
  }

  protected darDeBaja(producto: Producto): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Dar de baja producto',
          mensaje: `¿Dar de baja el producto "${producto.codigo} — ${producto.descripcion}"?`,
          textoConfirmar: 'Dar de baja',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.productosService.darDeBaja(producto.id).subscribe({
            next: () => {
              this.toast.exito('Producto dado de baja.');
              this.estado.refrescar();
            },
          });
        }
      });
  }
}
