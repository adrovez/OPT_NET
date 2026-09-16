import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal, viewChild } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';

import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { Auth } from '../../../../core/services/auth';
import { ListSkeleton } from '../../../../shared/components/list-skeleton/list-skeleton';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PesosPipe } from '../../../../shared/pipes/pesos-pipe';
import { Toast } from '../../../../shared/services/toast';
import {
  aFechaHoraIso,
  aFechaIso,
  aHoraCorta,
  aHoraIso,
} from '../../../../shared/utils/fechas.util';
import { Cliente } from '../../../clientes/models/cliente.model';
import {
  ClienteForm,
  ClienteFormDialogData,
} from '../../../clientes/pages/cliente-form/cliente-form';
import { Clientes } from '../../../clientes/services/clientes';
import { Empresa } from '../../../empresas/models/empresa.model';
import { Empresas } from '../../../empresas/services/empresas';
import { Producto } from '../../../inventario/models/producto.model';
import { Productos } from '../../../inventario/services/productos';
import { RecetaGraduacion } from '../../../receta-cristales/components/receta-graduacion/receta-graduacion';
import { RecetaCristales as RecetaCristalesModel } from '../../../receta-cristales/models/receta-cristales.model';
import {
  RecetaCristalesForm,
  RecetaCristalesFormDialogData,
} from '../../../receta-cristales/pages/receta-cristales-form/receta-cristales-form';
import { RecetaCristales as RecetaCristalesService } from '../../../receta-cristales/services/receta-cristales';
import { Sucursal } from '../../../sucursales/models/sucursal.model';
import { Sucursales } from '../../../sucursales/services/sucursales';
import {
  OrdenCreadaDialog,
  OrdenCreadaDialogData,
} from '../../components/orden-creada-dialog/orden-creada-dialog';
import { ResumenFinanciero } from '../../components/resumen-financiero/resumen-financiero';
import { FormaPago } from '../../models/catalogos-comercial.model';
import { LineaDetalleOT, OrdenDeTrabajo } from '../../models/orden-de-trabajo.model';
import { CatalogosComercial } from '../../services/catalogos-comercial';
import { OrdenesDeTrabajo } from '../../services/ordenes-de-trabajo';

/** Línea del detalle en edición — lleva los datos del producto solo para mostrarlos. */
interface LineaEnEdicion extends LineaDetalleOT {
  productoCodigo: string | null;
  productoDescripcion: string | null;
}

/**
 * Ficha resumida del cliente de la orden, unificando las dos formas en que llega: `Cliente`
 * (nombre + apellido) al crear y `ClienteOT` (nombre completo) al editar.
 */
interface ClienteResumen {
  publicId: string;
  rut: string;
  nombre: string;
  telefono: string | null;
  email: string | null;
  direccion: string | null;
  fechaNacimiento: string | null;
  tipoPrevision: string | null;
}

const TAMANIO_SUGERENCIAS = 10;

/** Un RUT chileno tecleado con o sin puntos, con guion y dígito verificador. */
const PATRON_RUT = /^\d{1,3}(\.?\d{3}){1,2}-[\dkK]$/;

/**
 * Alta y edición de una Orden de Trabajo, como asistente por pasos —Cliente, Receta, Detalle
 * y Pago— que es la misma secuencia de las cuatro pestañas del legacy
 * (`Areas/OrdenTrabajo/Views/Ingreso/Create.cshtml`).
 *
 * Qué se conserva del legacy, porque es el flujo real del mesón:
 *
 * - **El cliente se puede crear sin salir de la orden.** Se busca por RUT o nombre y, si no
 *   existe, se da de alta ahí mismo (el legacy hacía lo mismo en `ClienteInsertar`). Se reusa
 *   el diálogo `ClienteForm` en vez de duplicar sus campos y su cascada Región→Comuna.
 * - **La receta también.** Se puede elegir una del historial clínico del cliente o tomar una
 *   nueva con `RecetaCristalesForm`; la elegida queda vinculada a esta orden (es el `idOT` de
 *   `OPT_RecetaCristales` del legacy).
 * - **El saldo no queda sin plan por descuido**: si tras el abono inicial queda saldo hay que
 *   indicar cuotas y primer vencimiento, o marcar explícitamente "sin plan de cuotas". El
 *   legacy lanzaba una excepción y no ofrecía salida; el sistema nuevo sí permite cobrar con
 *   pagos sueltos, así que se pide confirmarlo en vez de bloquearlo.
 *
 * Qué NO se recupera, porque la migración ya lo mejoró: el precio no se escribe (es la suma
 * del detalle). El N° de OT sí volvió a ser un campo manual (decisión 2026-09-11, a pedido
 * del negocio): el backend valida que no se repita dentro del mismo año en una OT vigente
 * (no anulada) — una OT anulada libera su número.
 *
 * Es una página ruteada, no un diálogo (excepción documentada al patrón "CRUD simple =
 * diálogo" de src/frontend/CLAUDE.md): el detalle es una tabla editable y no cabe en un panel
 * de 560px.
 */
@Component({
  selector: 'app-orden-de-trabajo-form',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatRadioModule,
    MatSelectModule,
    MatStepperModule,
    MatTableModule,
    MatTooltipModule,
    ListSkeleton,
    PageHeader,
    PesosPipe,
    RecetaGraduacion,
    ResumenFinanciero,
  ],
  providers: [provideNativeDateAdapter(), { provide: MAT_DATE_LOCALE, useValue: 'es-CL' }],
  templateUrl: './orden-de-trabajo-form.html',
  styleUrl: './orden-de-trabajo-form.scss',
})
export class OrdenDeTrabajoForm {
  private readonly fb = inject(FormBuilder);
  private readonly ordenesService = inject(OrdenesDeTrabajo);
  private readonly clientesService = inject(Clientes);
  private readonly empresasService = inject(Empresas);
  private readonly sucursalesService = inject(Sucursales);
  private readonly productosService = inject(Productos);
  private readonly recetasService = inject(RecetaCristalesService);
  private readonly catalogos = inject(CatalogosComercial);
  private readonly auth = inject(Auth);
  private readonly toast = inject(Toast);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private readonly stepper = viewChild(MatStepper);

  /** Presente solo en la ruta de edición (`/ordenes-de-trabajo/:publicId/editar`). */
  protected readonly publicId = signal<string | null>(null);
  protected readonly esEdicion = computed(() => this.publicId() !== null);

  protected readonly cargando = signal(false);
  protected readonly guardando = signal(false);

  /** Nombre de la sucursal de una orden ya creada — de solo lectura (ver `cargarOrden`). */
  protected readonly sucursalNombre = signal<string | null>(null);
  private readonly misSucursales = signal<Sucursal[]>([]);
  /** Sucursal actual del menú (`Shell`), con nombre — para mostrarla al crear, sin volver a elegirla. */
  protected readonly sucursalActualNombre = computed(
    () => this.misSucursales().find((s) => s.id === this.auth.sucursalActualId())?.nombre ?? null,
  );
  protected readonly formasPago = signal<FormaPago[]>([]);
  protected readonly sugerenciasCliente = signal<Cliente[]>([]);
  protected readonly sugerenciasEmpresa = signal<Empresa[]>([]);
  protected readonly sugerenciasProducto = signal<Producto[]>([]);

  /** Texto tecleado en el buscador de cliente, para ofrecer el alta con ese RUT ya escrito. */
  protected readonly busquedaCliente = signal('');
  protected readonly buscandoCliente = signal(false);

  protected readonly clienteElegido = signal<Cliente | null>(null);
  protected readonly clienteResumen = signal<ClienteResumen | null>(null);
  protected readonly empresaElegida = signal<Empresa | null>(null);
  protected readonly productoElegido = signal<Producto | null>(null);

  /**
   * Ofrecer "crear cliente" solo cuando la búsqueda ya terminó sin resultados — evita que el
   * botón aparezca y desaparezca mientras se escribe.
   */
  protected readonly puedeCrearCliente = computed(
    () =>
      !this.esEdicion() &&
      !this.clienteElegido() &&
      !this.buscandoCliente() &&
      this.sugerenciasCliente().length === 0 &&
      this.busquedaCliente().trim().length >= 2,
  );

  /** Si lo tecleado ya es un RUT válido, el alta abre con él escrito. */
  protected readonly rutTecleado = computed(() => {
    const texto = this.busquedaCliente().trim();
    return PATRON_RUT.test(texto) ? texto.toUpperCase() : null;
  });

  /**
   * Recetas del cliente elegido, para vincular una a la orden. Se cargan cuando ya hay cliente:
   * antes no hay a quién pedírselas.
   */
  protected readonly recetasCliente = signal<RecetaCristalesModel[]>([]);
  protected readonly recetaPublicId = signal<string | null>(null);

  /**
   * `observaciones` ya no se edita desde el asistente (queda solo `comentario` por línea, a
   * pedido del negocio) — pero una OT editada debe conservar el valor migrado del legacy en vez
   * de perderlo, así que se guarda tal cual se cargó y se reenvía sin cambios al guardar.
   */
  private observacionesActual: string | null = null;

  protected readonly recetaElegida = computed(
    () => this.recetasCliente().find((receta) => receta.publicId === this.recetaPublicId()) ?? null,
  );

  /**
   * Recetas de los últimos 3 meses, más recientes primero — el combo por defecto que pide la
   * observación 4a (ADR 0010): el caso normal del mesón es repetir la última graduación
   * vigente, no revisar todo el historial clínico cada vez. "3 meses" se cuenta desde hoy,
   * no desde la fecha de atención de la orden.
   */
  protected readonly recetasRecientes = computed(() => {
    const limite = new Date();
    limite.setMonth(limite.getMonth() - 3);
    return this.recetasCliente()
      .filter((receta) => new Date(receta.fechaRegistro) >= limite)
      .sort((a, b) => new Date(b.fechaRegistro).getTime() - new Date(a.fechaRegistro).getTime());
  });

  /** El historial completo sigue disponible — una receta antigua puede ser la correcta. */
  protected readonly mostrarHistorialReceta = signal(false);

  protected readonly lineas = signal<LineaEnEdicion[]>([]);
  protected readonly columnasDetalle = [
    'producto',
    'cantidad',
    'valorUnitario',
    'total',
    'comentario',
    'quitar',
  ];

  /** El precio de la OT lo recalcula el backend; acá solo se anticipa lo que va a resultar. */
  protected readonly totalDetalle = computed(() =>
    this.lineas().reduce((suma, linea) => suma + linea.cantidad * linea.valorUnitario, 0),
  );

  // ── Paso 1: cliente, cabecera de la orden y convenio ───────────────────────
  // Fecha atención/entrega/hora y Beneficiario viven acá (no en el paso Detalle): son datos
  // de cabecera que el mesón levanta junto con el cliente, no del detalle de productos
  // (observaciones 2/3/5, ADR 0010).
  protected readonly formCliente = this.fb.nonNullable.group({
    // Ingreso manual, igual que el legacy (decisión 2026-09-11): fijo una vez creada la OT
    // (se deshabilita en edición, ver `cargarOrden`). El duplicado del mismo año se valida
    // en el backend — acá solo se exige un número positivo.
    numeroOT: this.fb.control<number | null>(null, [Validators.required, Validators.min(1)]),
    cliente: this.fb.control<Cliente | string | null>(null, Validators.required),
    fechaAtencion: this.fb.control<Date | null>(new Date()),
    fechaEntrega: this.fb.control<Date | null>(this.fechaEntregaSugerida(), Validators.required),
    horaEntrega: ['', Validators.maxLength(5)],
    beneficiario: ['', Validators.maxLength(100)],
    empresa: this.fb.control<Empresa | string | null>(null),
  });

  // ── Paso 3: detalle de productos ────────────────────────────────────────────
  protected readonly formOrden = this.fb.nonNullable.group({
    // Espejo de `lineas()` para que el paso no se pueda dar por terminado sin detalle:
    // `mat-step` valida por control, no por signal.
    hayDetalle: [false, Validators.requiredTrue],
  });

  /** Línea en construcción (producto + cantidad + valor) — no se envía, alimenta la tabla. */
  protected readonly formLinea = this.fb.nonNullable.group({
    producto: this.fb.control<Producto | string | null>(null),
    cantidad: [1, [Validators.required, Validators.min(1)]],
    valorUnitario: this.fb.control<number | null>(null, [Validators.required, Validators.min(0)]),
    comentario: ['', Validators.maxLength(200)],
  });

  // ── Paso 4: modalidad de pago, abono inicial y plan de cuotas ──────────────
  // El backend los acepta en el mismo POST del alta, en una sola transacción.
  //
  // `modalidadPago` es puramente de UI (no viaja al backend): agrupa las tres formas de
  // cobro que describe la observación 6 (ADR 0010) y, según cuál esté elegida, habilita o
  // deshabilita `abonoInicial`/`numeroCuotas`/`primerVencimiento` — los mismos controles que
  // ya aceptaba `CrearOrdenDeTrabajoCommand`. No se agregó ningún campo nuevo al contrato.
  protected readonly formPago = this.fb.nonNullable.group(
    {
      // Sin preseleccionar: obliga a elegir explícitamente una de las tres modalidades
      // (observación 6, ADR 0010) — nada se fuerza a "paga el total" por defecto.
      modalidadPago: this.fb.control<'total' | 'abonoCuotas' | 'cuotas' | null>(
        null,
        Validators.required,
      ),
      abonoInicial: this.fb.control<number | null>(null, Validators.min(1)),
      formaPagoAbono: this.fb.control<number | null>(null),
      referenciaAbono: ['', Validators.maxLength(50)],
      numeroCuotas: this.fb.control<number | null>(null, [Validators.min(1), Validators.max(60)]),
      primerVencimiento: this.fb.control<Date | null>(null),
      sinPlanCuotas: [false],
    },
    { validators: (grupo: AbstractControl) => this.validarPlanCuotas(grupo) },
  );

  // La app es zoneless: un `computed` que lea `control.value` nunca se recalcularía.
  // Los campos que alimentan cifras en pantalla se leen como signal.
  protected readonly modalidadPago = toSignal(this.formPago.controls.modalidadPago.valueChanges, {
    initialValue: this.formPago.controls.modalidadPago.value,
  });
  private readonly abonoInicial = toSignal(this.formPago.controls.abonoInicial.valueChanges, {
    initialValue: null,
  });
  private readonly numeroCuotas = toSignal(this.formPago.controls.numeroCuotas.valueChanges, {
    initialValue: null,
  });
  private readonly primerVencimiento = toSignal(
    this.formPago.controls.primerVencimiento.valueChanges,
    { initialValue: null as Date | null },
  );

  /** Saldo que quedará tras el abono inicial — es la base del plan de cuotas. */
  protected readonly saldoProyectado = computed(
    () => this.totalDetalle() - (this.abonoInicial() ?? 0),
  );

  protected readonly valorCuotaEstimado = computed(() => {
    const cuotas = this.numeroCuotas();
    if (!cuotas || cuotas < 1) {
      return null;
    }
    return Math.round(this.saldoProyectado() / cuotas);
  });

  /**
   * Vista previa del calendario de cuotas (N°, vencimiento, valor) — la misma regla que
   * `OrdenDeTrabajo.GenerarPlanCuotas` del dominio: valor parejo, la diferencia de redondeo
   * va a la última cuota, vencimiento mensual desde el primer vencimiento indicado (cuotas
   * mensuales, no cada 30 días corridos — confirmado 2026-09-08, ADR 0010). Si el dominio
   * cambia esta fórmula, esta vista previa hay que actualizarla junto con él.
   */
  protected readonly cuotasPreview = computed(() => {
    const numero = this.numeroCuotas();
    const primerVencimiento = this.primerVencimiento();
    const monto = this.saldoProyectado();

    if (!numero || numero < 1 || !primerVencimiento || monto <= 0) {
      return [];
    }

    const valorCuota = Math.trunc((monto / numero) * 100) / 100;
    const acumulado = valorCuota * (numero - 1);
    const ultimoValor = monto - acumulado;

    return Array.from({ length: numero }, (_, indice) => {
      const vencimiento = new Date(primerVencimiento);
      vencimiento.setMonth(vencimiento.getMonth() + indice);
      return {
        numero: indice + 1,
        vencimiento,
        valor: indice === numero - 1 ? ultimoValor : valorCuota,
      };
    });
  });

  constructor() {
    this.sucursalesService.listar().subscribe((sucursales) => this.misSucursales.set(sucursales));
    this.catalogos.listarFormasPago().subscribe((formas) => this.formasPago.set(formas));

    this.escucharBusquedaCliente();
    this.escucharBusquedaEmpresa();
    this.escucharBusquedaProducto();

    // El detalle vive en un signal: el paso lo valida a través de `hayDetalle`, y el plan de
    // cuotas depende del total, así que ambos se reevalúan cuando cambian las líneas.
    effect(() => {
      const hayDetalle = this.lineas().length > 0;
      this.formOrden.controls.hayDetalle.setValue(hayDetalle, { emitEvent: false });
      this.formPago.updateValueAndValidity({ emitEvent: false });
    });

    // Modalidad de pago (observación 6, ADR 0010): habilita/deshabilita abono y cuotas según
    // la opción elegida, y mantiene "Paga el total" sincronizado con el total de la orden.
    effect(() => {
      const modalidad = this.modalidadPago();
      const total = this.totalDetalle();
      const controles = this.formPago.controls;

      if (modalidad === 'total') {
        controles.abonoInicial.enable({ emitEvent: false });
        controles.abonoInicial.setValue(total || null);
        controles.abonoInicial.disable({ emitEvent: false });
        controles.numeroCuotas.setValue(null);
        controles.numeroCuotas.disable({ emitEvent: false });
        controles.primerVencimiento.setValue(null);
        controles.primerVencimiento.disable({ emitEvent: false });
        controles.sinPlanCuotas.setValue(true);
      } else if (modalidad === 'cuotas') {
        controles.abonoInicial.setValue(null);
        controles.abonoInicial.disable({ emitEvent: false });
        controles.formaPagoAbono.setValue(null);
        controles.referenciaAbono.setValue('');
        controles.numeroCuotas.enable({ emitEvent: false });
        controles.primerVencimiento.enable({ emitEvent: false });
        controles.sinPlanCuotas.setValue(false);
      } else if (modalidad === 'abonoCuotas') {
        controles.abonoInicial.enable({ emitEvent: false });
        controles.numeroCuotas.enable({ emitEvent: false });
        controles.primerVencimiento.enable({ emitEvent: false });
        controles.sinPlanCuotas.setValue(false);
      }
      // `modalidad === null`: todavía no se elige ninguna — no se fuerza ningún valor.
    });

    // La ruta de edición es /ordenes-de-trabajo/:publicId/editar; la de alta no lleva parámetro.
    const publicId = this.route.snapshot.paramMap.get('publicId');
    if (publicId) {
      this.publicId.set(publicId);
      this.cargarOrden(publicId);
    }
  }

  // ── Validación del plan de cuotas ──────────────────────────────────────────

  /**
   * Regla heredada del legacy (`Finaliza`): un saldo pendiente no debería quedar sin plan de
   * cuotas por olvido. A diferencia del legacy, que lanzaba excepción sin alternativa, acá se
   * puede declarar que el cobro será con pagos sueltos marcando "sin plan de cuotas".
   */
  private validarPlanCuotas(grupo: AbstractControl): ValidationErrors | null {
    const cuotas = grupo.get('numeroCuotas')?.value as number | null;
    const vencimiento = grupo.get('primerVencimiento')?.value as Date | null;
    const sinPlan = grupo.get('sinPlanCuotas')?.value as boolean;
    const abono = (grupo.get('abonoInicial')?.value as number | null) ?? 0;
    const saldo = this.totalDetalle() - abono;

    if (cuotas && !vencimiento) {
      return { vencimientoRequerido: true };
    }
    if (!cuotas && vencimiento) {
      return { cuotasRequeridas: true };
    }
    if (saldo > 0 && !cuotas && !sinPlan) {
      return { planRequerido: true };
    }
    return null;
  }

  // ── Autocompletados ────────────────────────────────────────────────────────

  protected mostrarCliente(valor: Cliente | string | null): string {
    if (!valor || typeof valor === 'string') {
      return typeof valor === 'string' ? valor : '';
    }
    return `${valor.rut} — ${valor.nombre} ${valor.apellido}`;
  }

  protected mostrarEmpresa(valor: Empresa | string | null): string {
    if (!valor || typeof valor === 'string') {
      return typeof valor === 'string' ? valor : '';
    }
    return valor.nombre;
  }

  protected mostrarProducto(valor: Producto | string | null): string {
    if (!valor || typeof valor === 'string') {
      return typeof valor === 'string' ? valor : '';
    }
    return `${valor.codigo} — ${valor.descripcion}`;
  }

  protected elegirCliente(cliente: Cliente): void {
    this.clienteElegido.set(cliente);
    this.clienteResumen.set(this.aResumen(cliente));
    this.recetaPublicId.set(null);
    this.mostrarHistorialReceta.set(false);
    this.cargarRecetas(cliente.publicId);
  }

  // ── Alta y edición del cliente sin salir de la orden ───────────────────────

  /** Da de alta al cliente con el RUT que el operador ya tecleó (legacy `ClienteInsertar`). */
  protected crearCliente(): void {
    this.abrirDialogoCliente({ rutInicial: this.rutTecleado() ?? undefined });
  }

  /** Corrige los datos del cliente elegido — el legacy los reescribía al guardar la OT. */
  protected editarCliente(): void {
    const cliente = this.clienteElegido();
    if (cliente) {
      this.abrirDialogoCliente({ cliente });
      return;
    }

    const publicId = this.clienteResumen()?.publicId;
    if (publicId) {
      this.clientesService
        .obtener(publicId)
        .subscribe((completo) => this.abrirDialogoCliente({ cliente: completo }));
    }
  }

  private abrirDialogoCliente(data: ClienteFormDialogData): void {
    this.dialog
      .open<ClienteForm, ClienteFormDialogData, Cliente | undefined>(ClienteForm, { data })
      .afterClosed()
      .subscribe((cliente) => {
        if (!cliente) {
          return;
        }
        // El alta desde la OT no puede dejar el paso en un estado a medias: se selecciona.
        this.sugerenciasCliente.set([cliente]);
        this.formCliente.controls.cliente.setValue(cliente, { emitEvent: false });
        this.elegirCliente(cliente);
      });
  }

  // ── Receta ─────────────────────────────────────────────────────────────────

  /** Historial de recetas del cliente — la elegida es la que queda vinculada a la orden. */
  private cargarRecetas(clientePublicId: string): void {
    this.recetasService.obtenerPorCliente(clientePublicId).subscribe((recetas) => {
      this.recetasCliente.set(recetas);

      // Atajo del mesón (observación 4a, ADR 0010): al dar de alta una OT nueva, se
      // preselecciona la receta más reciente dentro de los últimos 3 meses. En edición no se
      // toca — el vínculo actual ya viene preseleccionado desde `cargarOrden` y se reenvía tal
      // cual (`recetaPublicId` es estado final, ADR 0008).
      if (!this.esEdicion() && this.recetaPublicId() === null) {
        const [masReciente] = this.recetasRecientes();
        if (masReciente) {
          this.recetaPublicId.set(masReciente.publicId);
        }
      }
    });
  }

  protected elegirReceta(publicId: string | null): void {
    this.recetaPublicId.set(publicId);
  }

  /** Toma de receta en el mismo acto de la OT, como la pestaña Receta del legacy. */
  protected crearReceta(): void {
    const clientePublicId = this.clienteResumen()?.publicId;
    if (!clientePublicId) {
      this.toast.error('Elige primero el cliente de la orden.');
      return;
    }

    this.dialog
      .open<RecetaCristalesForm, RecetaCristalesFormDialogData, RecetaCristalesModel | undefined>(
        RecetaCristalesForm,
        { data: { clientePublicId }, width: '960px', maxWidth: '96vw' },
      )
      .afterClosed()
      .subscribe((receta) => {
        if (!receta) {
          return;
        }
        this.recetasCliente.update((recetas) => [receta, ...recetas]);
        this.recetaPublicId.set(receta.publicId);
      });
  }

  protected elegirEmpresa(empresa: Empresa): void {
    this.empresaElegida.set(empresa);
  }

  protected elegirProducto(producto: Producto): void {
    this.productoElegido.set(producto);
  }

  protected quitarEmpresa(): void {
    this.empresaElegida.set(null);
    this.formCliente.controls.empresa.setValue(null);
  }

  // ── Detalle ────────────────────────────────────────────────────────────────

  protected agregarLinea(): void {
    const producto = this.productoElegido();
    const { cantidad, valorUnitario, comentario } = this.formLinea.getRawValue();

    if (!producto || this.formLinea.invalid || valorUnitario === null) {
      this.formLinea.markAllAsTouched();
      this.toast.error('Elige un producto e indica cantidad y valor.');
      return;
    }

    this.lineas.update((lineas) => [
      ...lineas,
      {
        productoId: producto.id,
        productoCodigo: producto.codigo,
        productoDescripcion: producto.descripcion,
        cantidad,
        valorUnitario,
        comentario: comentario.trim() || null,
      },
    ]);

    this.formLinea.reset({ producto: null, cantidad: 1, valorUnitario: null, comentario: '' });
    this.productoElegido.set(null);
    this.sugerenciasProducto.set([]);
  }

  protected quitarLinea(indice: number): void {
    this.lineas.update((lineas) => lineas.filter((_, i) => i !== indice));
  }

  // ── Guardar ────────────────────────────────────────────────────────────────

  protected guardar(): void {
    if (this.guardando()) {
      return;
    }

    if (this.formCliente.invalid || this.formOrden.invalid) {
      this.formCliente.markAllAsTouched();
      this.formOrden.markAllAsTouched();
      this.toast.error('Revisa los pasos marcados: falta información obligatoria.');
      return;
    }

    if (this.lineas().length === 0) {
      this.toast.error('La orden necesita al menos una línea de detalle.');
      return;
    }

    const cabecera = this.formCliente.getRawValue();
    const detalles: LineaDetalleOT[] = this.lineas().map((linea) => ({
      productoId: linea.productoId,
      cantidad: linea.cantidad,
      valorUnitario: linea.valorUnitario,
      comentario: linea.comentario ?? null,
    }));

    const comunes = {
      fechaEntrega: aFechaHoraIso(cabecera.fechaEntrega!),
      detalles,
      empresaPublicId: this.empresaElegida()?.publicId ?? null,
      recetaPublicId: this.recetaPublicId(),
      observaciones: this.observacionesActual,
      beneficiario: cabecera.beneficiario || null,
      fechaAtencion: cabecera.fechaAtencion ? aFechaIso(cabecera.fechaAtencion) : null,
      horaEntrega: cabecera.horaEntrega ? aHoraIso(cabecera.horaEntrega) : null,
    };

    const publicId = this.publicId();
    if (publicId) {
      this.guardando.set(true);
      this.ordenesService.actualizar(publicId, comunes).subscribe({
        next: (orden) => {
          this.guardando.set(false);
          this.toast.exito('Orden de trabajo actualizada.');
          this.router.navigate(['/ordenes-de-trabajo', orden.publicId]);
        },
        error: () => this.guardando.set(false),
      });
      return;
    }

    const cliente = this.clienteElegido();
    if (!cliente) {
      this.toast.error('Elige el cliente de la orden.');
      return;
    }

    // Sucursal actual del menú (ver `Auth.cambiarSucursal`), no un campo del formulario:
    // el legacy la tomaba de la cookie de sesión, acá se toma del selector del `Shell`.
    const sucursalId = this.auth.sucursalActualId();
    if (!sucursalId) {
      this.toast.error('No hay una sucursal seleccionada en el menú.');
      return;
    }

    if (this.formPago.invalid) {
      this.formPago.markAllAsTouched();
      this.toast.error('Revisa el paso de pago: falta definir el plan de cuotas.');
      return;
    }

    const pago = this.formPago.getRawValue();
    this.guardando.set(true);
    this.ordenesService
      .crear({
        ...comunes,
        numeroOT: cabecera.numeroOT!,
        clientePublicId: cliente.publicId,
        sucursalId,
        abonoInicial: pago.abonoInicial,
        formaPagoAbono: pago.abonoInicial ? pago.formaPagoAbono : null,
        referenciaAbono: pago.abonoInicial ? pago.referenciaAbono || null : null,
        numeroCuotas: pago.numeroCuotas,
        primerVencimiento: pago.primerVencimiento ? aFechaIso(pago.primerVencimiento) : null,
      })
      .subscribe({
        next: (orden) => {
          this.guardando.set(false);
          this.confirmarCreacion(orden);
        },
        error: () => this.guardando.set(false),
      });
  }

  /** Cierre del alta: ticket a la vista, con la opción de imprimirlo antes de ver la orden. */
  private confirmarCreacion(orden: OrdenDeTrabajo): void {
    this.toast.exito(`OT N° ${orden.numeroOT} creada.`);

    this.dialog
      .open<OrdenCreadaDialog, OrdenCreadaDialogData, void>(OrdenCreadaDialog, {
        data: { orden },
        disableClose: true,
      })
      .afterClosed()
      .subscribe(() => {
        this.router.navigate(['/ordenes-de-trabajo', orden.publicId]);
      });
  }

  protected cancelar(): void {
    const publicId = this.publicId();
    this.router.navigate(publicId ? ['/ordenes-de-trabajo', publicId] : ['/ordenes-de-trabajo']);
  }

  private cargarOrden(publicId: string): void {
    this.cargando.set(true);
    this.ordenesService.obtener(publicId).subscribe({
      next: (orden) => {
        this.observacionesActual = orden.observaciones ?? null;
        this.formCliente.patchValue({
          fechaEntrega: new Date(orden.fechaEntrega),
          fechaAtencion: orden.fechaAtencion ? new Date(`${orden.fechaAtencion}T00:00:00`) : null,
          horaEntrega: aHoraCorta(orden.horaEntrega),
          beneficiario: orden.beneficiario ?? '',
        });
        this.sucursalNombre.set(orden.sucursalNombre);
        this.formCliente.controls.numeroOT.setValue(orden.numeroOT);

        // Cliente y N° de OT no son editables una vez creada la orden (la sucursal tampoco,
        // pero ya no es un control del formulario — ver `sucursalNombre`, de solo lectura).
        this.formCliente.controls.cliente.setValue(`${orden.clienteRut} — ${orden.clienteNombre}`);
        this.formCliente.controls.cliente.disable();
        this.formCliente.controls.numeroOT.disable();
        this.clienteResumen.set({
          publicId: orden.cliente.publicId,
          rut: orden.cliente.rut,
          nombre: orden.cliente.nombre,
          telefono: orden.cliente.telefono,
          email: orden.cliente.email,
          direccion: orden.cliente.direccion,
          fechaNacimiento: orden.cliente.fechaNacimiento,
          tipoPrevision: orden.cliente.tipoPrevision,
        });

        if (orden.empresaPublicId && orden.empresaNombre) {
          const empresa = {
            publicId: orden.empresaPublicId,
            nombre: orden.empresaNombre,
          } as Empresa;
          this.empresaElegida.set(empresa);
          this.formCliente.controls.empresa.setValue(empresa);
        }

        this.lineas.set(
          orden.detalles.map((detalle) => ({
            productoId: detalle.productoId,
            productoCodigo: detalle.productoCodigo,
            productoDescripcion: detalle.productoDescripcion,
            cantidad: detalle.cantidad,
            valorUnitario: detalle.valorUnitario,
            comentario: detalle.comentario,
          })),
        );

        // En edición el vínculo actual se preselecciona y se reenvía tal cual: el backend trata
        // `recetaPublicId` como estado final, así que omitirlo desvincularía la receta.
        this.recetaPublicId.set(orden.recetas[0]?.publicId ?? null);
        this.cargarRecetas(orden.cliente.publicId);

        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
        this.router.navigate(['/ordenes-de-trabajo']);
      },
    });
  }

  private aResumen(cliente: Cliente): ClienteResumen {
    return {
      publicId: cliente.publicId,
      rut: cliente.rut,
      nombre: `${cliente.nombre} ${cliente.apellido}`.trim(),
      telefono: cliente.telefono,
      email: cliente.email,
      direccion: cliente.direccion,
      fechaNacimiento: cliente.fechaNacimiento,
      tipoPrevision: cliente.tipoPrevision,
    };
  }

  /** Diez días hábiles, la misma regla de plazo que traía el formulario del legacy. */
  private fechaEntregaSugerida(): Date {
    const fecha = new Date();
    let habiles = 0;
    while (habiles < 10) {
      fecha.setDate(fecha.getDate() + 1);
      if (fecha.getDay() !== 0 && fecha.getDay() !== 6) {
        habiles++;
      }
    }
    return fecha;
  }

  private escucharBusquedaCliente(): void {
    this.formCliente.controls.cliente.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((valor) => {
          if (typeof valor !== 'string' || valor.trim().length < 2) {
            this.busquedaCliente.set(typeof valor === 'string' ? valor : '');
            this.buscandoCliente.set(false);
            return of(null);
          }
          // Al escribir se descarta la selección previa: lo tecleado ya no la representa.
          this.busquedaCliente.set(valor);
          this.buscandoCliente.set(true);
          this.clienteElegido.set(null);
          return this.clientesService.buscar({
            pagina: 1,
            tamanioPagina: TAMANIO_SUGERENCIAS,
            busqueda: valor.trim(),
          });
        }),
        takeUntilDestroyed(),
      )
      .subscribe((resultado) => {
        this.buscandoCliente.set(false);
        this.sugerenciasCliente.set(resultado?.items ?? []);
      });
  }

  private escucharBusquedaEmpresa(): void {
    this.formCliente.controls.empresa.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((valor) => {
          if (typeof valor !== 'string' || valor.trim().length < 2) {
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

  private escucharBusquedaProducto(): void {
    this.formLinea.controls.producto.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((valor) => {
          if (typeof valor !== 'string' || valor.trim().length < 2) {
            return of(null);
          }
          return this.productosService.buscar({
            pagina: 1,
            tamanioPagina: TAMANIO_SUGERENCIAS,
            busqueda: valor.trim(),
          });
        }),
        takeUntilDestroyed(),
      )
      .subscribe((resultado) => this.sugerenciasProducto.set(resultado?.items ?? []));
  }
}
