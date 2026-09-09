import { computed, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { EMPTY, Observable, Subject, catchError, switchMap } from 'rxjs';

import { PagedResult } from '../models/paged-result.model';
import {
  ParametrosConsultaPaginada,
  TAMANIO_PAGINA_DEFECTO,
} from '../models/parametros-consulta-paginada.model';

/**
 * Estado + orquestación de un listado con paginación **del lado del servidor**. Centraliza
 * lo que cada `*-list.ts` reimplementaba (pagina, total, cargando, error, `cargar()`), y
 * garantiza que cada cambio de página / búsqueda / orden dispara **una** nueva consulta al
 * backend (nunca se pagina ni se filtra en memoria).
 *
 * Debe instanciarse en el *field initializer* o el constructor del componente (contexto de
 * inyección) — usa `takeUntilDestroyed()` para cerrar la suscripción con el componente.
 * `switchMap` cancela la petición anterior si llega otra antes de que responda.
 *
 * ```ts
 * protected readonly estado = new EstadoListaPaginada<Cliente>((p) => this.clientes.buscar(p));
 * constructor() { this.estado.cargar(); }
 * ```
 */
export class EstadoListaPaginada<T> {
  readonly items = signal<T[]>([]);
  readonly total = signal(0);
  readonly pagina = signal(1);
  readonly tamanioPagina = signal(TAMANIO_PAGINA_DEFECTO);
  readonly busqueda = signal('');
  readonly ordenarPor = signal<string | undefined>(undefined);
  readonly direccionOrden = signal<'asc' | 'desc' | undefined>(undefined);
  readonly cargando = signal(true);
  readonly error = signal(false);

  /** `true` cuando hay un término de búsqueda activo — para distinguir "sin resultados" de "aún no hay datos". */
  readonly hayBusqueda = computed(() => this.busqueda().length > 0);

  private readonly solicitar$ = new Subject<void>();

  constructor(
    private readonly fetch: (p: ParametrosConsultaPaginada) => Observable<PagedResult<T>>,
  ) {
    this.solicitar$
      .pipe(
        switchMap(() => {
          this.cargando.set(true);
          this.error.set(false);
          return this.fetch(this.parametros()).pipe(
            catchError(() => {
              this.error.set(true);
              this.cargando.set(false);
              return EMPTY;
            }),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe((resultado) => {
        this.items.set(resultado.items);
        this.total.set(resultado.total);
        this.cargando.set(false);
      });
  }

  parametros(): ParametrosConsultaPaginada {
    return {
      pagina: this.pagina(),
      tamanioPagina: this.tamanioPagina(),
      busqueda: this.busqueda() || undefined,
      ordenarPor: this.ordenarPor(),
      direccionOrden: this.direccionOrden(),
    };
  }

  /** (Re)ejecuta la consulta con el estado actual. */
  cargar(): void {
    this.solicitar$.next();
  }

  /** Alias semántico para llamar tras crear / editar / eliminar un registro. */
  refrescar(): void {
    this.cargar();
  }

  buscar(texto: string): void {
    this.busqueda.set(texto.trim());
    this.pagina.set(1);
    this.cargar();
  }

  cambiarPagina(evento: PageEvent): void {
    this.pagina.set(evento.pageIndex + 1);
    this.tamanioPagina.set(evento.pageSize);
    this.cargar();
  }

  ordenar(sort: Sort): void {
    if (sort.direction === '') {
      this.ordenarPor.set(undefined);
      this.direccionOrden.set(undefined);
    } else {
      this.ordenarPor.set(sort.active);
      this.direccionOrden.set(sort.direction);
    }
    this.pagina.set(1);
    this.cargar();
  }
}
