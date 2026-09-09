import { Component, DestroyRef, inject, input, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

/**
 * Búsqueda tipo "Google": un único campo de texto que reemplaza los filtros por columna.
 * Emite `buscar` con el término ya trimmeado, con debounce y sin repetir el valor
 * anterior. Al limpiar o presionar Enter emite de inmediato (sin esperar el debounce).
 *
 * ```html
 * <app-search-box (buscar)="estado.buscar($event)" [valorInicial]="estado.busqueda()" />
 * ```
 */
@Component({
  selector: 'app-search-box',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
  ],
  templateUrl: './search-box.html',
  styleUrl: './search-box.scss',
})
export class SearchBox {
  private readonly destroyRef = inject(DestroyRef);

  readonly valorInicial = input('');
  readonly placeholder = input('Buscar…');
  readonly etiqueta = input('Buscar');
  readonly debounceMs = input(350);

  readonly buscar = output<string>();

  protected readonly control = new FormControl('', { nonNullable: true });

  /** Canal único de emisión: unifica el debounce del tecleo con las emisiones inmediatas (limpiar / Enter). */
  private readonly termino$ = new Subject<string>();

  constructor() {
    this.control.setValue(this.valorInicial(), { emitEvent: false });

    this.control.valueChanges
      .pipe(debounceTime(this.debounceMs()), takeUntilDestroyed(this.destroyRef))
      .subscribe((valor) => this.termino$.next(valor.trim()));

    this.termino$
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((valor) => this.buscar.emit(valor));
  }

  protected limpiar(): void {
    if (this.control.value === '') {
      return;
    }
    this.control.setValue('', { emitEvent: false });
    this.termino$.next('');
  }

  protected emitirAhora(): void {
    this.termino$.next(this.control.value.trim());
  }
}
