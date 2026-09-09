import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { DeudorEmpresa } from '../../models/deudor-empresa.model';
import { Cobranza } from '../../services/cobranza';

/**
 * Cobranza — deuda vigente consolidada por empresa convenio. Reemplaza al "Listado
 * Deudores" del legacy (Areas/OrdenTrabajo/Deuda), con dos correcciones:
 *
 * - El saldo viene ya calculado por el backend dentro del agregado OT; el legacy lo restaba
 *   en la vista (`Saldo - Pagado`) y arrastraba 3.472 OT con el saldo inflado.
 * - Las OT anuladas no cuentan como deuda (el legacy no tenía el estado ANULADO).
 *
 * El detalle de un deudor es el listado de OT filtrado: se navega a `/ordenes-de-trabajo`
 * con `empresaPublicId` y `soloConSaldo`, en vez de duplicar la tabla acá.
 *
 * El "Pago Masivo" del legacy no está: no existe endpoint que cobre varias OT a la vez, y
 * el pago se registra OT por OT desde el módulo Pagos.
 */
@Component({
  selector: 'app-cobranza-deudores',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatTooltipModule,
    EmptyState,
    ListSkeleton,
    PageHeader,
    PesosPipe,
  ],
  templateUrl: './cobranza-deudores.html',
  styleUrl: './cobranza-deudores.scss',
})
export class CobranzaDeudores {
  private readonly cobranzaService = inject(Cobranza);
  private readonly router = inject(Router);

  protected readonly columnas = ['empresa', 'cantidadOT', 'total', 'abonado', 'saldo', 'acciones'];

  protected readonly deudores = signal<DeudorEmpresa[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal(false);

  protected readonly totalDeuda = computed(() =>
    this.deudores().reduce((suma, deudor) => suma + deudor.saldo, 0),
  );

  protected readonly totalOT = computed(() =>
    this.deudores().reduce((suma, deudor) => suma + deudor.cantidadOT, 0),
  );

  constructor() {
    this.cargar();
  }

  protected cargar(): void {
    this.cargando.set(true);
    this.error.set(false);
    this.cobranzaService.listarDeudores().subscribe({
      next: (deudores) => {
        this.deudores.set(deudores);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set(true);
        this.cargando.set(false);
      },
    });
  }

  /** Detalle del deudor = listado de OT ya filtrado (no hay endpoint de detalle propio). */
  protected verOrdenes(deudor: DeudorEmpresa): void {
    this.router.navigate(['/ordenes-de-trabajo'], {
      queryParams: {
        empresaPublicId: deudor.empresaPublicId ?? undefined,
        soloConSaldo: 'true',
      },
    });
  }
}
