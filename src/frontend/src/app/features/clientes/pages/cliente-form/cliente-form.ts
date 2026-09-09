import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { forkJoin } from 'rxjs';

import { Comuna } from '../../../comunas/models/comuna.model';
import { Comunas } from '../../../comunas/services/comunas';
import { Region } from '../../../regiones/models/region.model';
import { Regiones } from '../../../regiones/services/regiones';
import { Cliente } from '../../models/cliente.model';
import { Clientes } from '../../services/clientes';

export interface ClienteFormDialogData {
  cliente?: Cliente;
  /**
   * RUT con el que abrir el formulario de alta ya escrito. Lo usa el alta de una Orden de
   * Trabajo: el operador busca por RUT, no aparece, y crea al cliente sin volver a teclearlo.
   */
  rutInicial?: string;
}

/**
 * Diálogo de alta/edición de Cliente. El RUT solo es editable al crear (inmutable en
 * ActualizarClienteCommand). Región es un filtro local para acotar el combo de Comuna
 * (ComunasController solo expone ObtenerPorRegion, igual que el legacy OPT_ComunaDAL.Lista) —
 * no se persiste en Cliente.
 */
@Component({
  selector: 'app-cliente-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  providers: [provideNativeDateAdapter(), { provide: MAT_DATE_LOCALE, useValue: 'es-CL' }],
  templateUrl: './cliente-form.html',
  styleUrl: './cliente-form.scss',
})
export class ClienteForm {
  private readonly fb = inject(FormBuilder);
  private readonly clientesService = inject(Clientes);
  private readonly regionesService = inject(Regiones);
  private readonly comunasService = inject(Comunas);
  private readonly dialogRef = inject<MatDialogRef<ClienteForm, Cliente | undefined>>(MatDialogRef);
  protected readonly data = inject<ClienteFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.cliente;
  protected readonly guardando = signal(false);
  protected readonly regiones = signal<Region[]>([]);
  protected readonly comunas = signal<Comuna[]>([]);

  protected readonly form = this.fb.nonNullable.group({
    rut: [
      this.data.cliente?.rut ?? this.data.rutInicial ?? '',
      [Validators.required, Validators.maxLength(12)],
    ],
    nombre: [this.data.cliente?.nombre ?? '', [Validators.required, Validators.maxLength(100)]],
    apellido: [this.data.cliente?.apellido ?? '', [Validators.required, Validators.maxLength(100)]],
    email: [this.data.cliente?.email ?? '', [Validators.maxLength(150), Validators.email]],
    telefono: [this.data.cliente?.telefono ?? '', [Validators.maxLength(20)]],
    direccion: [this.data.cliente?.direccion ?? '', [Validators.maxLength(200)]],
    regionId: this.fb.control<number | null>(null),
    comunaId: this.fb.control<number | null>(this.data.cliente?.comunaId ?? null),
    fechaNacimiento: this.fb.control<Date | null>(
      this.data.cliente?.fechaNacimiento ? new Date(this.data.cliente.fechaNacimiento) : null,
    ),
    tipoPrevision: this.fb.control<string | null>(this.data.cliente?.tipoPrevision ?? null),
  });

  constructor() {
    this.regionesService.listar().subscribe((regiones) => {
      this.regiones.set(regiones);

      const comunaId = this.data.cliente?.comunaId;
      if (comunaId) {
        this.ubicarRegionDeComuna(regiones, comunaId);
      }
    });
  }

  /** ComunasController no expone la región de una comuna — se busca una sola vez al editar. */
  private ubicarRegionDeComuna(regiones: Region[], comunaId: number): void {
    forkJoin(regiones.map((region) => this.comunasService.listarPorRegion(region.id))).subscribe(
      (comunasPorRegion) => {
        const regionEncontrada = regiones.find((region, indice) =>
          comunasPorRegion[indice].some((comuna) => comuna.id === comunaId),
        );
        if (regionEncontrada) {
          this.form.controls.regionId.setValue(regionEncontrada.id);
          this.comunas.set(comunasPorRegion[regiones.indexOf(regionEncontrada)]);
        }
      },
    );
  }

  protected onRegionSeleccionada(regionId: number | null): void {
    this.form.controls.comunaId.setValue(null);
    this.comunas.set([]);

    if (regionId) {
      this.comunasService
        .listarPorRegion(regionId)
        .subscribe((comunas) => this.comunas.set(comunas));
    }
  }

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const {
      rut,
      nombre,
      apellido,
      email,
      telefono,
      direccion,
      comunaId,
      fechaNacimiento,
      tipoPrevision,
    } = this.form.getRawValue();
    const datosComunes = {
      nombre,
      apellido,
      email: email || null,
      telefono: telefono || null,
      direccion: direccion || null,
      comunaId,
      fechaNacimiento: fechaNacimiento ? this.formatearFecha(fechaNacimiento) : null,
      tipoPrevision,
    };

    const peticion = this.esEdicion
      ? this.clientesService.actualizar(this.data.cliente!.publicId, datosComunes)
      : this.clientesService.crear({ rut, ...datosComunes });

    peticion.subscribe({
      next: (cliente) => this.dialogRef.close(cliente),
      error: () => this.guardando.set(false),
    });
  }

  private formatearFecha(fecha: Date): string {
    const anio = fecha.getFullYear();
    const mes = String(fecha.getMonth() + 1).padStart(2, '0');
    const dia = String(fecha.getDate()).padStart(2, '0');
    return `${anio}-${mes}-${dia}`;
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
