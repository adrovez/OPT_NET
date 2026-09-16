import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { MotivoDialog } from '../../../../shared/components/motivo-dialog/motivo-dialog';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { Toast } from '../../../../shared/services/toast';
import { EstadoOtChip } from '../../../ordenes-de-trabajo/components/estado-ot-chip/estado-ot-chip';
import { OrdenDeTrabajoResumen } from '../../../ordenes-de-trabajo/models/orden-de-trabajo.model';
import { AsociarOrdenDialog } from '../../components/asociar-orden-dialog/asociar-orden-dialog';
import { EstadoOperativoChip } from '../../components/estado-operativo-chip/estado-operativo-chip';
import { GastoOperativoDialog } from '../../components/gasto-operativo-dialog/gasto-operativo-dialog';
import {
  ESTADOS_OPERATIVO,
  EstadoOperativo,
  GastoOperativo,
  Operativo,
} from '../../models/operativo.model';
import { EstadosOperativo } from '../../services/estados-operativo';
import { Operativos } from '../../services/operativos';
import { OperativoForm } from '../operativo-form/operativo-form';

/**
 * Ficha del Operativo: cabecera (correlativo, fecha, empresa, sucursal, estado) + resumen
 * financiero (vendido / pagado / gastos / ganancia) arriba, y pestañas debajo — Órdenes
 * asociadas y Gastos, que son los dos historiales de subrecurso que el requerimiento pide
 * llevar a la vez (mismo criterio de "ficha ruteada" que `cliente-ficha`/`orden-de-trabajo-ficha`).
 *
 * También es la pantalla del flujo de estados: Prospecto→Ingresado→Cobranza→Cerrado avanza
 * de a una etapa, sin retroceso (`EstadosOperativo` del dominio no lo permite); Anular solo es
 * posible desde Prospecto o Ingresado.
 */
@Component({
  selector: 'app-operativo-ficha',
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatTabsModule,
    MatTooltipModule,
    EmptyState,
    EstadoOperativoChip,
    EstadoOtChip,
    ListSkeleton,
    PageHeader,
    PesosPipe,
  ],
  templateUrl: './operativo-ficha.html',
  styleUrl: './operativo-ficha.scss',
})
export class OperativoFicha implements OnInit {
  readonly publicId = input.required<string>();

  private readonly operativosService = inject(Operativos);
  private readonly catalogoEstados = inject(EstadosOperativo);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);

  protected readonly operativo = signal<Operativo | null>(null);
  protected readonly cargando = signal(true);
  protected readonly error = signal(false);
  protected readonly procesando = signal(false);
  protected readonly estados = signal<EstadoOperativo[]>([]);

  protected readonly columnasOrdenes = [
    'numeroOT',
    'cliente',
    'estadoOT',
    'vendido',
    'pagado',
    'acciones',
  ];
  protected readonly columnasGastos = ['fecha', 'documento', 'observacion', 'monto', 'acciones'];

  /** Etapas del flujo, sin ANULADO: es la interrupción del flujo, no un paso más. */
  protected readonly etapas = computed(() =>
    this.estados().filter((estado) => estado.id !== ESTADOS_OPERATIVO.anulado),
  );

  protected readonly estadoInfo = computed(() => {
    const estadoId = this.operativo()?.estadoOperativoId;
    return this.estados().find((e) => e.id === estadoId) ?? null;
  });

  protected readonly esTerminal = computed(() => this.estadoInfo()?.esTerminal ?? true);

  protected readonly sePuedeAnular = computed(() => this.estadoInfo()?.puedeAnularse ?? false);

  protected readonly estaAnulada = computed(
    () => this.operativo()?.estadoOperativoId === ESTADOS_OPERATIVO.anulado,
  );

  protected readonly estadoSiguiente = computed(() => {
    const operativo = this.operativo();
    if (!operativo || this.esTerminal()) {
      return null;
    }
    return this.etapas().find((estado) => estado.id === operativo.estadoOperativoId + 1) ?? null;
  });

  constructor() {
    this.catalogoEstados.listar().subscribe((estados) => this.estados.set(estados));
  }

  ngOnInit(): void {
    this.cargar();
  }

  protected cargar(): void {
    this.cargando.set(true);
    this.error.set(false);
    this.operativosService.obtener(this.publicId()).subscribe({
      next: (operativo) => {
        this.operativo.set(operativo);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set(true);
        this.cargando.set(false);
      },
    });
  }

  /** Etapa ya recorrida o en curso: se pinta llena en la barra de progreso. */
  protected etapaAlcanzada(etapaId: number): boolean {
    const actual = this.operativo()?.estadoOperativoId;
    return actual !== undefined && !this.estaAnulada() && etapaId <= actual;
  }

  // ── Flujo de estados ───────────────────────────────────────────────────────

  protected avanzar(): void {
    const operativo = this.operativo();
    const siguiente = this.estadoSiguiente();
    if (!operativo || !siguiente || this.procesando()) {
      return;
    }

    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: `Avanzar a ${siguiente.nombre}`,
          mensaje: `El Operativo N° ${operativo.correlativo} pasará de ${operativo.estadoOperativo} a ${siguiente.nombre}.`,
          textoConfirmar: 'Avanzar',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.ejecutar(
            this.operativosService.cambiarEstado(operativo.publicId, { nuevoEstadoId: siguiente.id }),
            `Operativo avanzado a ${siguiente.nombre}.`,
          );
        }
      });
  }

  protected anular(): void {
    const operativo = this.operativo();
    if (!operativo || this.procesando()) {
      return;
    }

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
          this.ejecutar(
            this.operativosService.anular(operativo.publicId, { motivo }),
            `Operativo N° ${operativo.correlativo} anulado.`,
          );
        }
      });
  }

  protected editar(): void {
    const operativo = this.operativo();
    if (!operativo) {
      return;
    }

    this.dialog
      .open(OperativoForm, { data: { operativo } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.operativo.set(resultado);
          this.toast.exito('Operativo actualizado.');
        }
      });
  }

  protected recalcularMontos(): void {
    const operativo = this.operativo();
    if (!operativo || this.procesando()) {
      return;
    }
    this.ejecutar(
      this.operativosService.recalcularMontos(operativo.publicId),
      'Montos recalculados desde las órdenes asociadas.',
    );
  }

  // ── Órdenes asociadas ────────────────────────────────────────────────────────

  protected asociarOrden(): void {
    const operativo = this.operativo();
    if (!operativo || this.procesando()) {
      return;
    }

    this.dialog
      .open(AsociarOrdenDialog)
      .afterClosed()
      .subscribe((orden?: OrdenDeTrabajoResumen) => {
        if (orden) {
          this.ejecutar(
            this.operativosService.asociarOrden(operativo.publicId, { ordenPublicId: orden.publicId }),
            `OT N° ${orden.numeroOT} asociada al Operativo.`,
          );
        }
      });
  }

  protected quitarOrden(ordenPublicId: string, numeroOT: number): void {
    const operativo = this.operativo();
    if (!operativo || this.procesando()) {
      return;
    }

    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Quitar orden del Operativo',
          mensaje: `¿Quitar la OT N° ${numeroOT} de este Operativo? La orden en sí no se modifica.`,
          textoConfirmar: 'Quitar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.ejecutar(
            this.operativosService.quitarOrden(operativo.publicId, ordenPublicId),
            `OT N° ${numeroOT} quitada del Operativo.`,
          );
        }
      });
  }

  protected verOrden(ordenPublicId: string): void {
    this.router.navigate(['/ordenes-de-trabajo', ordenPublicId]);
  }

  protected verTodasLasOrdenes(): void {
    this.router.navigate(['/ordenes-de-trabajo'], {
      queryParams: { operativoPublicId: this.publicId() },
    });
  }

  // ── Gastos ─────────────────────────────────────────────────────────────────

  protected registrarGasto(): void {
    const operativo = this.operativo();
    if (!operativo || this.procesando()) {
      return;
    }

    this.dialog
      .open(GastoOperativoDialog)
      .afterClosed()
      .subscribe((datos) => {
        if (datos) {
          this.ejecutar(
            this.operativosService.registrarGasto(operativo.publicId, datos),
            'Gasto registrado.',
          );
        }
      });
  }

  protected eliminarGasto(gasto: GastoOperativo): void {
    const operativo = this.operativo();
    if (!operativo || this.procesando()) {
      return;
    }

    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Eliminar gasto',
          mensaje: `¿Eliminar el gasto de ${gasto.numeroDocumento ?? 'sin N° de documento'}?`,
          textoConfirmar: 'Eliminar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.ejecutar(
            this.operativosService.eliminarGasto(operativo.publicId, gasto.id),
            'Gasto eliminado.',
          );
        }
      });
  }

  /** Todo comando del agregado devuelve el Operativo completo: se refresca sin volver a leer. */
  private ejecutar(peticion: ReturnType<Operativos['anular']>, mensaje: string): void {
    this.procesando.set(true);
    peticion.subscribe({
      next: (operativo) => {
        this.operativo.set(operativo);
        this.procesando.set(false);
        this.toast.exito(mensaje);
      },
      error: () => this.procesando.set(false),
    });
  }
}
