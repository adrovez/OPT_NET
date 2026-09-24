import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
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
import { RecetaGraduacion } from '../../../receta-cristales/components/receta-graduacion/receta-graduacion';
import { EstadoOtChip } from '../../../ordenes-de-trabajo/components/estado-ot-chip/estado-ot-chip';
import { ESTADOS_OT, EstadoOT } from '../../../ordenes-de-trabajo/models/catalogos-comercial.model';
import { OrdenDeTrabajoResumen } from '../../../ordenes-de-trabajo/models/orden-de-trabajo.model';
import { CatalogosComercial } from '../../../ordenes-de-trabajo/services/catalogos-comercial';
import { OrdenesDeTrabajo } from '../../../ordenes-de-trabajo/services/ordenes-de-trabajo';
import { AsociarOrdenDialog } from '../../components/asociar-orden-dialog/asociar-orden-dialog';
import { EstadoOperativoChip } from '../../components/estado-operativo-chip/estado-operativo-chip';
import { GastoOperativoDialog } from '../../components/gasto-operativo-dialog/gasto-operativo-dialog';
import {
  ESTADOS_OPERATIVO,
  EstadoOperativo,
  GastoOperativo,
  Operativo,
  ReporteCristalesItem,
} from '../../models/operativo.model';
import { descargarExcelCristales } from '../../utils/reporte-cristales-excel';
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
    MatCheckboxModule,
    DatePipe,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatMenuModule,
    MatSelectModule,
    MatTableModule,
    MatTabsModule,
    MatTooltipModule,
    EmptyState,
    EstadoOperativoChip,
    EstadoOtChip,
    ListSkeleton,
    PageHeader,
    PesosPipe,
    RecetaGraduacion,
  ],
  templateUrl: './operativo-ficha.html',
  styleUrl: './operativo-ficha.scss',
})
export class OperativoFicha implements OnInit {
  readonly publicId = input.required<string>();

  private readonly operativosService = inject(Operativos);
  private readonly catalogoEstados = inject(EstadosOperativo);
  private readonly ordenesService = inject(OrdenesDeTrabajo);
  private readonly catalogosComercial = inject(CatalogosComercial);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);

  protected readonly operativo = signal<Operativo | null>(null);
  protected readonly cargando = signal(true);
  protected readonly error = signal(false);
  protected readonly procesando = signal(false);
  protected readonly estados = signal<EstadoOperativo[]>([]);
  protected readonly estadosOT = signal<EstadoOT[]>([]);

  /** HU-OP-03: filtro por estado dentro de la pestaña Recepción — null = sin filtrar. */
  protected readonly filtroEstadoOT = signal<number | null>(null);

  protected readonly columnasOrdenes = [
    'numeroOT',
    'cliente',
    'estadoOT',
    'fechaAtencion',
    'vendido',
    'pagado',
    'acciones',
  ];
  protected readonly columnasGastos = ['fecha', 'documento', 'observacion', 'monto', 'acciones'];

  protected readonly ordenesFiltradas = computed(() => {
    const filtro = this.filtroEstadoOT();
    const ordenes = this.operativo()?.ordenes ?? [];
    return filtro === null ? ordenes : ordenes.filter((o) => o.estadoOTId === filtro);
  });

  // ── Reporte de Cristales (HU-OP-10) ───────────────────────────────────────────
  protected readonly reporteCristales = signal<ReporteCristalesItem[] | null>(null);
  protected readonly cargandoReporte = signal(false);
  protected readonly filtroEstadoReporte = signal<number | null>(null);
  /** OT marcadas para exportar; al cargar el reporte quedan todas marcadas. */
  protected readonly seleccionReporte = signal<Set<string>>(new Set());
  protected readonly seleccionadasReporte = computed(() =>
    this.reporteFiltrado().filter((i) => this.seleccionReporte().has(i.ordenPublicId)),
  );
  protected readonly todasSeleccionadas = computed(
    () =>
      this.reporteFiltrado().length > 0 &&
      this.seleccionadasReporte().length === this.reporteFiltrado().length,
  );

  protected readonly reporteFiltrado = computed(() => {
    const filtro = this.filtroEstadoReporte();
    const items = this.reporteCristales() ?? [];
    return filtro === null ? items : items.filter((i) => i.estadoOTId === filtro);
  });

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

  /**
   * HU-OP-09: en Prospecto, el botón se llama "Iniciar recepción" (el criterio de aceptación
   * pide esa acción explícita) — para cualquier otra etapa sigue diciendo "Avanzar a X".
   */
  protected readonly textoAvanzar = computed(() => {
    const operativo = this.operativo();
    const siguiente = this.estadoSiguiente();
    if (!siguiente) {
      return '';
    }
    return operativo?.estadoOperativoId === ESTADOS_OPERATIVO.prospecto
      ? 'Iniciar recepción'
      : `Avanzar a ${siguiente.nombre}`;
  });

  constructor() {
    this.catalogoEstados.listar().subscribe((estados) => this.estados.set(estados));
    this.catalogosComercial.listarEstadosOT().subscribe((estados) => this.estadosOT.set(estados));
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

  /** Una OT Entregada o Anulada no admite "Avanzar etapa" — ídem que en su propia ficha. */
  protected ordenEsTerminal(estadoOTId: number): boolean {
    return this.estadosOT().find((e) => e.id === estadoOTId)?.esTerminal ?? true;
  }

  protected ordenEstaAnulada(estadoOTId: number): boolean {
    return estadoOTId === ESTADOS_OT.anulado;
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
            this.operativosService.cambiarEstado(operativo.publicId, {
              nuevoEstadoId: siguiente.id,
            }),
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
            this.operativosService.asociarOrden(operativo.publicId, {
              ordenPublicId: orden.publicId,
            }),
            `OT N° ${orden.numeroOT} asociada al Operativo.`,
          );
        }
      });
  }

  /** HU-OP-04: abre el mismo asistente de alta de OT, con Empresa y Sucursal preasignadas. */
  protected nuevaOrden(): void {
    const operativo = this.operativo();
    if (!operativo || this.esTerminal()) {
      return;
    }
    this.router.navigate(['/ordenes-de-trabajo/nueva'], {
      queryParams: {
        operativoPublicId: operativo.publicId,
        empresaPublicId: operativo.empresaPublicId,
        sucursalId: operativo.sucursalId,
      },
    });
  }

  /** HU-OP-08: avanza UNA etapa de la OT — misma acción que ofrece la ficha de la OT. */
  protected avanzarOrden(orden: Operativo['ordenes'][number]): void {
    if (this.procesando()) {
      return;
    }
    const siguienteId = orden.estadoOTId + 1;
    const siguiente = this.estadosOT().find((e) => e.id === siguienteId);
    if (!siguiente) {
      return;
    }

    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: `Avanzar a ${siguiente.nombre}`,
          mensaje: `La OT N° ${orden.numeroOT} pasará de ${orden.estadoOT} a ${siguiente.nombre}.`,
          textoConfirmar: 'Avanzar',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.ejecutarSobreOrden(
            this.ordenesService.cambiarEstado(orden.ordenPublicId, { nuevoEstadoId: siguienteId }),
            `OT N° ${orden.numeroOT} avanzada a ${siguiente.nombre}.`,
          );
        }
      });
  }

  /** HU-OP-07: anula la OT sin salir de Recepción — mismo control de rol que en su propia ficha. */
  protected anularOrden(orden: Operativo['ordenes'][number]): void {
    if (this.procesando()) {
      return;
    }

    this.dialog
      .open(MotivoDialog, {
        data: {
          titulo: `Anular la OT N° ${orden.numeroOT}`,
          mensaje:
            'La orden no se elimina: queda como ANULADA, visible en este Operativo para trazabilidad.',
          etiqueta: 'Motivo de la anulación',
          textoConfirmar: 'Anular orden',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((motivo?: string) => {
        if (motivo) {
          this.ejecutarSobreOrden(
            this.ordenesService.anular(orden.ordenPublicId, { motivo }),
            `OT N° ${orden.numeroOT} anulada.`,
          );
        }
      });
  }

  /**
   * A diferencia de `ejecutar` (que opera el agregado Operativo y ya recibe la respuesta
   * completa), esto actúa sobre la OT — hay que releer el Operativo para reflejar su nuevo
   * estado en la tabla de Recepción, sin pasar por el skeleton de carga inicial.
   */
  private ejecutarSobreOrden(
    peticion: ReturnType<OrdenesDeTrabajo['anular']>,
    mensaje: string,
  ): void {
    const operativo = this.operativo();
    if (!operativo) {
      return;
    }
    this.procesando.set(true);
    peticion.subscribe({
      next: () => {
        this.toast.exito(mensaje);
        this.operativosService.obtener(operativo.publicId).subscribe({
          next: (actualizado) => {
            this.operativo.set(actualizado);
            this.procesando.set(false);
          },
          error: () => this.procesando.set(false),
        });
      },
      error: () => this.procesando.set(false),
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

  // ── Reporte de Cristales (HU-OP-10) ───────────────────────────────────────────

  /** Se pide bajo demanda al entrar a la pestaña — no en `cargar()`, para no pagar el costo
   * en las otras pestañas, que son las que se usan más seguido. */
  protected alCambiarPestania(indice: number): void {
    // 0 Recepción · 1 Reporte de Cristales · 2 Gastos
    if (indice === 1) {
      this.cargarReporteCristales();
    }
  }

  protected cargarReporteCristales(): void {
    if (this.reporteCristales() !== null || this.cargandoReporte()) {
      return;
    }
    this.cargandoReporte.set(true);
    this.operativosService.reporteCristales(this.publicId()).subscribe({
      next: (items) => {
        this.reporteCristales.set(items);
        this.seleccionReporte.set(new Set(items.map((i) => i.ordenPublicId)));
        this.cargandoReporte.set(false);
      },
      error: () => this.cargandoReporte.set(false),
    });
  }

  /** Exporta a .xlsx solo las OT seleccionadas (sin Empresa/Sucursal: va al proveedor). */
  protected async exportarReporteExcel(): Promise<void> {
    const operativo = this.operativo();
    if (!operativo || this.seleccionadasReporte().length === 0) {
      return;
    }
    await descargarExcelCristales(
      this.seleccionadasReporte(),
      `ListaCristales_Operativo${operativo.correlativo}.xlsx`,
    );
  }

  protected estaSeleccionada(item: ReporteCristalesItem): boolean {
    return this.seleccionReporte().has(item.ordenPublicId);
  }

  protected alternarSeleccion(item: ReporteCristalesItem): void {
    this.seleccionReporte.update((s) => {
      const nueva = new Set(s);
      if (!nueva.delete(item.ordenPublicId)) {
        nueva.add(item.ordenPublicId);
      }
      return nueva;
    });
  }

  protected alternarTodas(marcar: boolean): void {
    this.seleccionReporte.set(
      marcar ? new Set(this.reporteFiltrado().map((i) => i.ordenPublicId)) : new Set(),
    );
  }
}
