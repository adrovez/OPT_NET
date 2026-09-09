import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { Toast } from '../../../../shared/services/toast';
import { aFechaIso } from '../../../../shared/utils/fechas.util';
import { EstadoOtChip } from '../../../ordenes-de-trabajo/components/estado-ot-chip/estado-ot-chip';
import { ResumenFinanciero } from '../../../ordenes-de-trabajo/components/resumen-financiero/resumen-financiero';
import { SelectorOrden } from '../../../ordenes-de-trabajo/components/selector-orden/selector-orden';
import {
  ESTADOS_CUOTA,
  ESTADOS_OT,
  FormaPago,
} from '../../../ordenes-de-trabajo/models/catalogos-comercial.model';
import {
  Cuota,
  OrdenDeTrabajo,
  OrdenDeTrabajoResumen,
} from '../../../ordenes-de-trabajo/models/orden-de-trabajo.model';
import { CatalogosComercial } from '../../../ordenes-de-trabajo/services/catalogos-comercial';
import { OrdenesDeTrabajo } from '../../../ordenes-de-trabajo/services/ordenes-de-trabajo';
import { CuotaPagoDialog } from '../cuota-pago-dialog/cuota-pago-dialog';

/**
 * Módulo Cuotas — plan de pago de una OT: generarlo, verlo, marcar una cuota como pagada
 * (regularización) y anular cuotas pendientes.
 *
 * En el legacy el plan se definía solo al crear la OT (N° de cuotas + fecha de inicio en la
 * pestaña de abonos) y nunca se administraba después; sus 34.110 cuotas quedaron todas en
 * PENDIENTE. Acá el plan es administrable y el estado de cada cuota es un catálogo real.
 *
 * Solo puede haber un plan vigente: para rehacerlo hay que anular antes las cuotas pendientes
 * (regla del dominio — el backend devuelve 422 si se intenta generar otro).
 */
@Component({
  selector: 'app-cuotas-plan',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatTableModule,
    MatTooltipModule,
    EmptyState,
    EstadoOtChip,
    ListSkeleton,
    PageHeader,
    PesosPipe,
    ResumenFinanciero,
    SelectorOrden,
  ],
  providers: [provideNativeDateAdapter(), { provide: MAT_DATE_LOCALE, useValue: 'es-CL' }],
  templateUrl: './cuotas-plan.html',
  styleUrl: './cuotas-plan.scss',
})
export class CuotasPlan {
  private readonly fb = inject(FormBuilder);
  private readonly ordenesService = inject(OrdenesDeTrabajo);
  private readonly catalogos = inject(CatalogosComercial);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly orden = signal<OrdenDeTrabajo | null>(null);
  protected readonly cargando = signal(false);
  protected readonly procesando = signal(false);
  protected readonly formasPago = signal<FormaPago[]>([]);

  protected readonly columnas = ['numero', 'vencimiento', 'valor', 'estado', 'pago', 'acciones'];

  protected readonly estaAnulada = computed(() => this.orden()?.estadoOTId === ESTADOS_OT.anulado);

  protected readonly cuotas = computed(() => this.orden()?.cuotas ?? []);

  protected readonly hayPlanVigente = computed(() =>
    this.cuotas().some((cuota) => cuota.estadoCuotaId !== ESTADOS_CUOTA.anulada),
  );

  protected readonly totalPendiente = computed(() =>
    this.cuotas()
      .filter((cuota) => cuota.estadoCuotaId === ESTADOS_CUOTA.pendiente)
      .reduce((suma, cuota) => suma + cuota.valorCuota, 0),
  );

  protected readonly form = this.fb.nonNullable.group({
    numeroCuotas: this.fb.control<number | null>(null, [
      Validators.required,
      Validators.min(1),
      Validators.max(60),
    ]),
    primerVencimiento: this.fb.control<Date | null>(null, Validators.required),
  });

  constructor() {
    this.catalogos.listarFormasPago().subscribe((formas) => this.formasPago.set(formas));

    const publicId = this.route.snapshot.queryParamMap.get('ot');
    if (publicId) {
      this.cargarOrden(publicId);
    }
  }

  protected seleccionar(resumen: OrdenDeTrabajoResumen): void {
    this.cargarOrden(resumen.publicId);
  }

  protected cambiarOrden(): void {
    this.orden.set(null);
    this.form.reset({ numeroCuotas: null, primerVencimiento: null });
    this.router.navigate(['/cuotas']);
  }

  protected verOrden(): void {
    const orden = this.orden();
    if (orden) {
      this.router.navigate(['/ordenes-de-trabajo', orden.publicId]);
    }
  }

  protected esPendiente(cuota: Cuota): boolean {
    return cuota.estadoCuotaId === ESTADOS_CUOTA.pendiente;
  }

  protected generarPlan(): void {
    const orden = this.orden();
    if (!orden || this.procesando()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { numeroCuotas, primerVencimiento } = this.form.getRawValue();
    this.ejecutar(
      this.ordenesService.generarPlanCuotas(orden.publicId, {
        numeroCuotas: numeroCuotas!,
        primerVencimiento: aFechaIso(primerVencimiento!),
      }),
      `Plan de ${numeroCuotas} cuota(s) generado.`,
    );
  }

  protected pagarCuota(cuota: Cuota): void {
    const orden = this.orden();
    if (!orden) {
      return;
    }

    this.dialog
      .open(CuotaPagoDialog, {
        data: { numero: cuota.numero, valorCuota: cuota.valorCuota, formasPago: this.formasPago() },
      })
      .afterClosed()
      .subscribe((datos) => {
        if (datos) {
          this.ejecutar(
            this.ordenesService.pagarCuota(orden.publicId, cuota.numero, datos),
            `Cuota ${cuota.numero} marcada como pagada.`,
          );
        }
      });
  }

  protected anularCuota(cuota: Cuota): void {
    const orden = this.orden();
    if (!orden) {
      return;
    }

    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: `Anular la cuota ${cuota.numero}`,
          mensaje:
            'La cuota queda en estado ANULADA. Es el paso previo para rehacer un plan mal generado; una cuota ya pagada no se puede anular.',
          textoConfirmar: 'Anular cuota',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.ejecutar(
            this.ordenesService.anularCuota(orden.publicId, cuota.numero),
            `Cuota ${cuota.numero} anulada.`,
          );
        }
      });
  }

  /** Todo comando del agregado devuelve la OT completa: se refresca sin volver a leer. */
  private ejecutar(peticion: ReturnType<OrdenesDeTrabajo['anularCuota']>, mensaje: string): void {
    this.procesando.set(true);
    peticion.subscribe({
      next: (actualizada) => {
        this.orden.set(actualizada);
        this.procesando.set(false);
        this.form.reset({ numeroCuotas: null, primerVencimiento: null });
        this.toast.exito(mensaje);
      },
      error: () => this.procesando.set(false),
    });
  }

  private cargarOrden(publicId: string): void {
    this.cargando.set(true);
    this.ordenesService.obtener(publicId).subscribe({
      next: (orden) => {
        this.orden.set(orden);
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
        this.orden.set(null);
      },
    });
  }
}
