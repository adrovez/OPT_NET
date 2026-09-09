import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { merge } from 'rxjs';

import { RecetaCristales as RecetaCristalesModel } from '../../models/receta-cristales.model';
import { RecetaCristales } from '../../services/receta-cristales';

export interface RecetaCristalesFormDialogData {
  clientePublicId: string;
  receta?: RecetaCristalesModel;
}

// Rango típico de graduación óptica — mismo límite que CrearRecetaCristalesCommandValidator (backend).
const MIN_GRADUACION = -30;
const MAX_GRADUACION = 30;

// Campos numéricos de cada bloque — usados por bloqueActivoConDatosValidator para decidir si el
// bloque activo ("Incluir Cristales Lejos/Cerca") ya tiene algún dato cargado.
const CAMPOS_LEJOS = [
  'odEsferaLejos', 'odCilindroLejos', 'odEjeLejos',
  'oiEsferaLejos', 'oiCilindroLejos', 'oiEjeLejos',
  'dpLejos', 'addLejos',
] as const;
const CAMPOS_CERCA = [
  'odEsferaCerca', 'odCilindroCerca', 'odEjeCerca',
  'oiEsferaCerca', 'oiCilindroCerca', 'oiEjeCerca',
  'dpCerca',
] as const;

function tieneAlgunDato(grupo: AbstractControl, campos: readonly string[]): boolean {
  return campos.some((campo) => {
    const valor = grupo.get(campo)?.value;
    return valor !== null && valor !== undefined && valor !== '';
  });
}

/**
 * Habilita el botón Guardar solo cuando corresponde: al menos un bloque (Lejos/Cerca) incluido,
 * y cada bloque incluido con algún dato cargado (los campos vacíos de un bloque activo se guardan
 * en cero — ver `guardar()` — así que acá solo se exige que no esté todo vacío). Las 7
 * observaciones (6 de detalle + la general) son siempre opcionales, no participan de este validador.
 */
function bloqueActivoConDatosValidator(): ValidatorFn {
  return (grupo: AbstractControl): ValidationErrors | null => {
    const incluirLejos = grupo.get('incluirLejos')?.value;
    const incluirCerca = grupo.get('incluirCerca')?.value;

    if (!incluirLejos && !incluirCerca) {
      return { bloqueRequerido: true };
    }
    if (incluirLejos && !tieneAlgunDato(grupo, CAMPOS_LEJOS)) {
      return { lejosSinDatos: true };
    }
    if (incluirCerca && !tieneAlgunDato(grupo, CAMPOS_CERCA)) {
      return { cercaSinDatos: true };
    }
    return null;
  };
}

/**
 * Diálogo de alta/edición de RecetaCristales — layout OD/OI x Lejos/Cerca, igual al legacy
 * (`Areas/OrdenTrabajo/Views/Ingreso/_ParcialReceta.cshtml` + `Scripts/Ingreso/Receta.js`), con:
 * - "Incluir Cristales Lejos/Cerca": habilitan los inputs de su bloque (las 3 observaciones
 *   OD/OI/DP quedan editables pero son opcionales — decisión 2026-09-08, ADR 0010); al
 *   desmarcar, el bloque completo se limpia y se deshabilita.
 * - Cálculo automático de Cerca a partir de ADD: al cambiar Esf./Cil./Eje de Lejos o el propio
 *   ADD, si ADD > 0 se recalcula Esférico de Cerca (Lejos + ADD) y se copian Cilíndrico/Eje de
 *   Lejos — igual que Receta.js. Cerca queda editable: el cálculo solo precompleta. Al ingresar
 *   un ADD > 0 se marca "Incluir Cristales Cerca" automáticamente (habilitando sus inputs) si
 *   no lo estaba ya — antes había que marcarlo a mano aunque el cálculo ya mostrara valores. Si
 *   ADD se vacía, se limpia todo el bloque Cerca y se desmarca su check (igual que
 *   `$.LimpiarCerca()`).
 * - DP y ADD son numéricos en la UI (2026-09-08), igual que Esférico/Cilíndrico/Eje — el legacy
 *   los guardaba como texto libre sin formato fijo y la API los sigue recibiendo como string
 *   (`DpLejos`/`DpCerca`/`AddLejos` sin cambio de esquema); si una receta migrada trae un valor
 *   no numérico en alguno de estos tres campos (p. ej. notación "31/30"), al editarla ese campo
 *   se ve vacío en vez de mostrar el texto original — es una pérdida deliberada de esos casos
 *   raros a cambio de un input numérico consistente con el resto de la tabla.
 */
@Component({
  selector: 'app-receta-cristales-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './receta-cristales-form.html',
  styleUrl: './receta-cristales-form.scss',
})
export class RecetaCristalesForm {
  private readonly fb = inject(FormBuilder);
  private readonly recetaService = inject(RecetaCristales);
  private readonly dialogRef =
    inject<MatDialogRef<RecetaCristalesForm, RecetaCristalesModel | undefined>>(MatDialogRef);
  protected readonly data = inject<RecetaCristalesFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.receta;
  protected readonly guardando = signal(false);

  private readonly graduacion = [Validators.min(MIN_GRADUACION), Validators.max(MAX_GRADUACION)];
  private readonly eje = [Validators.min(0), Validators.max(180)];
  private readonly observacionDetalle = [Validators.maxLength(50)];

  protected readonly form = this.fb.nonNullable.group({
    odEsferaLejos: this.fb.control<number | null>(
      this.data.receta?.odEsferaLejos ?? null,
      this.graduacion,
    ),
    odCilindroLejos: this.fb.control<number | null>(
      this.data.receta?.odCilindroLejos ?? null,
      this.graduacion,
    ),
    odEjeLejos: this.fb.control<number | null>(this.data.receta?.odEjeLejos ?? null, this.eje),
    odEsferaCerca: this.fb.control<number | null>(
      this.data.receta?.odEsferaCerca ?? null,
      this.graduacion,
    ),
    odCilindroCerca: this.fb.control<number | null>(
      this.data.receta?.odCilindroCerca ?? null,
      this.graduacion,
    ),
    odEjeCerca: this.fb.control<number | null>(this.data.receta?.odEjeCerca ?? null, this.eje),
    oiEsferaLejos: this.fb.control<number | null>(
      this.data.receta?.oiEsferaLejos ?? null,
      this.graduacion,
    ),
    oiCilindroLejos: this.fb.control<number | null>(
      this.data.receta?.oiCilindroLejos ?? null,
      this.graduacion,
    ),
    oiEjeLejos: this.fb.control<number | null>(this.data.receta?.oiEjeLejos ?? null, this.eje),
    oiEsferaCerca: this.fb.control<number | null>(
      this.data.receta?.oiEsferaCerca ?? null,
      this.graduacion,
    ),
    oiCilindroCerca: this.fb.control<number | null>(
      this.data.receta?.oiCilindroCerca ?? null,
      this.graduacion,
    ),
    oiEjeCerca: this.fb.control<number | null>(this.data.receta?.oiEjeCerca ?? null, this.eje),
    dpLejos: this.fb.control<number | null>(this.parsear(this.data.receta?.dpLejos ?? null), []),
    dpCerca: this.fb.control<number | null>(this.parsear(this.data.receta?.dpCerca ?? null), []),
    addLejos: this.fb.control<number | null>(this.parsear(this.data.receta?.addLejos ?? null), []),
    urgente: [this.data.receta?.urgente ?? false],
    requiereLab: [this.data.receta?.requiereLab ?? false],
    observaciones: [this.data.receta?.observaciones ?? '', [Validators.maxLength(500)]],
    incluirLejos: [this.data.receta?.incluirLejos ?? false],
    incluirCerca: [this.data.receta?.incluirCerca ?? false],
    observacionOdLejos: [this.data.receta?.observacionOdLejos ?? '', this.observacionDetalle],
    observacionOiLejos: [this.data.receta?.observacionOiLejos ?? '', this.observacionDetalle],
    observacionDpLejos: [this.data.receta?.observacionDpLejos ?? '', this.observacionDetalle],
    observacionOdCerca: [this.data.receta?.observacionOdCerca ?? '', this.observacionDetalle],
    observacionOiCerca: [this.data.receta?.observacionOiCerca ?? '', this.observacionDetalle],
    observacionDpCerca: [this.data.receta?.observacionDpCerca ?? '', this.observacionDetalle],
  }, { validators: bloqueActivoConDatosValidator() });

  // Reflejan el check "Incluir Cristales Lejos/Cerca" como signal — el checkbox se mudó a la
  // caption de cada tabla (2026-09-08) y el template los usa para atenuar visualmente el bloque
  // que no aplica, en vez de solo confiar en el gris nativo de los inputs deshabilitados.
  protected readonly incluirLejosActivo = toSignal(this.form.controls.incluirLejos.valueChanges, {
    initialValue: this.form.controls.incluirLejos.value,
  });
  protected readonly incluirCercaActivo = toSignal(this.form.controls.incluirCerca.valueChanges, {
    initialValue: this.form.controls.incluirCerca.value,
  });

  constructor() {
    // "Incluir Cristales Lejos/Cerca": habilita/deshabilita y limpia su bloque completo.
    this.form.controls.incluirLejos.valueChanges.subscribe((incluir) => this.alternarLejos(incluir));
    this.form.controls.incluirCerca.valueChanges.subscribe((incluir) => this.alternarCerca(incluir));

    // Cálculo de Cerca a partir de Lejos + ADD — mismos 7 campos que escuchan los handlers
    // "change" de Receta.js (LejosOD/OI Esférico/Cilindro/Eje y LejosADDEsfera).
    merge(
      this.form.controls.odEsferaLejos.valueChanges,
      this.form.controls.odCilindroLejos.valueChanges,
      this.form.controls.odEjeLejos.valueChanges,
      this.form.controls.oiEsferaLejos.valueChanges,
      this.form.controls.oiCilindroLejos.valueChanges,
      this.form.controls.oiEjeLejos.valueChanges,
      this.form.controls.addLejos.valueChanges,
    ).subscribe(() => this.recalcularCerca());

    // Estado inicial: habilita/deshabilita según lo guardado (alta nueva = ambos bloques
    // deshabilitados) sin limpiar datos ya cargados en edición.
    this.alternarLejos(this.form.controls.incluirLejos.value, false);
    this.alternarCerca(this.form.controls.incluirCerca.value, false);
  }

  /** Convierte texto libre chileno ("1,25") a número — igual que Receta.js (`replace(",", ".")`) . */
  private parsear(valor: string | number | null): number | null {
    if (valor === null || valor === undefined || valor === '') {
      return null;
    }
    const numero = typeof valor === 'number' ? valor : parseFloat(String(valor).replace(',', '.'));
    return Number.isNaN(numero) ? null : numero;
  }

  private alternarLejos(incluir: boolean, limpiarSiApagado = true): void {
    const v = this.form.controls;
    const numericos = [
      v.odEsferaLejos, v.odCilindroLejos, v.odEjeLejos,
      v.oiEsferaLejos, v.oiCilindroLejos, v.oiEjeLejos,
      v.dpLejos, v.addLejos,
    ];
    const observaciones = [v.observacionOdLejos, v.observacionOiLejos, v.observacionDpLejos];

    if (incluir) {
      [...numericos, ...observaciones].forEach((c) => c.enable({ emitEvent: false }));
    } else {
      if (limpiarSiApagado) {
        numericos.forEach((c) => c.reset(null, { emitEvent: false }));
        observaciones.forEach((c) => c.reset('', { emitEvent: false }));
        this.recalcularCerca();
      }
      [...numericos, ...observaciones].forEach((c) => c.disable({ emitEvent: false }));
    }
  }

  private alternarCerca(incluir: boolean, limpiarSiApagado = true): void {
    const v = this.form.controls;
    const numericos = [
      v.odEsferaCerca, v.odCilindroCerca, v.odEjeCerca,
      v.oiEsferaCerca, v.oiCilindroCerca, v.oiEjeCerca,
      v.dpCerca,
    ];
    const observaciones = [v.observacionOdCerca, v.observacionOiCerca, v.observacionDpCerca];

    if (incluir) {
      [...numericos, ...observaciones].forEach((c) => c.enable({ emitEvent: false }));
    } else {
      if (limpiarSiApagado) {
        numericos.forEach((c) => c.reset(null, { emitEvent: false }));
        observaciones.forEach((c) => c.reset('', { emitEvent: false }));
      }
      [...numericos, ...observaciones].forEach((c) => c.disable({ emitEvent: false }));
    }
  }

  private recalcularCerca(): void {
    const v = this.form.controls;
    const add = this.parsear(v.addLejos.value);

    if (add !== null && add > 0) {
      if (!v.incluirCerca.value) {
        // Ingresar un ADD > 0 implica que corresponde receta de Cerca — marcar el check y
        // habilitar sus inputs en vez de dejar los valores calculados en un bloque deshabilitado.
        v.incluirCerca.setValue(true);
      }

      const odEsfera = this.parsear(v.odEsferaLejos.value);
      const oiEsfera = this.parsear(v.oiEsferaLejos.value);

      v.odEsferaCerca.setValue(odEsfera !== null ? odEsfera + add : null, { emitEvent: false });
      v.odCilindroCerca.setValue(v.odCilindroLejos.value, { emitEvent: false });
      v.odEjeCerca.setValue(v.odEjeLejos.value, { emitEvent: false });

      v.oiEsferaCerca.setValue(oiEsfera !== null ? oiEsfera + add : null, { emitEvent: false });
      v.oiCilindroCerca.setValue(v.oiCilindroLejos.value, { emitEvent: false });
      v.oiEjeCerca.setValue(v.oiEjeLejos.value, { emitEvent: false });
    } else if (v.incluirCerca.value) {
      // ADD vacío o 0 — igual que Receta.js: desmarca "Incluir Cristales Cerca" y limpia el bloque.
      v.incluirCerca.setValue(false);
    }
  }

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const valores = this.form.getRawValue();
    // Campos numéricos vacíos de un bloque incluido se guardan en cero (los del bloque no
    // incluido quedan como estén — normalmente null, por `alternarLejos`/`alternarCerca`).
    const cero = (valor: number | null, bloqueActivo: boolean) =>
      bloqueActivo ? (valor ?? 0) : valor;

    const datos = {
      ...valores,
      odEsferaLejos: cero(valores.odEsferaLejos, valores.incluirLejos),
      odCilindroLejos: cero(valores.odCilindroLejos, valores.incluirLejos),
      odEjeLejos: cero(valores.odEjeLejos, valores.incluirLejos),
      oiEsferaLejos: cero(valores.oiEsferaLejos, valores.incluirLejos),
      oiCilindroLejos: cero(valores.oiCilindroLejos, valores.incluirLejos),
      oiEjeLejos: cero(valores.oiEjeLejos, valores.incluirLejos),
      odEsferaCerca: cero(valores.odEsferaCerca, valores.incluirCerca),
      odCilindroCerca: cero(valores.odCilindroCerca, valores.incluirCerca),
      odEjeCerca: cero(valores.odEjeCerca, valores.incluirCerca),
      oiEsferaCerca: cero(valores.oiEsferaCerca, valores.incluirCerca),
      oiCilindroCerca: cero(valores.oiCilindroCerca, valores.incluirCerca),
      oiEjeCerca: cero(valores.oiEjeCerca, valores.incluirCerca),
      // La API sigue recibiendo DP/ADD como string (sin cambio de esquema) — se formatea el
      // número de vuelta a texto solo al enviar, la UI trabaja con ellos como numéricos.
      dpLejos: cero(valores.dpLejos, valores.incluirLejos) !== null
        ? String(cero(valores.dpLejos, valores.incluirLejos))
        : null,
      dpCerca: cero(valores.dpCerca, valores.incluirCerca) !== null
        ? String(cero(valores.dpCerca, valores.incluirCerca))
        : null,
      addLejos: cero(valores.addLejos, valores.incluirLejos) !== null
        ? String(cero(valores.addLejos, valores.incluirLejos))
        : null,
      observaciones: valores.observaciones || null,
      observacionOdLejos: valores.observacionOdLejos || null,
      observacionOiLejos: valores.observacionOiLejos || null,
      observacionDpLejos: valores.observacionDpLejos || null,
      observacionOdCerca: valores.observacionOdCerca || null,
      observacionOiCerca: valores.observacionOiCerca || null,
      observacionDpCerca: valores.observacionDpCerca || null,
    };

    const peticion = this.esEdicion
      ? this.recetaService.actualizar(this.data.receta!.publicId, datos)
      : this.recetaService.crear(this.data.clientePublicId, datos);

    peticion.subscribe({
      next: (receta) => this.dialogRef.close(receta),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
