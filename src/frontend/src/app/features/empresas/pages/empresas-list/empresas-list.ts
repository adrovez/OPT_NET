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
import { EstadoListaPaginada } from '../../../../shared/utils/estado-lista-paginada';
import { Toast } from '../../../../shared/services/toast';
import { Empresa } from '../../models/empresa.model';
import { Empresas } from '../../services/empresas';
import { EmpresaForm } from '../empresa-form/empresa-form';

@Component({
  selector: 'app-empresas-list',
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
  templateUrl: './empresas-list.html',
  styleUrl: './empresas-list.scss',
})
export class EmpresasList {
  private readonly empresasService = inject(Empresas);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);

  protected readonly columnas = ['nombre', 'rut', 'razonSocial', 'telefono', 'acciones'];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly estado = new EstadoListaPaginada<Empresa>((p) =>
    this.empresasService.buscar(p),
  );

  constructor() {
    this.estado.cargar();
  }

  protected nueva(): void {
    this.dialog
      .open(EmpresaForm, { data: {} })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Empresa creada.');
          this.estado.refrescar();
        }
      });
  }

  protected editar(empresa: Empresa): void {
    this.dialog
      .open(EmpresaForm, { data: { empresa } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Empresa actualizada.');
          this.estado.refrescar();
        }
      });
  }

  protected eliminar(empresa: Empresa): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Eliminar empresa',
          mensaje: `¿Eliminar la empresa "${empresa.nombre}"?`,
          textoConfirmar: 'Eliminar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.empresasService.eliminar(empresa.publicId).subscribe({
            next: () => {
              this.toast.exito('Empresa eliminada.');
              this.estado.refrescar();
            },
          });
        }
      });
  }
}
