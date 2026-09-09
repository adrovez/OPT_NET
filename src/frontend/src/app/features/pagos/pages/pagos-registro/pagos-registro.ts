import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';

import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { Toast } from '../../../../shared/services/toast';
import { aFechaHoraIso } from '../../../../shared/utils/fechas.util';
import { EstadoOtChip } from '../../../ordenes-de-trabajo/components/estado-ot-chip/estado-ot-chip';
import { ResumenFinanciero } from '../../../ordenes-de-trabajo/components/resumen-financiero/resumen-financiero';
import { SelectorOrden } from '../../../ordenes-de-trabajo/components/selector-orden/selector-orden';
import {
  ESTADOS_CUOTA,
  ESTADOS_OT,
  FormaPago,
} from '../../../ordenes-de-trabajo/models/catalogos-comercial.model';
import {
  OrdenDeTrabajo,
  OrdenDeTrabajoResumen,
} from '../../../ordenes-de-trabajo/models/orden-de-trabajo.model';
import { CatalogosComercial } from '../../../ordenes-de-trabajo/services/catalogos-comercial';
import { OrdenesDeTrabajo } from '../../../ordenes-de-trabajo/services/ordenes-de-trabajo';

/**
 * Módulo Pagos — cobro posterior al abono inicial. Equivale a la pantalla "Pago Orden de
 * Trabajo" del legacy (Areas/OrdenTrabajo/Pago), con una diferencia importante: el backend
 * **imputa el pago a las cuotas pendientes más antiguas** que alcance a cubrir completas
 * (ADR 0006). El legacy no lo hacía y dejó sus 34.110 cuotas en PENDIENTE.
 *
 * Por defecto la búsqueda muestra solo las OT con saldo, que es el caso de uso real.
 */
@Component({
  selector: 'app-pagos-registro',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    EmptyState,
    EstadoOtChip,
    ListSkeleton,
    PageHeader,
    PesosPipe,
    ResumenFinanciero,
    SelectorOrden,
  ],
  providers: [provideNativeDateAdapter(), { provide: MAT_DATE_LOCALE, useValue: 'es-CL' }],
  templateUrl: './pagos-registro.html',
  styleUrl: './pagos-registro.scss',
})
export class PagosRegistro {
  private readonly fb = inject(FormBuilder);
  private readonly ordenesService = inject(OrdenesDeTrabajo);
  private readonly catalogos = inject(CatalogosComercial);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly orden = signal<OrdenDeTrabajo | null>(null);
  protected readonly cargando = signal(false);
  protected readonly guardando = signal(false);
  protected readonly formasPago = signal<FormaPago[]>([]);

  protected readonly columnas = ['fecha', 'formaPago', 'referencia', 'monto'];

  protected readonly estaAnulada = computed(() => this.orden()?.estadoOTId === ESTADOS_OT.anulado);

  /** Cuotas que el próximo pago puede llegar a cubrir — se muestran para dar contexto. */
  protected readonly cuotasPendientes = computed(
    () =>
      this.orden()?.cuotas.filter((cuota) => cuota.estadoCuotaId === ESTADOS_CUOTA.pendiente) ?? [],
  );

  protected readonly form = this.fb.nonNullable.group({
    monto: this.fb.control<number | null>(null, [Validators.required, Validators.min(1)]),
    formaPagoId: this.fb.control<number | null>(null, Validators.required),
    fechaPago: this.fb.control<Date | null>(new Date()),
    referencia: ['', Validators.maxLength(100)],
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
    this.form.reset({ monto: null, formaPagoId: null, fechaPago: new Date(), referencia: '' });
    this.router.navigate(['/pagos']);
  }

  protected verOrden(): void {
    const orden = this.orden();
    if (orden) {
      this.router.navigate(['/ordenes-de-trabajo', orden.publicId]);
    }
  }

  /** Cobra el saldo completo de una vez — es lo más frecuente en el mesón. */
  protected pagarSaldoTotal(): void {
    const saldo = this.orden()?.saldo ?? 0;
    if (saldo > 0) {
      this.form.controls.monto.setValue(saldo);
    }
  }

  protected registrar(): void {
    const orden = this.orden();
    if (!orden || this.guardando()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { monto, formaPagoId, fechaPago, referencia } = this.form.getRawValue();
    this.guardando.set(true);

    this.ordenesService
      .registrarPago(orden.publicId, {
        monto: monto!,
        formaPagoId: formaPagoId!,
        fechaPago: fechaPago ? aFechaHoraIso(fechaPago) : null,
        referencia: referencia || null,
      })
      .subscribe({
        next: (actualizada) => {
          this.orden.set(actualizada);
          this.guardando.set(false);
          this.form.reset({
            monto: null,
            formaPagoId: null,
            fechaPago: new Date(),
            referencia: '',
          });
          this.toast.exito('Pago registrado.');
        },
        error: () => this.guardando.set(false),
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
