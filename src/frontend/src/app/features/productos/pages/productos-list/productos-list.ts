import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
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
import { CategoriaProducto, Producto } from '../../../inventario/models/producto.model';
import { CategoriasProducto } from '../../../inventario/services/categorias-producto';
import { Productos } from '../../../inventario/services/productos';
import { ProductoForm } from '../producto-form/producto-form';

/** Lista del catálogo de productos con alta, edición y baja lógica. */
@Component({
  selector: 'app-productos-list',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
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
  templateUrl: './productos-list.html',
  styleUrl: './productos-list.scss',
})
export class ProductosList {
  private readonly productosService = inject(Productos);
  private readonly categoriasService = inject(CategoriasProducto);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);

  protected readonly columnas = [
    'codigo',
    'categoria',
    'marca',
    'modelo',
    'color',
    'descripcion',
    'controlStock',
    'acciones',
  ];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly categorias = signal<CategoriaProducto[]>([]);
  protected readonly categoriaFiltro = signal<number | null>(null);
  private readonly nombreCategoria = computed(
    () => new Map(this.categorias().map((c) => [c.id, c.nombre])),
  );

  protected readonly estado = new EstadoListaPaginada<Producto>((p) =>
    this.productosService.buscar(p, this.categoriaFiltro()),
  );

  constructor() {
    this.categoriasService
      .listar()
      .pipe(takeUntilDestroyed())
      .subscribe((categorias) => this.categorias.set(categorias));
    this.estado.cargar();
  }

  protected nombreDe(categoriaId: number): string {
    return this.nombreCategoria().get(categoriaId) ?? '—';
  }

  protected filtrarCategoria(categoriaId: number | null): void {
    this.categoriaFiltro.set(categoriaId);
    this.estado.pagina.set(1);
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
