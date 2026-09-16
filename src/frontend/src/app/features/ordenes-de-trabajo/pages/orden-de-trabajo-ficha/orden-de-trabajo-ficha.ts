import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { MotivoDialog } from '../../../../shared/components/motivo-dialog/motivo-dialog';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { Toast } from '../../../../shared/services/toast';
import { RecetaGraduacion } from '../../../receta-cristales/components/receta-graduacion/receta-graduacion';
import { EstadoOtChip } from '../../components/estado-ot-chip/estado-ot-chip';
import { ImprimirTicketDialog } from '../../components/imprimir-ticket-dialog/imprimir-ticket-dialog';
import { ResumenFinanciero } from '../../components/resumen-financiero/resumen-financiero';
import { ESTADOS_OT, EstadoOT } from '../../models/catalogos-comercial.model';
import { OrdenDeTrabajo } from '../../models/orden-de-trabajo.model';
import { CatalogosComercial } from '../../services/catalogos-comercial';
import { OrdenesDeTrabajo } from '../../services/ordenes-de-trabajo';

/**
 * Ficha de la Orden de Trabajo: cabecera de la orden arriba (N° OT, fecha, fecha de entrega,
 * beneficiario, empresa) y pestañas debajo, replicando el modal "Detalle Orden de Trabajo" del
 * legacy — que es la pantalla que el negocio pidió conservar. Las cuatro primeras pestañas son
 * las suyas (Cliente / Receta / Detalle / Abonos); Pagos, Cuotas y Bitácora son propias del
 * sistema nuevo. Sigue siendo una página ruteada, no un modal: es también la pantalla desde la
 * que se opera la orden.
 *
 * Es también la pantalla del **flujo de estados**: reemplaza al modal "Enviar OT a flujo" y
 * al modal de bitácora del legacy (Areas/Flujo). El legacy dejaba elegir cualquier estado de
 * un `<select>`; acá solo se ofrece avanzar o retroceder **una** etapa, que es lo único que
 * el dominio acepta (`OrdenDeTrabajo.CambiarEstado`) — y al retroceder se exige observación.
 *
 * Registrar abonos, pagos y cuotas tiene pantalla propia (módulos Abonos/Pagos/Cuotas):
 * desde acá se navega a ellas con la OT ya seleccionada.
 */
@Component({
  selector: 'app-orden-de-trabajo-ficha',
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatTabsModule,
    MatTooltipModule,
    EmptyState,
    EstadoOtChip,
    ListSkeleton,
    PageHeader,
    PesosPipe,
    RecetaGraduacion,
    ResumenFinanciero,
  ],
  templateUrl: './orden-de-trabajo-ficha.html',
  styleUrl: './orden-de-trabajo-ficha.scss',
})
export class OrdenDeTrabajoFicha implements OnInit {
  readonly publicId = input.required<string>();

  private readonly ordenesService = inject(OrdenesDeTrabajo);
  private readonly catalogos = inject(CatalogosComercial);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);

  protected readonly orden = signal<OrdenDeTrabajo | null>(null);
  protected readonly cargando = signal(true);
  protected readonly error = signal(false);
  protected readonly procesando = signal(false);
  protected readonly estadosOT = signal<EstadoOT[]>([]);

  protected readonly columnasDetalle = [
    'producto',
    'cantidad',
    'valorUnitario',
    'total',
    'comentario',
  ];
  protected readonly columnasAbonos = ['fecha', 'formaPago', 'referencia', 'monto'];
  protected readonly columnasPagos = ['fecha', 'formaPago', 'referencia', 'monto'];
  protected readonly columnasCuotas = ['numero', 'vencimiento', 'valor', 'estado', 'pago'];

  /** Etapas del proceso, sin ANULADO: es la interrupción del flujo, no un paso más. */
  protected readonly etapas = computed(() =>
    this.estadosOT().filter((estado) => estado.id !== ESTADOS_OT.anulado),
  );

  protected readonly esTerminal = computed(() => {
    const estadoId = this.orden()?.estadoOTId;
    if (estadoId === undefined) {
      return true;
    }
    return this.estadosOT().find((e) => e.id === estadoId)?.esTerminal ?? false;
  });

  protected readonly estadoSiguiente = computed(() => {
    const orden = this.orden();
    if (!orden || this.esTerminal()) {
      return null;
    }
    return this.etapas().find((estado) => estado.id === orden.estadoOTId + 1) ?? null;
  });

  protected readonly estadoAnterior = computed(() => {
    const orden = this.orden();
    if (!orden || this.esTerminal()) {
      return null;
    }
    return this.etapas().find((estado) => estado.id === orden.estadoOTId - 1) ?? null;
  });

  protected readonly estaAnulada = computed(() => this.orden()?.estadoOTId === ESTADOS_OT.anulado);

  protected readonly sePuedeAnular = computed(() => {
    const estadoId = this.orden()?.estadoOTId;
    return (
      estadoId !== undefined && estadoId !== ESTADOS_OT.anulado && estadoId !== ESTADOS_OT.entregado
    );
  });

  constructor() {
    this.catalogos.listarEstadosOT().subscribe((estados) => this.estadosOT.set(estados));
  }

  ngOnInit(): void {
    this.cargar();
  }

  protected cargar(): void {
    this.cargando.set(true);
    this.error.set(false);
    this.ordenesService.obtener(this.publicId()).subscribe({
      next: (orden) => {
        this.orden.set(orden);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set(true);
        this.cargando.set(false);
      },
    });
  }

  // ── Flujo de estados ───────────────────────────────────────────────────────

  /** Etapa ya recorrida o en curso: se pinta llena en la barra de progreso. */
  protected etapaAlcanzada(etapaId: number): boolean {
    const actual = this.orden()?.estadoOTId;
    return actual !== undefined && !this.estaAnulada() && etapaId <= actual;
  }

  protected avanzar(): void {
    const orden = this.orden();
    const siguiente = this.estadoSiguiente();
    if (!orden || !siguiente || this.procesando()) {
      return;
    }

    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: `Avanzar a ${siguiente.nombre}`,
          mensaje: `La OT N° ${orden.numeroOT} pasará de ${orden.estadoOT} a ${siguiente.nombre}. Queda registrado en la bitácora.`,
          textoConfirmar: 'Avanzar',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.ejecutar(
            this.ordenesService.cambiarEstado(orden.publicId, { nuevoEstadoId: siguiente.id }),
            `OT avanzada a ${siguiente.nombre}.`,
          );
        }
      });
  }

  /** Retroceder exige observación: el dominio devuelve 422 si no viene. */
  protected retroceder(): void {
    const orden = this.orden();
    const anterior = this.estadoAnterior();
    if (!orden || !anterior || this.procesando()) {
      return;
    }

    this.dialog
      .open(MotivoDialog, {
        data: {
          titulo: `Retroceder a ${anterior.nombre}`,
          mensaje: `La OT N° ${orden.numeroOT} volverá de ${orden.estadoOT} a ${anterior.nombre}.`,
          etiqueta: 'Motivo del retroceso',
          textoConfirmar: 'Retroceder',
          maxLength: 100,
        },
      })
      .afterClosed()
      .subscribe((observacion?: string) => {
        if (observacion) {
          this.ejecutar(
            this.ordenesService.cambiarEstado(orden.publicId, {
              nuevoEstadoId: anterior.id,
              observacion,
            }),
            `OT devuelta a ${anterior.nombre}.`,
          );
        }
      });
  }

  protected anular(): void {
    const orden = this.orden();
    if (!orden || this.procesando()) {
      return;
    }

    this.dialog
      .open(MotivoDialog, {
        data: {
          titulo: `Anular la OT N° ${orden.numeroOT}`,
          mensaje:
            'La orden no se elimina: queda como ANULADA, con su detalle y su historial completos.',
          etiqueta: 'Motivo de la anulación',
          textoConfirmar: 'Anular orden',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((motivo?: string) => {
        if (motivo) {
          this.ejecutar(
            this.ordenesService.anular(orden.publicId, { motivo }),
            `OT N° ${orden.numeroOT} anulada.`,
          );
        }
      });
  }

  // ── Navegación a los módulos de dinero ─────────────────────────────────────

  protected editar(): void {
    this.router.navigate(['/ordenes-de-trabajo', this.publicId(), 'editar']);
  }

  /**
   * Reimprime el ticket de una OT ya existente — el legacy lo permitía desde
   * `Imprimir/Ticket/TicketOT/{id}` en cualquier momento, sin importar el estado de la orden;
   * acá tampoco se bloquea si está anulada o entregada, porque reimprimir no modifica nada.
   */
  protected imprimirTicket(): void {
    const orden = this.orden();
    if (!orden) {
      return;
    }
    this.dialog.open(ImprimirTicketDialog, { data: { orden } });
  }

  protected verCliente(): void {
    const orden = this.orden();
    if (orden) {
      this.router.navigate(['/clientes', orden.clientePublicId]);
    }
  }

  protected irA(modulo: 'abonos' | 'pagos' | 'cuotas'): void {
    this.router.navigate([`/${modulo}`], { queryParams: { ot: this.publicId() } });
  }

  /** Todo comando del agregado devuelve la OT completa: se refresca sin volver a leer. */
  private ejecutar(peticion: ReturnType<OrdenesDeTrabajo['anular']>, mensaje: string): void {
    this.procesando.set(true);
    peticion.subscribe({
      next: (orden) => {
        this.orden.set(orden);
        this.procesando.set(false);
        this.toast.exito(mensaje);
      },
      error: () => this.procesando.set(false),
    });
  }
}
