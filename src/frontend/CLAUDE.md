# CLAUDE.md — Frontend (OPT)

Guía técnica para generar código en `src/frontend/`. Complementa al `CLAUDE.md` y `AGENTS.md` de la raíz del repo — léelos primero para el contexto general del proyecto (dominio de negocio, reglas críticas, convenciones de commits). Este archivo cubre solo lo específico de Angular. Para "cómo correr el proyecto" y la descripción de la estructura, ver `src/frontend/README.md` — no se duplica aquí, esto es el complemento prescriptivo para generar código nuevo.

---

## Stack (no renegociable sin ADR nuevo)

Angular 21, **standalone**, **zoneless** (`provideZonelessChangeDetection()`, sin `zone.js` como dependencia), signals como mecanismo de reactividad, Angular Material 21, Vitest, ESLint (`angular-eslint`) + Prettier. Sin NgRx ni otro store — ver ADR `0002` y la sección "Decisiones específicas del frontend" en `README.md`.

---

## Antes de escribir código de un feature nuevo

**Nunca inventar el contrato de un endpoint.** Este repo tiene un patrón repetido: los `Controller` del backend a veces son stubs vacíos (`InventarioController` a la fecha de este documento; `OrdenesDeTrabajoController` lo era hasta 2026-08-27) mientras que la entidad de `OPT.Domain` ya existe con sus campos reales. Antes de escribir un modelo, servicio o página para un módulo:

1. Leer el `Controller` correspondiente en `src/backend/OPT.API/Controllers/` — si es un stub (comentario `// TODO`), **no** construir CRUD completo en el frontend. Dejar un placeholder (ver `features/inventario/` como ejemplo: solo consume `GET /api/productos`, que es lo único implementado de ese controller) hasta que el backend tenga al menos un endpoint real.
2. Si el `Controller` sí implementa algo, leer el `Command`/`Query` en `OPT.Application/Features/<Modulo>/` para el shape exacto del request/response — los nombres de campo son los que serializa `System.Text.Json` con `PropertyNamingPolicy.CamelCase` (ver `Program.cs`), no hay que adivinarlos ni "mejorarlos".
3. Si no hay `Controller` implementado pero sí hace falta un modelo (p. ej. para la página de listado), basar los campos en la entidad real de `OPT.Domain/Entities/<Modulo>/` — nunca en lo que "probablemente" tendrá el DTO final. Dejarlo documentado con un comentario corto apuntando al archivo de dominio (ver `features/clientes/models/cliente.model.ts` como ejemplo).
4. Toda entidad expuesta por `PublicId` en el backend (`Cliente`, `Empresa`, `Usuario`, `Anamnesis`, `RecetaCristales`, `OrdenDeTrabajo` — ADR `0004`) se referencia por `publicId` en el modelo, el servicio y las rutas del frontend. Nunca agregar ni exponer un `id` numérico de estas 6 entidades. Sus **subrecursos** sí llevan Id interno (`DetalleOT`, `Abono`, `Pago`, `Cuota`, `BitacoraOT`): solo se alcanzan anidados bajo la OT, que ya está protegida (ADR `0007`).

---

## Branding y sistema visual

El tema Angular Material M3 de marca ya está aplicado (`src/theme-colors.scss`, `src/styles.scss`,
`src/theme-tokens.scss`, `src/app/app.config.ts` para el set de iconos) — ver la sección
"Implementación en código" de `.agents/context/branding-ux-ui.md` para el mapa completo de qué
vive dónde antes de tocar cualquiera de esos archivos. Reglas al generar UI nueva:

- Colores: siempre `var(--mat-sys-*)` (roles M3) o `var(--opt-*)` de `theme-tokens.scss` (colores
  con significado de `OPT_EstadoOT`/`OPT_FormaPago`, `primary-dark`). Nunca un hex suelto — si hace
  falta un color no cubierto, calcular su contraste WCAG y agregarlo primero a
  `.agents/context/branding-ux-ui.md`.
- Iconos: `mat-icon` con el nombre de Material Symbols (mismo namespace que Material Icons clásico
  para los nombres ya usados en el repo) — el `fontSet` de Material Symbols Rounded ya es el default
  de la app, no pasar `fontSet` por icono.
- Tipografía de titular (`Fraunces`, clase `.opt-titular`): solo en login/portada/marketing — nunca
  en pantallas de trabajo diario (listados, formularios, tablas). **Única excepción interna:** el
  título de `app-empty-state` la usa (guiño humanista en un "momento de pausa", ver v2.1 abajo).
- **Densidad y tamaños (v2.1)**: `mat.theme()` usa `density: -2` y una escala tipográfica compacta
  (override de `--mat-sys-*` en `styles.scss`) — **nunca fijar `font-size` ni alturas de control por
  componente**. Si un texto se ve grande, es un rol M3 mal elegido (`body-large` vs `body-medium`
  vs `label-large`), no un tamaño que falte. Hay una excepción móvil obligatoria en `styles.scss`
  (input a 16px bajo 600px, anti auto-zoom de iOS) — no tocarla. Ver "Densidad y escala tipográfica
  compacta (v2.1)" en `.agents/context/branding-ux-ui.md`.
- Estado de una OT o una forma de pago: usar las variables `--opt-estado-ot-*`/`--opt-forma-pago-*`
  de `theme-tokens.scss`, siempre como chip (fondo + texto, nunca solo color) — no reinterpretar los
  pares de la tabla de catálogos del branding doc.
- Pantalla que muestre `Anamnesis` o `RecetaCristales`: agregar la clase `.opt-badge-clinico` (ya
  definida globalmente) como badge persistente.
- **Graduación de una receta**: `features/receta-cristales/components/receta-graduacion/`
  (`app-receta-graduacion`, input `receta`) — la usan la ficha del cliente y la pestaña Receta de la
  OT. No volver a escribir las tablas de esfera/cilindro/eje por pantalla.
- **Chip de flag booleano** (Urgente, Laboratorio, antecedentes): clases globales `.opt-chip` +
  `.opt-chip--si|--alerta|--info` de `styles.scss` — no redefinir un `.chip` local por feature.
- Antes de escribir copy de UI/error nuevo, revisar la tabla de voz y tono del branding doc (tuteo
  interno vs. formal/usted hacia el cliente).
- Estado vacío o de error de un listado: usar `shared/components/empty-state/` (`app-empty-state`),
  nunca un `<p>` de texto suelto — ver sección "Estados de listado: vacío y error" más abajo.
- Toda pantalla bajo `Shell` es responsive por herencia (el navbar horizontal ya colapsa a un menú
  de hamburguesa bajo 960px, ver `layout/shell/shell.ts` y sección "Navegación horizontal" de
  `.agents/context/branding-ux-ui.md`) — no agregar lógica de responsive propia por feature salvo
  que la pantalla tenga contenido ancho específico (una tabla con muchas columnas ya hace scroll
  horizontal automático vía `.contenido { overflow-x: auto }` en `shell.scss`).
- Encabezado/hover de `mat-table` y título de `MatDialog` ya tienen tratamiento de marca aplicado
  globalmente (`.mat-mdc-table`/`.mat-mdc-dialog-title` en `src/styles.scss`) — no agregar un color
  de fondo o de texto propio a una tabla o un `h2[mat-dialog-title]` nuevos, ya heredan el estilo.
  Desde v2.1 la cabecera de tabla es liviana (fondo `surface` + borde inferior `primary`, ya no una
  banda `primary-container` plena). Ver "Aplicación de color en listados y formularios" en
  `.agents/context/branding-ux-ui.md` antes de agregar cualquier color nuevo a un listado o formulario.
- **Modo oscuro**: la app tiene tema claro/oscuro (servicio `Tema` en `shared/services/`, toggle en
  la toolbar del `Shell`). Un componente nuevo sale correcto en ambos temas **solo si** usa
  `var(--mat-sys-*)` / `var(--opt-*)` — nunca un hex suelto ni `#fff`/`#000` literales. No hace
  falta escribir reglas dark por componente. Ver "Modo oscuro" en `.agents/context/branding-ux-ui.md`.
- **Escalas de sistema**: usar `var(--opt-space-*)`, `var(--opt-radius-*)`, `var(--opt-elevation-*)`,
  `var(--opt-motion-*)` de `theme-tokens.scss` en vez de píxeles/ms sueltos. Hay un reset global de
  `prefers-reduced-motion` en `styles.scss` — no re-implementarlo por componente.
- **Encabezado de página**: usar `shared/components/page-header/` (`app-page-header`) — nunca un
  `<div class="encabezado">` propio. Inputs `title`/`subtitle?`/`backTo?`/`backLabel`; la acción
  primaria va como `<ng-content>`. La tipografía de `h1/h2/h3` "desnudos" ya está en `styles.scss`
  (escala v2.1: 24 / 19 / 15px, peso 600) — no estilar encabezados por feature.
- **Estado de carga de un listado**: `shared/components/list-skeleton/` (`app-list-skeleton`,
  inputs `rows`/`header`) — no un `<mat-spinner>` suelto (salta el layout).
- **Acciones de fila**: >2 acciones → 1 primaria inline + `mat-menu` (`more_vert`) con el resto,
  usando `<ng-template matMenuContent>` + `[matMenuTriggerData]`. Todo `mat-icon-button` con
  `[attr.aria-label]`; la destructiva con la clase global `.opt-accion-destructiva`.
- **Montos**: siempre `{{ valor | pesos }}` (`shared/pipes/pesos-pipe.ts`) — nunca `currency`/`number`
  de Angular. El `CurrencyPipe` exigiría registrar el locale `es-CL` en el bundle inicial y sin eso
  imprime `CLP12,578` (separador equivocado para Chile); el pipe usa `Intl.NumberFormat` y devuelve
  `—` cuando no hay monto. Los montos van además con `.opt-mono` y alineados a la derecha.
- **Fechas hacia el backend**: `shared/utils/fechas.util.ts` (`aFechaIso` para `DateOnly`,
  `aFechaHoraIso` para `DateTimeOffset`, `aHoraIso`/`aHoraCorta` para `TimeOnly`) — nunca
  `toISOString()` a secas sobre lo que entrega el datepicker: corre la fecha un día en Chile.
- **Formularios**: `<mat-error>` por cada tipo de error de cada campo con validadores, `<mat-hint>`
  cuando aporte. Campo de contraseña → botón `matSuffix` mostrar/ocultar con signal `ocultarClave`.
  Fechas siempre con `DatePipe`, nunca el ISO crudo.
- **Diálogos de formulario — una sola columna** (sesión 2026-08-27): el panel por defecto de
  `MatDialog` es angosto (`--mat-dialog-container-max-width: 560px`); una grilla de 2 columnas
  dentro se desborda o queda apretada. Regla: `<mat-dialog-content>` con los campos apilados +
  `<p class="opt-form-section">` por grupo (clase global). Para 2 campos cortos lado a lado
  (Nombre/Apellido, Región/Comuna…), un `<div class="fila">` flex **por feature** (`flex-wrap` +
  `flex: 1 1 200px`, ver `cliente-form`). **No existe `.opt-form-grid`** — se retiró por frágil.
- **Ancho de un diálogo**: `:host` del `<entidad>-form` = `width: min(520px, 94vw)` (o `min(480px, 94vw)`
  para formularios cortos como empresa/usuario/sucursal) — nunca `min-width` fijo, nunca > 520px
  (desborda el panel de 560px → scroll horizontal, que fue el bug de `cliente-form`). `styles.scss`
  ya trae la red de seguridad `.mat-mdc-dialog-container .mat-mdc-dialog-content { overflow-x: hidden }` —
  no re-implementarla por feature ni tocar `MAT_DIALOG_DEFAULT_OPTIONS` en `app.config.ts` (metería
  `@angular/material/dialog` en el bundle inicial). Un confirm-dialog acota su ancho con `max-width`
  en su propio `:host`.

## Convención de archivos y nombres

- **2025 style guide de Angular** (el que usa `ng generate` en este proyecto — ver `angular.json`, no tiene `schematicCollections` alternativa): archivos concisos sin sufijo de tipo (`login.ts`, no `login.component.ts`; `auth.ts`, no `auth.service.ts`), clases sin sufijo (`export class Login`, no `LoginComponent`).
- **Nunca nombrar una clase igual a un identificador global del navegador o de una API web.** Ya pasó una vez en este proyecto: el servicio de notificaciones se llamaba `Notification` y colisionaba con `window.Notification`; se renombró a `Toast`. Antes de nombrar un servicio nuevo, verificar que el nombre no choque con una API global (`Location`, `History`, `Navigator`, `Notification`, `Storage`, etc.).
- Selectores de componente: prefijo `app-`, kebab-case (ya configurado en `eslint.config.js` — el lint falla si no se respeta).
- Generar archivos siempre con `ng generate` (`npx @angular/cli@21 g component features/<modulo>/pages/<pagina>`, `g service`, `g guard`, `g interceptor`) para heredar la convención de nombres y el registro automático de `styleUrl`/`templateUrl`. No crear archivos de componente/servicio a mano.
- Un feature nuevo con listado y rutas propias sigue la forma de `features/clientes/` (CRUD completo con paginación — usarlo como plantilla): `models/`, `services/`, `pages/<pagina>/`, `<modulo>.routes.ts` exportando `<MODULO>_ROUTES`.
- Un feature que solo se consume desde **otro** módulo (nunca tiene su propia página de listado ni entrada en `app.routes.ts`) omite `pages/<modulo>-list/` y `<modulo>.routes.ts` — solo `models/` + `services/` (+ `pages/<entidad>-form/` si además tiene alta/edición). Ejemplos: `features/anamnesis/` y `features/receta-cristales/` (su único punto de entrada es una pestaña de `ClienteFicha`, ver más abajo) y `features/regiones/`/`features/comunas/` (catálogos de solo lectura consumidos por el selector Región→Comuna de `ClienteForm`, sin ni siquiera `pages/`).
- **`ng generate service <feature>/services/<feature>` puede producir una clase con el mismo nombre que el modelo del feature** cuando el nombre del feature coincide con el de una entidad de dominio singular (pasó con `Anamnesis` y `RecetaCristales`, sesión 2026-08-26 — mismo problema que ya tiene `OPT.Application.Features.Anamnesis` en el backend, ver `CLAUDE.md` raíz). Resolverlo con un alias de tipo al importar el modelo en cualquier archivo que necesite ambos símbolos: `import { Anamnesis as AnamnesisModel } from '../../models/anamnesis.model'` — nunca renombrar la clase de servicio (rompe la convención de nombres sin sufijo de la sección anterior).

---

## Patrones de componente

- Standalone siempre (`standalone` es el default de la CLI en este proyecto — no pasar `standalone: false`).
- Inyección de dependencias con la función `inject()`, no con parámetros de constructor — es el patrón ya usado en todo el código existente (`private readonly auth = inject(Auth)`).
- Estado local: `signal()`. Estado derivado: `computed()`. No usar `BehaviorSubject` para estado de UI simple — RxJS queda reservado para flujos async reales (HTTP, eventos).
- Formularios: `ReactiveFormsModule` + `FormBuilder.nonNullable` (ver `features/auth/login/login.ts`) — nunca template-driven forms (`ngModel`) para formularios con validación.
- Control de flujo en templates: sintaxis nativa `@if` / `@for` / `@else` — nunca `*ngIf` / `*ngFor` (structural directives clásicas).
- Estilos con los tokens de sistema de Angular Material (`var(--mat-sys-on-surface)`, `var(--mat-sys-surface-container-low)`, etc. — ver `shell.scss`/`login.scss`) en vez de colores hardcodeados, para que el tema custom de `styles.scss` se propague solo.
- **Un provider de configuración usado por un solo feature va en el `providers: []` de ese
  componente, nunca en `app.config.ts`.** `app.config.ts` se carga eager (bundle inicial); un
  feature lazy que necesita `provideNativeDateAdapter()` (Datepicker) o un `MatPaginatorIntl` en
  español debe declararlo ahí mismo (ver `features/clientes/pages/cliente-form/cliente-form.ts` y
  `features/clientes/pages/clientes-list/clientes-list.ts`). Ponerlo en `app.config.ts` por error
  metió ~180kB de más en el bundle inicial y disparó el budget de `angular.json` en la sesión
  2026-08-26 — se corrigió moviéndolo al componente.

## CRUD simple: lista + diálogo, no rutas de alta/edición

Patrón usado por `features/sucursales|empresas|usuarios` (ver esos folders como plantilla) para
cualquier módulo cuyo alta/edición sea un formulario chico de una sola página:

- La página `<modulo>-list` es la única ruta del feature (`<modulo>.routes.ts` solo registra `''`).
  Alta y edición son un componente `<entidad>-form` abierto con `MatDialog` desde esa página, no
  rutas `/nuevo` o `/editar/:id`.
- El diálogo recibe sus datos vía `MAT_DIALOG_DATA` tipado (`export interface <Entidad>FormDialogData { entidad?: Entidad }`) — `undefined` implica alta, presente implica edición. El propio diálogo llama al servicio (crea o actualiza según corresponda) y hace `dialogRef.close(resultado)` al terminar; el listado solo reacciona a `afterClosed()` para refrescar y mostrar el toast de éxito.
- Eliminar (y cualquier confirmación destructiva, como activar/desactivar) usa
  `shared/components/confirm-dialog/confirm-dialog.ts` — no crear un diálogo de confirmación nuevo
  por feature.
- Un campo que solo aplica al crear (p. ej. `EsMatriz` de Sucursal, `Clave` de Usuario) se oculta del
  formulario en modo edición con `@if (!esEdicion)` — nunca inventar un endpoint de actualización
  para un campo que el backend no permite editar.

## Listado paginado + búsqueda (server-side) — patrón obligatorio para todo listado transaccional

Sesión 2026-08-27. Aplicado a `features/clientes|empresas|usuarios|sucursales-list`; **usarlo tal cual en todo listado nuevo** (OrdenesDeTrabajo, Inventario, …). Nada de traer la lista completa ni paginar/filtrar en memoria.

- **Estado**: `protected readonly estado = new EstadoListaPaginada<T>((p) => this.<servicio>.buscar(p));` (de `shared/utils/estado-lista-paginada.ts`), y `constructor() { this.estado.cargar(); }`. La clase expone las signals `items/total/pagina/tamanioPagina/busqueda/ordenarPor/direccionOrden/cargando/error/hayBusqueda` y los métodos `cargar()/refrescar()/buscar(texto)/cambiarPagina($event)/ordenar($event)`. No re-declarar esas signals en el componente. Tras un alta/edición/eliminación exitosa: `this.estado.refrescar()` (nunca `cargar()` a mano).
- **Servicio**: método `buscar(p: ParametrosConsultaPaginada): Observable<PagedResult<T>>` que hace `this.http.get<PagedResult<T>>(this.baseUrl, { params: aHttpParams(p) })` (`aHttpParams` en `core/utils/paginacion.util.ts` omite `busqueda`/`ordenarPor` vacíos). Coincide con `OPT.Domain/Common/ParametrosPaginacion` + `PagedResult<T>` del backend: query params `pagina`, `tamanioPagina`, `busqueda`, `ordenarPor`, `direccionOrden`.
- **Búsqueda tipo Google**: `<app-search-box (buscar)="estado.buscar($event)" [valorInicial]="estado.busqueda()" etiqueta="Buscar por …" />` (de `shared/components/search-box/`) — un solo input, debounced, con botón limpiar. **Nunca** varios `mat-form-field` de filtro por columna.
- **Paginador**: `<mat-paginator [length]="estado.total()" [pageSize]="estado.tamanioPagina()" [pageIndex]="estado.pagina() - 1" [pageSizeOptions]="opcionesTamanioPagina" (page)="estado.cambiarPagina($event)" />` con `opcionesTamanioPagina = OPCIONES_TAMANIO_PAGINA` (de `shared/models/parametros-consulta-paginada.model.ts`) y el provider `{ provide: MatPaginatorIntl, useFactory: crearMatPaginatorIntlEs }` en el componente (no en `app.config.ts`).
- **Orden**: `<table mat-table matSort (matSortChange)="estado.ordenar($event)">` + `mat-sort-header` **solo** en las columnas que el backend acepta en su lista blanca `_orden` (si no está, el orden cae al de por defecto sin error, pero la cabecera clickeable sería engañosa — no marcarla).
- El `EstadoListaPaginada` usa `switchMap` (cancela la petición previa) y `takeUntilDestroyed()` — por eso **debe** instanciarse en el field initializer / constructor (contexto de inyección).

## Estados de listado: vacío y error

Patrón agregado en la sesión de mejora de UX/UI 2026-08-26, aplicado a `features/sucursales|empresas|usuarios|roles|clientes` y a las pestañas de `cliente-ficha` — usarlo en todo listado nuevo:

- Un listado transaccional usa `EstadoListaPaginada` (sección anterior), que ya trae las signals `cargando`/`error`/`items`/`hayBusqueda` y el `refrescar()` para el botón "Reintentar". Un catálogo de lista completa (Roles) mantiene sus propias signals `cargando`/`error` + método `cargar()` `protected`.
- Bloque `@if` en el template, siempre en este orden: `cargando()` (`app-list-skeleton`) → `error()` (`app-empty-state` con `icon="error_outline"`, `tone="error"`, mensaje genérico de conexión, botón "Reintentar" → `estado.refrescar()`) → sin resultados de búsqueda (`estado.items().length === 0 && estado.hayBusqueda()` → `app-empty-state` `icon="search_off"`) → lista vacía real (`app-empty-state` con ícono/mensaje del dominio + botón de alta) → tabla con datos + `<mat-paginator>`.
- **No fusionar el estado de error con el estado vacío** — un fallo de red no debe verse igual que una lista real sin filas; antes de este patrón un error HTTP dejaba el listado en el mismo estado visual que "no hay datos", lo que ocultaba fallos reales de conexión.
- Un catálogo de solo lectura sin acción de alta (como `roles-list`) usa `app-empty-state` sin contenido proyectado — el componente no exige una acción.

## Módulo Comercial (OT, Abonos, Pagos, Cuotas, Cobranza) — sesión 2026-08-27

Cinco features que comparten un solo agregado del backend. Antes de tocar cualquiera de ellos:

- **El agregado vive en `features/ordenes-de-trabajo/`**: modelos (`orden-de-trabajo.model.ts`,
  `catalogos-comercial.model.ts`), servicios (`ordenes-de-trabajo.ts`, `catalogos-comercial.ts`) y
  los componentes compartidos (`components/estado-ot-chip`, `components/resumen-financiero`,
  `components/selector-orden`). Abonos, Pagos, Cuotas y Cobranza **importan de ahí** — no duplicar
  el modelo ni crear un servicio propio: en la API no existe `/api/pagos` ni `/api/cuotas`, son
  subrecursos de la OT (ADR `0007`).
- **Todo comando del agregado devuelve la OT completa ya recalculada** (precio, totalAbonado, saldo,
  estado, colecciones). Tras registrar un abono/pago/cuota o cambiar el estado: `this.orden.set(respuesta)`.
  **Nunca** volver a llamar a `obtener()` para refrescar, y nunca recalcular saldos en el frontend.
- **Flujo de estados** (`orden-de-trabajo-ficha`): solo se ofrece avanzar o retroceder **una** etapa,
  que es lo único que acepta `OrdenDeTrabajo.CambiarEstado`; el legacy dejaba elegir cualquier estado
  de un `<select>`. Retroceder exige observación y anular exige motivo → `shared/components/motivo-dialog/`
  (`MotivoDialog`, confirmación *con texto obligatorio*), no `ConfirmDialog`. Qué estado es terminal lo
  dice `esTerminal` del backend, no una constante del frontend; `ESTADOS_OT`/`ESTADOS_CUOTA` solo sirven
  para navegar el flujo (siguiente/anterior) y reconocer ANULADO/PENDIENTE.
- **Elegir la OT**: `<app-selector-orden [soloConSaldo]="…" (seleccionar)="…" />`. Es el primer paso de
  Abonos/Pagos/Cuotas, y esas pantallas también aceptan la OT ya elegida en `?ot=<publicId>` (así enlaza
  la ficha). No escribir otro buscador de OT.
- **Chip de estado**: `<app-estado-ot-chip [estadoId] [nombre] />` — el nombre lo manda el backend, no se
  traduce ni se reinterpreta acá.
- El listado de OT acepta filtros de contexto por query param (`clientePublicId`, `empresaPublicId`,
  `soloConSaldo`): es la vuelta desde la ficha de un cliente y desde Cobranza, que no tiene endpoint de
  detalle propio.
- **Excepción al patrón "CRUD simple = diálogo"**: el alta/edición de la OT es una **página ruteada**
  (`/ordenes-de-trabajo/nueva` y `/:publicId/editar`) porque incluye la tabla de detalle, que no cabe en
  el panel de 560px de `MatDialog`. En `ordenes-de-trabajo.routes.ts`, `nueva` va **antes** de `:publicId`.
  Es la única excepción — no extenderla a otros módulos sin la misma razón.
- El formulario de OT **no toma la receta, la vincula** (sesión 2026-08-28): la prescripción se registra
  en la ficha clínica del cliente, y el formulario solo ofrece elegir una de las que ese cliente ya tiene
  (`recetaPublicId`). En **edición** hay que reenviar la receta actual: el backend trata `recetaPublicId`
  como estado final —igual que `empresaPublicId`— y omitirla desvincula la receta. Lo que el formulario
  sigue sin pedir es el N° de OT y el precio (los genera/calcula el backend): no "restaurarlos" para
  parecerse al legacy. **Al crear** (sesión 2026-09-08, ADR `0010`), el paso Receta preselecciona
  automáticamente la más reciente del cliente dentro de los últimos 3 meses (contados desde hoy, no
  desde la fecha de atención) vía el `computed()` `recetasRecientes`; el `mat-radio-group` con el
  historial completo queda detrás de un botón "Ver historial completo" (`mostrarHistorialReceta`), no
  desaparece — sigue siendo necesario cuando la receta que corresponde no es la más nueva.
- **Cabecera de la OT en el paso Cliente, no en Detalle** (ADR `0010`): `fechaAtencion` (default hoy),
  `fechaEntrega`, `horaEntrega`, `empresa` y `beneficiario` viven en `formCliente`, no en `formOrden` ni
  repartidos en Detalle como antes. El N° de OT sigue sin ser un campo — lo genera la base de datos.
- **Tres modalidades de pago, no una casilla** (ADR `0010`): `formPago.modalidadPago` es
  `'total' | 'abonoCuotas' | 'cuotas' | null` — **nulo por defecto y `Validators.required`**, nunca un
  valor por defecto "útil": forzar `'total'` de entrada habría dejado el saldo en 0 antes de que el
  usuario eligiera nada, rompiendo la regla de que un saldo sin decisión debe bloquear el guardado (ver
  el spec `exige un plan de cuotas...`). Un único `effect()` en el constructor sincroniza qué controles
  de `formPago` están habilitados/deshabilitados y sus valores según la modalidad elegida — no repetir
  esa lógica en el template ni en `guardar()`. `cuotasPreview` (un `computed()`) replica en el frontend
  la fórmula exacta de `OrdenDeTrabajo.GenerarPlanCuotas` del backend (monto truncado a 2 decimales por
  cuota, el resto de redondeo va en la última, vencimientos mensuales con `setMonth`) solo para
  mostrarla antes de guardar — el backend sigue siendo quien genera el plan real al persistir la OT.
- **La ficha de la OT replica el modal "Detalle Orden de Trabajo" del legacy**: cabecera de la orden
  arriba (a todo el ancho, campos de solo lectura) y pestañas debajo — Cliente / Receta / Detalle /
  Abonos son las del legacy (ojo: el legacy llamaba "Lentes" a Detalle, nombre descartado por el
  usuario), y Pagos / Cuotas / Bitácora son propias del sistema nuevo. La pestaña Cliente y la de
  Receta se pintan con lo que ya trae `OrdenDeTrabajoDto` (`cliente`, `recetas`) — **no** hacer una
  llamada aparte a `/api/clientes/{publicId}` ni a `/api/recetacristales`.
- **El listado de OT no consulta al entrar** — son más de 12.000 órdenes y en el legacy traerlas daba
  timeout. La pantalla arranca en un `app-empty-state` "Busca una orden de trabajo" (signal `consultado`)
  y recién consulta con la primera búsqueda o filtro; llegar con un filtro de contexto en la URL sí
  dispara la carga, porque ya es un criterio. Quitar el contexto vuelve al estado inicial, no trae todo.
  Aplicar el mismo patrón a cualquier listado sobre una tabla de este volumen.

## Ficha ruteada — cuando el diálogo no alcanza

Excepción al patrón anterior, usada por `features/clientes/pages/cliente-ficha/` (sesión
2026-08-26, rediseñada 2026-08-27): si una entidad tiene **más de un historial de sub-recursos**
que mostrar a la vez (Cliente → historial de Anamnesis + historial de RecetaCristales), ese
contenido no cabe en un diálogo chico — se usa una página ruteada (`<entidad>.routes.ts` agrega un
hijo `:publicId` además de `''`). Layout tipo **"ficha clínica"** (mismo patrón que la Ficha
Paciente del legacy): panel de identidad de la entidad **fijo** a la izquierda
(`position: sticky`, `grid-template-columns: 300px 1fr`, colapsa a 1 columna bajo 900px) +
`MatTabsModule` a la derecha, una pestaña por sub-recurso (con contador en la etiqueta). El panel
izquierdo lleva el `.opt-badge-clinico`, los datos clave en `<dl>`, el resumen ("N anamnesis · M
recetas") y el botón "Editar datos" (reabre el `<Entidad>Form` del listado). Reglas:

- Es la única ruta de detalle con parámetro permitida — no crear rutas `/nuevo` o `/editar/:id`
  para el CRUD simple de la entidad principal, que sigue siendo diálogo.
- Cada pestaña de sub-recurso es autónoma: barra superior con conteo + botón "Nueva…", tarjetas de
  historial **ordenadas de la más reciente a la más antigua** (por `fechaRegistro`, que el backend
  expone desde el audit `CreadoEn` — en datos migrados = la fecha real del legacy), cada una con su
  fecha visible y sus acciones editar/eliminar (`ConfirmDialog`), y su propio estado vacío/error
  (`app-empty-state` + reintentar). No se comparte estado entre pestañas más allá del `publicId`.
- Si esto es lo que se necesita para reemplazar una pantalla del legacy que combinaba varias
  entidades en un solo formulario (como `Atencion` del legacy, que mezclaba Cliente + Anamnesis +
  Receta), **no inventar una entidad nueva en el backend para calzar con el legacy** — verificar
  primero si el esquema nuevo ya decidió separarlas (ver `CLAUDE.md` raíz) antes de construir el
  patrón "ficha con pestañas" u otro equivalente.
- El componente usa `input.required<string>()` para el parámetro de ruta (requiere
  `withComponentInputBinding()`, ya activado en `app.routes.ts`) — en el `.spec.ts`, fijar el valor
  con `fixture.componentRef.setInput('publicId', '...')` antes de `fixture.whenStable()`.

## RecetaCristalesForm — diálogo con tablas anchas (sesión 2026-09-08)

`features/receta-cristales/pages/receta-cristales-form/` recibió una pasada de UX/UI a pedido del
usuario, con dos patrones reusables para cualquier diálogo que tenga que mostrar una tabla ancha o
un grupo de campos que se activa/desactiva como bloque:

- **Diálogo grande = ancho explícito en cada `.open()`, no solo en el `:host` del componente.**
  El CDK de Material limita el panel a `80vw` por defecto aunque el `:host` pida más — hay que
  pasar `width`/`maxWidth` en la config de cada llamada a `dialog.open(...)` (las 3 que abren este
  diálogo: `cliente-ficha.ts` ×2, `orden-de-trabajo-form.ts`), coordinado con el mismo valor en el
  `:host { width: …; max-width: 96vw }` del componente. Sin el segundo, el diálogo se ve angosto
  aunque el componente esté preparado para más ancho.
- **Checkbox como título de sección, no como fila aparte.** Cuando un checkbox activa/desactiva un
  bloque completo de campos (el patrón "Incluir X" ya existía en `alternarLejos`/`alternarCerca`),
  ponerlo en la `<caption>`/encabezado de ese bloque en vez de agruparlo con checks no relacionados
  arriba — el checkbox pasa a ser el título del bloque, no una opción más en una lista. Acompañarlo
  de un `toSignal(control.valueChanges, { initialValue: control.value })` para atenuar
  (`opacity`/`border`) el bloque completo mientras está desactivado — más notorio que confiar solo
  en el gris nativo de los inputs deshabilitados. Ver `incluirLejosActivo`/`incluirCercaActivo` y
  las clases `.tabla-receta--activa`/`.check-bloque`.
- Ese mismo diálogo también volvió numéricos los campos DP y ADD (antes texto libre): la API sigue
  recibiendo `string` (sin cambio de esquema — ver `CrearRecetaCristalesCommand`), la conversión
  número↔texto pasa solo al guardar/cargar. Riesgo aceptado: una receta migrada con un DP en
  formato no numérico (p. ej. "31/30") se ve vacía en ese campo al editarla.

## Servicios y acceso a datos

- Los componentes **nunca** inyectan `HttpClient` directamente — siempre a través de un servicio de `features/<modulo>/services/` (regla ya presente en `AGENTS.md`/`CLAUDE.md` raíz, reforzada aquí). El servicio expone métodos con nombres en español (`listar`, `obtener`, `crear`, `actualizar`, `eliminar`), no genéricos (`getAll`, `get`).
- Base URL siempre desde `environment.apiUrl` (`src/environments/`) — nunca hardcodear `http://localhost:...` en un servicio.
- El manejo de errores HTTP genérico (toast + logout en 401) ya lo hace `errorInterceptor` — no envolver cada llamada en un `catchError` a menos que el componente necesite una reacción específica (p. ej. desactivar un spinner de envío, mostrar un error de campo).

## Rutas

- Todo feature se carga con `loadComponent` (página única) o `loadChildren` apuntando a su `<modulo>.routes.ts` (feature con varias páginas) — nunca importado de forma eager en `app.routes.ts`.
- Rutas protegidas van bajo el `Shell` (`canActivate: [authGuard]`); la única ruta pública es `/login` bajo `AuthLayout`.

## Testing

Todo componente/servicio/guard/interceptor generado con `ng generate` trae su `.spec.ts` — completarlo, no borrarlo. Providers mínimos según lo que inyecte la pieza bajo prueba (ya resueltos así en el código existente, usar como receta):

| La pieza inyecta... | Providers a agregar en `TestBed.configureTestingModule` |
|---|---|
| `Router`, `RouterLink`, `RouterOutlet`, o cualquier servicio que a su vez inyecte `Router`/`ActivatedRoute` | `provideRouter([])` |
| `HttpClient` (directo o vía un servicio) | `provideHttpClient()` — si el test no debe hacer una llamada real, sumar `provideHttpClientTesting()` |
| `MatDialogRef`/`MAT_DIALOG_DATA` (componente abierto como diálogo) | `{ provide: MatDialogRef, useValue: { close: () => undefined } }` + `{ provide: MAT_DIALOG_DATA, useValue: {...} }` — ver `sucursal-form.spec.ts` como receta. `close: () => {}` viola `no-empty-function` del lint, usar `() => undefined`. |

Correr `npm test` antes de dar por terminado un cambio — no basta con que compile.

## Asistentes por pasos e impresión

Dos patrones que introdujo el alta de una Orden de Trabajo (sesión 2026-08-28, 2ª) y que hay que
reusar en vez de reinventar:

- **Alta larga = `mat-stepper`, no una página de scroll único.** `orden-de-trabajo-form` es el
  ejemplo: un `FormGroup` por paso (`formCliente` / `formOrden` / `formPago`) enlazado con
  `[stepControl]`, lineal al crear y no lineal al editar. Si un paso depende de estado que vive en
  un signal (el detalle de la OT), agregar al `FormGroup` un control espejo con
  `Validators.requiredTrue` y sincronizarlo desde un `effect` — `mat-step` valida por control, no
  por signal.
- **Crear una entidad relacionada sin salir de la pantalla**: abrir su `MatDialog` ya existente
  (`ClienteForm`, `RecetaCristalesForm`) y consumir el resultado de `afterClosed()`. No duplicar
  sus campos ni sus validaciones dentro del formulario anfitrión.
- **Impresión**: las reglas viven en el bloque `@media print` de `styles.scss`, nunca en el SCSS del
  componente (los estilos encapsulados no pueden ocultar el resto de la app). Quien imprime agrega
  `opt-imprimiendo` al `<body>` mientras dura el `window.print()` y lo quita después; lo que no debe
  salir en papel se marca con `.opt-no-imprimir`. El comprobante de una OT es
  `features/ordenes-de-trabajo/components/ticket-ot/` (`app-ticket-ot`, input `orden`) — reemplaza al
  reporte `.rdlc` del legacy y se reusa donde haga falta reimprimirlo.

## Comandos

```bash
cd src/frontend
npm start            # ng serve
npm run build         # build de producción (respeta budgets en angular.json)
npm test              # Vitest
npm run lint           # ESLint
npm run format          # Prettier — aplica
npm run format:check     # Prettier — solo verifica
```

Antes de considerar terminado un cambio: `npm run lint` y `npm test` en verde, y `npm run build` sin warnings de budget nuevos.

## Documentación relacionada

| Documento | Contenido |
|---|---|
| `src/frontend/README.md` | Stack, estructura de carpetas, cómo correr el proyecto, decisiones específicas del frontend. |
| `src/documentos/Manual_Tecnico_Frontend_OPT.docx` | Manual técnico extendido del frontend (para desarrolladores humanos). |
| `.agents/decisions/0002-frontend-angular.md` | ADR — Angular sobre Blazor. |
| `.agents/decisions/0004-cumplimiento-ley-21719.md` | ADR — `PublicId`, datos sensibles (aplica también al frontend: nunca exponer el `id` interno). |
| `CLAUDE.md` / `AGENTS.md` (raíz) | Reglas globales del proyecto — dominio de negocio, reglas críticas, convenciones de commits. |
