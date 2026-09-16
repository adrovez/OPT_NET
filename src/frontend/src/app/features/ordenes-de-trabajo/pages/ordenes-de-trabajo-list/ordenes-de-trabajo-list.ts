import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
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
import { EstadoOtChip } from '../../components/estado-ot-chip/estado-ot-chip';
import { ESTADOS_OT, EstadoOT } from '../../models/catalogos-comercial.model';
import { OrdenDeTrabajoResumen } from '../../models/orden-de-trabajo.model';
import { CatalogosComercial } from '../../services/catalogos-comercial';
import { OrdenesDeTrabajo } from '../../services/ordenes-de-trabajo';

/**
 * Listado de Órdenes de Trabajo — reemplaza al "Listado OT" del legacy (Ingreso/Index).
 *
 * El legacy filtraba con tres campos separados (N° OT, RUT/Nombre, Empresa); acá hay una
 * sola búsqueda tipo Google (el backend prueba el término contra número de OT, beneficiario
 * y RUT/nombre del cliente) más dos filtros de trabajo: estado y "solo con saldo".
 *
 * Los filtros de contexto (`clientePublicId`, `empresaPublicId`, `operativoPublicId`) llegan
 * por query params: es la vuelta desde la ficha de un cliente, desde Cobranza o desde la
 * ficha de un Operativo ("Ver en el listado de OT").
 *
 * **No carga nada al entrar**, igual que el legacy: son 12.578 órdenes y traer la primera
 * página sin criterio no le sirve a nadie (en el legacy además daba timeout). La consulta se
 * dispara con la primera búsqueda o filtro — salvo que se llegue con un filtro de contexto en
 * la URL, que ya *es* un criterio.
 */
@Component({
  selector: 'app-ordenes-de-trabajo-list',
  imports: [
    DatePipe,
    FormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatMenuModule,
    MatPaginatorModule,
    MatSelectModule,
    MatSortModule,
    MatTableModule,
    MatTooltipModule,
    EmptyState,
    EstadoOtChip,
    ListSkeleton,
    PageHeader,
    PesosPipe,
    SearchBox,
  ],
  providers: [{ provide: MatPaginatorIntl, useFactory: crearMatPaginatorIntlEs }],
  templateUrl: './ordenes-de-trabajo-list.html',
  styleUrl: './ordenes-de-trabajo-list.scss',
})
export class OrdenesDeTrabajoList {
  private readonly ordenesService = inject(OrdenesDeTrabajo);
  private readonly catalogos = inject(CatalogosComercial);
  private readonly auth = inject(Auth);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly columnas = [
    'numeroOT',
    'fechaEntrega',
    'cliente',
    'sucursal',
    'estado',
    'precio',
    'saldo',
    'acciones',
  ];
  protected readonly opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA;

  protected readonly estadosOT = signal<EstadoOT[]>([]);
  protected readonly filtroEstadoId = signal<number | null>(null);
  protected readonly filtroSoloConSaldo = signal(false);

  /** Filtros de contexto que llegan por query param (ficha de cliente / Cobranza / Operativo). */
  protected readonly clientePublicId = signal<string | null>(null);
  protected readonly empresaPublicId = signal<string | null>(null);
  protected readonly operativoPublicId = signal<string | null>(null);

  /**
   * `false` hasta que el usuario busca o filtra: distingue "todavía no consultaste" de
   * "consultaste y no hubo resultados", que es lo que decide qué estado vacío se muestra.
   */
  protected readonly consultado = signal(false);

  protected readonly hayContexto = computed(
    () =>
      this.clientePublicId() !== null ||
      this.empresaPublicId() !== null ||
      this.operativoPublicId() !== null,
  );

  protected readonly estado = new EstadoListaPaginada<OrdenDeTrabajoResumen>((p) =>
    this.ordenesService.buscar(p, {
      clientePublicId: this.clientePublicId(),
      empresaPublicId: this.empresaPublicId(),
      operativoPublicId: this.operativoPublicId(),
      // Sucursal actual del menú (`Shell`) — el listado nunca muestra OT de otra sucursal
      // que la elegida, igual que el dashboard del legacy.
      sucursalId: this.auth.sucursalActualId(),
      estadoOTId: this.filtroEstadoId(),
      soloConSaldo: this.filtroSoloConSaldo(),
    }),
  );

  /** Última sucursal con la que se consultó — para distinguir un cambio real del valor inicial. */
  private sucursalConsultada: number | null = null;

  constructor() {
    const params = this.route.snapshot.queryParamMap;
    this.clientePublicId.set(params.get('clientePublicId'));
    this.empresaPublicId.set(params.get('empresaPublicId'));
    this.operativoPublicId.set(params.get('operativoPublicId'));
    this.filtroSoloConSaldo.set(params.get('soloConSaldo') === 'true');

    this.catalogos.listarEstadosOT().subscribe((estados) => this.estadosOT.set(estados));

    // Llegar con un contexto (cliente, empresa, solo con saldo) ya es una consulta acotada:
    // ahí sí se carga sola. Sin contexto se espera a que el usuario busque.
    if (this.hayContexto() || this.filtroSoloConSaldo()) {
      this.consultar();
    }

    // Cambiar de sucursal en el menú mientras se está viendo el listado repite la consulta
    // con la nueva sucursal — nunca se deja a la vista una página con datos de la sucursal
    // anterior. No dispara nada si todavía no se había consultado (respeta el "no carga nada
    // al entrar" de esta pantalla).
    this.sucursalConsultada = this.auth.sucursalActualId();
    effect(() => {
      const actual = this.auth.sucursalActualId();
      if (actual !== this.sucursalConsultada) {
        this.sucursalConsultada = actual;
        if (this.consultado()) {
          this.aplicarFiltros();
        }
      }
    });
  }

  /** Punto único de entrada a la consulta: deja registrado que ya se consultó. */
  private consultar(): void {
    this.consultado.set(true);
    this.estado.cargar();
  }

  protected buscar(texto: string): void {
    this.consultado.set(true);
    this.estado.buscar(texto);
  }

  /** Un filtro nuevo siempre vuelve a la primera página: la actual puede no existir. */
  protected aplicarFiltros(): void {
    this.estado.pagina.set(1);
    this.consultar();
  }

  protected limpiarContexto(): void {
    this.router.navigate(['/ordenes-de-trabajo']).then(() => {
      this.clientePublicId.set(null);
      this.empresaPublicId.set(null);
      this.operativoPublicId.set(null);
      // Se vuelve al estado inicial (sin resultados) en vez de traer las 12.578 órdenes:
      // quitar el contexto deja la pantalla sin criterio, y sin criterio no se consulta.
      this.estado.pagina.set(1);
      this.estado.items.set([]);
      this.estado.total.set(0);
      this.consultado.set(false);
    });
  }

  protected nueva(): void {
    this.router.navigate(['/ordenes-de-trabajo/nueva']);
  }

  protected verFicha(orden: OrdenDeTrabajoResumen): void {
    this.router.navigate(['/ordenes-de-trabajo', orden.publicId]);
  }

  protected editar(orden: OrdenDeTrabajoResumen): void {
    this.router.navigate(['/ordenes-de-trabajo', orden.publicId, 'editar']);
  }

  protected verCliente(orden: OrdenDeTrabajoResumen): void {
    this.router.navigate(['/clientes', orden.clientePublicId]);
  }

  /** Una OT anulada no admite ninguna modificación (regla del dominio, no de la pantalla). */
  protected esModificable(orden: OrdenDeTrabajoResumen): boolean {
    return orden.estadoOTId !== ESTADOS_OT.anulado;
  }

  protected sePuedeAnular(orden: OrdenDeTrabajoResumen): boolean {
    return orden.estadoOTId !== ESTADOS_OT.anulado && orden.estadoOTId !== ESTADOS_OT.entregado;
  }

  /**
   * Anula la OT (no la borra: el legacy la eliminaba con SP_OTEliminar). El motivo es
   * obligatorio y queda en la bitácora, por eso se pide con `MotivoDialog` y no con
   * un confirm a secas.
   */
  protected anular(orden: OrdenDeTrabajoResumen): void {
    this.dialog
      .open(MotivoDialog, {
        data: {
          titulo: `Anular la OT N° ${orden.numeroOT}`,
          mensaje:
            'La orden no se elimina: queda en el listado como ANULADA, con su historial completo.',
          etiqueta: 'Motivo de la anulación',
          textoConfirmar: 'Anular orden',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((motivo?: string) => {
        if (motivo) {
          this.ordenesService.anular(orden.publicId, { motivo }).subscribe({
            next: () => {
              this.toast.exito(`OT N° ${orden.numeroOT} anulada.`);
              this.estado.refrescar();
            },
          });
        }
      });
  }
}
