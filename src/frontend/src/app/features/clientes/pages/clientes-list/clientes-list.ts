import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
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
import { Cliente } from '../../models/cliente.model';
import { Clientes } from '../../services/clientes';
import { ClienteForm } from '../cliente-form/cliente-form';

@Component({
  selector: 'app-clientes-list',
  imports: [
    MatButtonModule,
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
  templateUrl: './clientes-list.html',
  styleUrl: './clientes-list.scss',
})
export class ClientesList {
  private readonly clientesService = inject(Clientes);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);

  protected readonly columnas = ['rut', 'nombre', 'apellido', 'email', 'telefono', 'acciones'];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly estado = new EstadoListaPaginada<Cliente>((p) =>
    this.clientesService.buscar(p),
  );

  constructor() {
    this.estado.cargar();
  }

  protected nuevo(): void {
    this.dialog
      .open(ClienteForm, { data: {} })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Cliente creado.');
          this.estado.refrescar();
        }
      });
  }

  protected editar(cliente: Cliente): void {
    this.dialog
      .open(ClienteForm, { data: { cliente } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Cliente actualizado.');
          this.estado.refrescar();
        }
      });
  }

  protected verFicha(cliente: Cliente): void {
    this.router.navigate(['/clientes', cliente.publicId]);
  }

  protected eliminar(cliente: Cliente): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Eliminar cliente',
          mensaje: `¿Eliminar al cliente "${cliente.nombre} ${cliente.apellido}"?`,
          textoConfirmar: 'Eliminar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.clientesService.eliminar(cliente.publicId).subscribe({
            next: () => {
              this.toast.exito('Cliente eliminado.');
              this.estado.refrescar();
            },
          });
        }
      });
  }
}
