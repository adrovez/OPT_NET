import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
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
import { EstadoOtChip } from '../../../ordenes-de-trabajo/components/estado-ot-chip/estado-ot-chip';
import { ResumenFinanciero } from '../../../ordenes-de-trabajo/components/resumen-financiero/resumen-financiero';
import { SelectorOrden } from '../../../ordenes-de-trabajo/components/selector-orden/selector-orden';
import {
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
 * Módulo Abonos — registrar el abono inicial de una OT y ver los ya registrados.
 * Equivale a la pestaña "Abonos" del asistente de ingreso del legacy, pero como pantalla
 * propia: en el mesón el abono muchas veces se toma después de crear la orden.
 *
 * El abono no es un recurso propio en la API (es un subrecurso de la OT, ADR 0007): por eso
 * la pantalla empieza eligiendo la orden, o la recibe ya elegida en `?ot=<publicId>`.
 *
 * El backend acepta **sobrepago** (decisión de negocio 2026-08-27): un monto mayor al saldo
 * no se bloquea, solo se avisa.
 */
@Component({
  selector: 'app-abonos-registro',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatButtonModule,
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
  templateUrl: './abonos-registro.html',
  styleUrl: './abonos-registro.scss',
})
export class AbonosRegistro {
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

  protected readonly form = this.fb.nonNullable.group({
    monto: this.fb.control<number | null>(null, [Validators.required, Validators.min(1)]),
    formaPagoId: this.fb.control<number | null>(null, Validators.required),
    referencia: ['', Validators.maxLength(50)],
  });

  constructor() {
    this.catalogos.listarFormasPago().subscribe((formas) => this.formasPago.set(formas));

    // La ficha de la OT enlaza acá con la orden ya elegida.
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
    this.form.reset({ monto: null, formaPagoId: null, referencia: '' });
    this.router.navigate(['/abonos']);
  }

  protected verOrden(): void {
    const orden = this.orden();
    if (orden) {
      this.router.navigate(['/ordenes-de-trabajo', orden.publicId]);
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

    const { monto, formaPagoId, referencia } = this.form.getRawValue();
    this.guardando.set(true);

    this.ordenesService
      .registrarAbono(orden.publicId, {
        monto: monto!,
        formaPagoId: formaPagoId!,
        referencia: referencia || null,
      })
      .subscribe({
        next: (actualizada) => {
          // El comando devuelve la OT completa ya recalculada: no hace falta releer.
          this.orden.set(actualizada);
          this.guardando.set(false);
          this.form.reset({ monto: null, formaPagoId: null, referencia: '' });
          this.toast.exito('Abono registrado.');
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
