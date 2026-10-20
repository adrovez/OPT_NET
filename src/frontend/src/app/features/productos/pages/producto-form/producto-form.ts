import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import {
  CATEGORIA_PRODUCTO,
  CategoriaProducto,
  Producto,
  categoriaExigeMarcaYModelo,
} from '../../../inventario/models/producto.model';
import { CategoriasProducto } from '../../../inventario/services/categorias-producto';
import { Productos } from '../../../inventario/services/productos';

export interface ProductoFormDialogData {
  producto?: Producto;
}

/**
 * Diálogo de alta/edición de Producto (ADR 0013). En Marcos y Lentes de Sol se piden Marca,
 * Modelo (obligatorios) y Color (opcional) y la descripción se muestra calculada; en el resto
 * de las categorías la descripción es de texto libre.
 */
@Component({
  selector: 'app-producto-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './producto-form.html',
  styleUrl: './producto-form.scss',
})
export class ProductoForm {
  private readonly fb = inject(FormBuilder);
  private readonly productosService = inject(Productos);
  private readonly categoriasService = inject(CategoriasProducto);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef =
    inject<MatDialogRef<ProductoForm, Producto | undefined>>(MatDialogRef);
  protected readonly data = inject<ProductoFormDialogData>(MAT_DIALOG_DATA);

  protected readonly esEdicion = !!this.data.producto;
  protected readonly guardando = signal(false);
  protected readonly categorias = signal<CategoriaProducto[]>([]);
  /** `true` cuando la categoría elegida usa Marca/Modelo/Color y descripción calculada. */
  protected readonly usaMarcaModelo = signal(false);
  protected readonly descripcionCalculada = signal('');

  protected readonly form = this.fb.group({
    codigo: this.fb.nonNullable.control(this.data.producto?.codigo ?? '', [
      Validators.required,
      Validators.maxLength(50),
    ]),
    categoriaId: this.fb.nonNullable.control(
      this.data.producto?.categoriaId ?? CATEGORIA_PRODUCTO.marcos,
      Validators.required,
    ),
    marca: this.fb.control(this.data.producto?.marca ?? '', Validators.maxLength(100)),
    modelo: this.fb.control(this.data.producto?.modelo ?? '', Validators.maxLength(100)),
    color: this.fb.control(this.data.producto?.color ?? '', Validators.maxLength(50)),
    descripcion: this.fb.control(this.data.producto?.descripcion ?? '', Validators.maxLength(200)),
    controlStock: this.fb.nonNullable.control(this.data.producto?.controlStock ?? true),
  });

  constructor() {
    this.categoriasService
      .listar()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((categorias) => this.categorias.set(categorias));

    this.form.controls.categoriaId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((id) => this.aplicarCategoria(id));
    this.form.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.recalcularDescripcion());

    this.aplicarCategoria(this.form.controls.categoriaId.value);
  }

  /** Marca y Modelo son obligatorias solo en categorías que las usan; la descripción, solo en las demás. */
  private aplicarCategoria(categoriaId: number): void {
    const usa = categoriaExigeMarcaYModelo(categoriaId);
    this.usaMarcaModelo.set(usa);
    const { marca, modelo, descripcion } = this.form.controls;

    for (const control of [marca, modelo]) {
      control.setValidators(
        usa ? [Validators.required, Validators.maxLength(100)] : [Validators.maxLength(100)],
      );
    }
    descripcion.setValidators(
      usa ? [Validators.maxLength(200)] : [Validators.required, Validators.maxLength(200)],
    );
    [marca, modelo, descripcion].forEach((c) => c.updateValueAndValidity({ emitEvent: false }));
    this.recalcularDescripcion();
  }

  private recalcularDescripcion(): void {
    if (!this.usaMarcaModelo()) {
      this.descripcionCalculada.set('');
      return;
    }
    const { marca, modelo, color } = this.form.getRawValue();
    const partes = [marca, modelo, color].map((v) => (v ?? '').trim().toUpperCase());
    this.descripcionCalculada.set(partes.filter((v) => v).join('-'));
  }

  protected guardar(): void {
    if (this.form.invalid || this.guardando()) {
      return;
    }

    this.guardando.set(true);
    const v = this.form.getRawValue();
    const usa = this.usaMarcaModelo();
    const datos = {
      codigo: v.codigo,
      categoriaId: v.categoriaId,
      controlStock: v.controlStock,
      descripcion: usa ? null : (v.descripcion ?? '').trim(),
      marca: usa ? (v.marca ?? '').trim() : null,
      modelo: usa ? (v.modelo ?? '').trim() : null,
      color: usa ? (v.color ?? '').trim() || null : null,
    };

    const peticion = this.esEdicion
      ? this.productosService.actualizar(this.data.producto!.id, datos)
      : this.productosService.crear(datos);

    peticion.subscribe({
      next: (producto) => this.dialogRef.close(producto),
      error: () => this.guardando.set(false),
    });
  }

  protected cancelar(): void {
    this.dialogRef.close();
  }
}
