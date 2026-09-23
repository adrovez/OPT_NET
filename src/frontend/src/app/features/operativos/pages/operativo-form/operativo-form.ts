import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';

import { aFechaIso } from '../../../../shared/utils/fechas.util';
import { Empresa } from '../../../empresas/models/empresa.model';
import { Empresas } from '../../../empresas/services/empresas';
import { Sucursal } from '../../../sucursales/models/sucursal.model';
import { Sucursales } from '../../../sucursales/services/sucursales';
import { Operativo, OperativoResumen } from '../../models/operativo.model';
import { Operativos } from '../../services/operativos';

const TAMANIO_SUGERENCIAS = 10;

/**
 * Exige que el control de empresa contenga la `Empresa` elegida en el autocompletado, no un
 * texto suelto tipeado por el usuario. Sin este validador, `form.invalid` no reflejaba que
 * faltaba elegir una empresa: el botón Guardar quedaba habilitado igual y `guardar()` retornaba
 * en silencio (sin ningún error visible) — el bug reportado de "no guarda registro".
 */
function empresaSeleccionadaValidator(control: AbstractControl): ValidationErrors | null {
  const valor = control.value;
  return valor && typeof valor === 'object' ? null : { empresaNoSeleccionada: true };
}

export interface OperativoFormDialogData {
  operativo?: OperativoResumen;
}

/**
 * Diálogo de alta/edición de Operativo. Empresa y sucursal son **inmutables tras crear**
 * (ActualizarOperativoCommand solo acepta fecha/observación) — se ocultan en edición, mismo
 * patrón que `EsMatriz` en `sucursal-form` o `Rut` en `cliente-form`.
 *
 * Empresa se busca con autocompletado (491 empresas, no cabe en un `mat-select`); Sucursal es
 * un catálogo chico y usa `mat-select` directo — mismo criterio que el resto del proyecto.
 */
@Component({
  selector: 'app-operativo-form',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  providers: [provideNativeDateAdapter(), { provide: MAT_DATE_LOCALE, useValue: 'es-CL' }],
  templateUrl: './operativo-form.html',
  styleUrl: './operativo-form.scss',
})
export class OperativoForm {
  private readonly fb = inject(FormBuilder);
  private readonly operativosService = inject(Operativos);
  private readonly empresasService = inject(Empresas);
  private readonly sucursalesService = inject(Sucursales);
  private readonly dialogRef =
    inject<MatDialogRef<OperativoForm, Operativo | undefined>>(MatDialogRef);
  protected readonly data = inject<OperativoFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.operativo;
  protected readonly guardando = signal(false);
  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly sugerenciasEmpresa = signal<Empresa[]>([]);

  /** Solo se completa realmente al elegir una opción del autocompletado, nunca al tipear. */
  protected readonly empresaElegida = signal<Empresa | null>(
    this.data.operativo
      ? ({
          publicId: this.data.operativo.empresaPublicId,
          nombre: this.data.operativo.empresaNombre,
        } as Empresa)
      : null,
  );

  protected readonly form = this.fb.nonNullable.group({
    nombre: [this.data.operativo?.nombre ?? '', [Validators.required, Validators.maxLength(200)]],
    empresa: this.fb.control<Empresa | string | null>(this.empresaElegida(), [
      empresaSeleccionadaValidator,
    ]),
    sucursalId: this.fb.control<number | null>(this.data.operativo?.sucursalId ?? null, [
      Validators.required,
    ]),
    fecha: this.fb.control<Date | null>(
      this.data.operativo ? new Date(this.data.operativo.fecha) : null,
      [Validators.required],
    ),
    observacion: ['', [Validators.maxLength(500)]],
    nombreContacto: ['', [Validators.maxLength(200)]],
    mailContacto: ['', [Validators.maxLength(200), Validators.email]],
    telefonoContacto: ['', [Validators.maxLength(30)]],
  });

  constructor() {
    this.sucursalesService.listar().subscribe((sucursales) => this.sucursales.set(sucursales));

    if (this.esEdicion) {
      this.form.controls.empresa.disable();
      this.form.controls.sucursalId.disable();
    } else {
      this.escucharBusquedaEmpresa();
    }

    if (this.data.operativo) {
      this.operativosService.obtener(this.data.operativo.publicId).subscribe((completo) => {
        this.form.controls.observacion.setValue(completo.observacion ?? '');
        this.form.controls.nombreContacto.setValue(completo.nombreContacto ?? '');
        this.form.controls.mailContacto.setValue(completo.mailContacto ?? '');
        this.form.controls.telefonoContacto.setValue(completo.telefonoContacto ?? '');
      });
    }
  }

  protected mostrarEmpresa(valor: Empresa | string | null): string {
    if (!valor || typeof valor === 'string') {
      return typeof valor === 'string' ? valor : '';
    }
    return valor.nombre;
  }

  protected elegirEmpresa(empresa: Empresa): void {
    this.empresaElegida.set(empresa);
  }

  private escucharBusquedaEmpresa(): void {
    this.form.controls.empresa.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((valor) => {
          if (typeof valor !== 'string') {
            // El valor es la `Empresa` recién elegida en el autocompletado (`elegirEmpresa` ya
            // actualizó el signal) — no un texto de búsqueda. Si esto se trata como "sin
            // búsqueda" y se limpia `empresaElegida` acá, la carrera entre este observable
            // debounced y la selección deja el signal en null ~300ms después de elegir una
            // empresa, y `guardar()` revienta con `TypeError: Cannot read properties of null
            // (reading 'publicId')` aunque el formulario se vea válido.
            return of(null);
          }
          if (valor.trim().length < 2) {
            this.empresaElegida.set(null);
            return of(null);
          }
          return this.empresasService.buscar({
            pagina: 1,
            tamanioPagina: TAMANIO_SUGERENCIAS,
            busqueda: valor.trim(),
          });
        }),
        takeUntilDestroyed(),
      )
      .subscribe((resultado) => this.sugerenciasEmpresa.set(resultado?.items ?? []));
  }

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      this.form.markAllAsTouched();
      return;
    }

    this.guardando.set(true);
    const { nombre, sucursalId, fecha, observacion, nombreContacto, mailContacto, telefonoContacto } =
      this.form.getRawValue();

    const peticion = this.esEdicion
      ? this.operativosService.actualizar(this.data.operativo!.publicId, {
          nombre,
          fecha: aFechaIso(fecha!),
          observacion: observacion || null,
          nombreContacto: nombreContacto || null,
          mailContacto: mailContacto || null,
          telefonoContacto: telefonoContacto || null,
        })
      : this.operativosService.crear({
          nombre,
          empresaPublicId: this.empresaElegida()!.publicId,
          sucursalId: sucursalId!,
          fecha: aFechaIso(fecha!),
          observacion: observacion || null,
          nombreContacto: nombreContacto || null,
          mailContacto: mailContacto || null,
          telefonoContacto: telefonoContacto || null,
        });

    peticion.subscribe({
      next: (operativo) => this.dialogRef.close(operativo),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
