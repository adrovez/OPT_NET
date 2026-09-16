import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { MotivoDialog } from '../../../../shared/components/motivo-dialog/motivo-dialog';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { SearchBox } from '../../../../shared/components/search-box/search-box';
import { crearMatPaginatorIntlEs } from '../../../../shared/i18n/mat-paginator-intl-es';
import { OPCIONES_TAMANIO_PAGINA } from '../../../../shared/models/parametros-consulta-paginada.model';
import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { Toast } from '../../../../shared/services/toast';
import { EstadoListaPaginada } from '../../../../shared/utils/estado-lista-paginada';
import { EstadoOperativoChip } from '../../components/estado-operativo-chip/estado-operativo-chip';
import { EstadoOperativo, OperativoResumen } from '../../models/operativo.model';
import { EstadosOperativo } from '../../services/estados-operativo';
import { Operativos } from '../../services/operativos';
import { OperativoForm } from '../operativo-form/operativo-form';

/**
 * Listado de Operativos — el catálogo es chico comparado con Cliente/OrdenDeTrabajo
 * (jornadas en terreno, no miles de filas), así que a diferencia del listado de OT sí carga
 * al entrar (mismo criterio que Empresas/Clientes/Sucursales).
 */
@Component({
  selector: 'app-operativos-list',
  imports: [
    DatePipe,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatMenuModule,
    MatPaginatorModule,
    MatSelectModule,
    MatSortModule,
    MatTableModule,
    MatTooltipModule,
    EmptyState,
    EstadoOperativoChip,
    ListSkeleton,
    PageHeader,
    PesosPipe,
    SearchBox,
  ],
  providers: [{ provide: MatPaginatorIntl, useFactory: crearMatPaginatorIntlEs }],
  templateUrl: './operativos-list.html',
  styleUrl: './operativos-list.scss',
})
export class OperativosList {
  private readonly operativosService = inject(Operativos);
  private readonly catalogoEstados = inject(EstadosOperativo);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);

  protected readonly columnas = [
    'correlativo',
    'fecha',
    'empresa',
    'sucursal',
    'estado',
    'vendido',
    'pagado',
    'gastos',
    'ganancia',
    'acciones',
  ];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly estados = signal<EstadoOperativo[]>([]);
  protected readonly filtroEstadoId = signal<number | null>(null);

  protected readonly estado = new EstadoListaPaginada<OperativoResumen>((p) =>
    this.operativosService.buscar(p, { estadoOperativoId: this.filtroEstadoId() }),
  );

  constructor() {
    this.catalogoEstados.listar().subscribe((estados) => this.estados.set(estados));
    this.estado.cargar();
  }

  protected aplicarFiltros(): void {
    this.estado.pagina.set(1);
    this.estado.cargar();
  }

  protected nuevo(): void {
    this.dialog
      .open(OperativoForm, { data: {} })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito(`Operativo N° ${resultado.correlativo} creado.`);
          this.estado.refrescar();
        }
      });
  }

  protected editar(operativo: OperativoResumen): void {
    this.dialog
      .open(OperativoForm, { data: { operativo } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito(`Operativo N° ${operativo.correlativo} actualizado.`);
          this.estado.refrescar();
        }
      });
  }

  protected verFicha(operativo: OperativoResumen): void {
    this.router.navigate(['/operativos', operativo.publicId]);
  }

  protected esModificable(operativo: OperativoResumen): boolean {
    return !(this.estados().find((e) => e.id === operativo.estadoOperativoId)?.esTerminal ?? false);
  }

  protected sePuedeAnular(operativo: OperativoResumen): boolean {
    return this.estados().find((e) => e.id === operativo.estadoOperativoId)?.puedeAnularse ?? false;
  }

  /** No se elimina: queda ANULADO con su historial de gastos y órdenes completo. */
  protected anular(operativo: OperativoResumen): void {
    this.dialog
      .open(MotivoDialog, {
        data: {
          titulo: `Anular el Operativo N° ${operativo.correlativo}`,
          mensaje: 'El Operativo no se elimina: queda como ANULADO, con su historial completo.',
          etiqueta: 'Motivo de la anulación',
          textoConfirmar: 'Anular Operativo',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((motivo?: string) => {
        if (motivo) {
          this.operativosService.anular(operativo.publicId, { motivo }).subscribe({
            next: () => {
              this.toast.exito(`Operativo N° ${operativo.correlativo} anulado.`);
              this.estado.refrescar();
            },
          });
        }
      });
  }
}
