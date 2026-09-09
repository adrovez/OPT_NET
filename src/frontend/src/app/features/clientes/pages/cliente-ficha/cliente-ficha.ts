import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { Toast } from '../../../../shared/services/toast';
import { Anamnesis as AnamnesisModel } from '../../../anamnesis/models/anamnesis.model';
import { Anamnesis as AnamnesisService } from '../../../anamnesis/services/anamnesis';
import { AnamnesisForm } from '../../../anamnesis/pages/anamnesis-form/anamnesis-form';
import { RecetaGraduacion } from '../../../receta-cristales/components/receta-graduacion/receta-graduacion';
import { RecetaCristales as RecetaCristalesModel } from '../../../receta-cristales/models/receta-cristales.model';
import { RecetaCristales as RecetaCristalesService } from '../../../receta-cristales/services/receta-cristales';
import { RecetaCristalesForm } from '../../../receta-cristales/pages/receta-cristales-form/receta-cristales-form';
import { Cliente } from '../../models/cliente.model';
import { Clientes } from '../../services/clientes';
import { ClienteForm } from '../cliente-form/cliente-form';

/**
 * Ficha del cliente — reemplaza a la pantalla "Atención" del legacy (OPT_Atencion no tiene
 * tabla equivalente en el esquema nuevo, ver CLAUDE.md). "Atender a un cliente" es navegar
 * aquí y agregar una Anamnesis y/o una RecetaCristales, cada una con su propio historial.
 *
 * Layout tipo "ficha clínica": panel de identidad del paciente fijo a la izquierda +
 * historial clínico en pestañas a la derecha (mismo patrón que la Ficha Paciente del legacy).
 */
@Component({
  selector: 'app-cliente-ficha',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatTooltipModule,
    EmptyState,
    ListSkeleton,
    PageHeader,
    RecetaGraduacion,
  ],
  templateUrl: './cliente-ficha.html',
  styleUrl: './cliente-ficha.scss',
})
export class ClienteFicha implements OnInit {
  readonly publicId = input.required<string>();

  private readonly clientesService = inject(Clientes);
  private readonly anamnesisService = inject(AnamnesisService);
  private readonly recetaService = inject(RecetaCristalesService);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);

  protected readonly cliente = signal<Cliente | null>(null);
  protected readonly cargando = signal(true);
  protected readonly errorCliente = signal(false);

  protected readonly anamnesisLista = signal<AnamnesisModel[]>([]);
  protected readonly recetasLista = signal<RecetaCristalesModel[]>([]);
  protected readonly errorAnamnesis = signal(false);
  protected readonly errorRecetas = signal(false);

  /** Historial ordenado del más reciente al más antiguo. */
  protected readonly anamnesisOrdenada = computed(() =>
    [...this.anamnesisLista()].sort((a, b) => b.fechaRegistro.localeCompare(a.fechaRegistro)),
  );
  protected readonly recetasOrdenadas = computed(() =>
    [...this.recetasLista()].sort((a, b) => b.fechaRegistro.localeCompare(a.fechaRegistro)),
  );

  /** Edad en años a partir de la fecha de nacimiento (null si no hay dato). */
  protected readonly edad = computed(() => {
    const nacimiento = this.cliente()?.fechaNacimiento;
    if (!nacimiento) {
      return null;
    }
    const nac = new Date(nacimiento);
    const hoy = new Date();
    let anios = hoy.getFullYear() - nac.getFullYear();
    const mes = hoy.getMonth() - nac.getMonth();
    if (mes < 0 || (mes === 0 && hoy.getDate() < nac.getDate())) {
      anios--;
    }
    return anios >= 0 && anios < 150 ? anios : null;
  });

  ngOnInit(): void {
    this.cargarCliente();
    this.cargarAnamnesis();
    this.cargarRecetas();
  }

  protected editarCliente(): void {
    const cliente = this.cliente();
    if (!cliente) {
      return;
    }

    this.dialog
      .open(ClienteForm, { data: { cliente } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Cliente actualizado.');
          this.cliente.set(resultado);
        }
      });
  }

  protected nuevaAnamnesis(): void {
    this.dialog
      .open(AnamnesisForm, { data: { clientePublicId: this.publicId() } })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Anamnesis registrada.');
          this.cargarAnamnesis();
        }
      });
  }

  // Regla de negocio: la anamnesis es inmutable una vez creada — sin acción de editar.

  protected eliminarAnamnesis(anamnesis: AnamnesisModel): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Eliminar anamnesis',
          mensaje: '¿Eliminar esta ficha de anamnesis? Esta acción no se puede deshacer.',
          textoConfirmar: 'Eliminar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.anamnesisService.eliminar(anamnesis.publicId).subscribe({
            next: () => {
              this.toast.exito('Anamnesis eliminada.');
              this.cargarAnamnesis();
            },
          });
        }
      });
  }

  protected nuevaReceta(): void {
    this.dialog
      .open(RecetaCristalesForm, {
        data: { clientePublicId: this.publicId() },
        width: '960px',
        maxWidth: '96vw',
      })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Receta registrada.');
          this.cargarRecetas();
        }
      });
  }

  protected editarReceta(receta: RecetaCristalesModel): void {
    this.dialog
      .open(RecetaCristalesForm, {
        data: { clientePublicId: this.publicId(), receta },
        width: '960px',
        maxWidth: '96vw',
      })
      .afterClosed()
      .subscribe((resultado) => {
        if (resultado) {
          this.toast.exito('Receta actualizada.');
          this.cargarRecetas();
        }
      });
  }

  protected eliminarReceta(receta: RecetaCristalesModel): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          titulo: 'Eliminar receta',
          mensaje: '¿Eliminar esta receta de cristales? Esta acción no se puede deshacer.',
          textoConfirmar: 'Eliminar',
          colorConfirmar: 'warn',
        },
      })
      .afterClosed()
      .subscribe((confirmado) => {
        if (confirmado) {
          this.recetaService.eliminar(receta.publicId).subscribe({
            next: () => {
              this.toast.exito('Receta eliminada.');
              this.cargarRecetas();
            },
          });
        }
      });
  }

  protected cargarCliente(): void {
    this.cargando.set(true);
    this.errorCliente.set(false);
    this.clientesService.obtener(this.publicId()).subscribe({
      next: (cliente) => {
        this.cliente.set(cliente);
        this.cargando.set(false);
      },
      error: () => {
        this.errorCliente.set(true);
        this.cargando.set(false);
      },
    });
  }

  protected cargarAnamnesis(): void {
    this.errorAnamnesis.set(false);
    this.anamnesisService.obtenerPorCliente(this.publicId()).subscribe({
      next: (lista) => this.anamnesisLista.set(lista),
      error: () => this.errorAnamnesis.set(true),
    });
  }

  protected cargarRecetas(): void {
    this.errorRecetas.set(false);
    this.recetaService.obtenerPorCliente(this.publicId()).subscribe({
      next: (lista) => this.recetasLista.set(lista),
      error: () => this.errorRecetas.set(true),
    });
  }
}
