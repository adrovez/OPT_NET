# Branding y contexto UX/UI — OPT

Contexto de identidad de marca y sistema visual para cualquier agente IA que genere interfaz (componentes Angular/Angular Material), copy de producto, correos transaccionales o material de marca para OPT. Es el resumen accionable en texto plano de `src/documentos/Manual_Tecnico_UX_OPT.docx` (la propuesta completa, con logotipo, swatches y mockups de pantalla en imagen). Ante cualquier duda de detalle visual, `Manual_Tecnico_UX_OPT.docx` es la fuente completa; este archivo es la fuente rápida para no tener que abrirlo cada vez.

**Estado: propuesta v2.0, aplicada al código (sesión 2026-08-27), no validada aún con stakeholders** (dueños de óptica, equipo de atención). Reemplaza a la v1.0 (paleta teal/ámbar, Manrope/Inter, tagline "Visión clara, gestión clara") — el usuario pidió explícitamente una evolución de paleta, tipografía y tono porque v1.0 "no convencía del todo". Es la mejor línea base disponible hoy — úsala por defecto para todo módulo nuevo — pero no la trates como decisión irreversible: si el equipo humano la corrige, actualiza este archivo, el código (`theme-colors.scss`/`theme-tokens.scss`/`styles.scss`) y el `.docx` en la misma sesión.

**2ª pasada UX/UI (sesión 2026-08-27, misma fecha, posterior):** sobre esa base v2.0 ya aplicada se hizo una revisión de acabado que agregó: modo oscuro completo (toggle + preferencia de sistema), escalas de sistema en tokens (espaciado/radio/elevación/movimiento), tipografía global de encabezados, componentes compartidos `app-page-header` y `app-list-skeleton`, patrón de menú de overflow para acciones de fila, `mat-error`/`mat-hint` y toggles de visibilidad de clave en todos los formularios, `.opt-mono` aplicado a los listados, y `prefers-reduced-motion`. No cambió ningún color, tipografía ni tono de v2.0 — solo estructura, accesibilidad y consistencia. Ver secciones "Modo oscuro", "Escalas de sistema", "Componentes compartidos de layout" y "Formularios" más abajo, y la entrada 2026-08-27 (2ª pasada) de `.agents/progress.md`.

**3ª pasada UX/UI — Propuesta v2.1: densidad y tipografía (sesión 2026-08-27, misma fecha, posterior):** el usuario reportó que "la fuente es muy grande, en especial los input". Diagnóstico: la causa raíz eran dos decisiones globales, no `font-size` sueltos — `density: 0` en `mat.theme()` (calibración táctil de M3) + la escala tipográfica M3 por defecto (texto de input = rol `body-large` = 16px). Se aplicó **`density: -2` global** y una **escala tipográfica compacta** (override de tokens `--mat-sys-*` tras `mat.theme()`), más una excepción móvil (input a 16px bajo 600px para no disparar el auto-zoom de iOS), encabezados un escalón más bajos, y ajustes de espaciado. También se aplicaron los toques de marca de baja prioridad: cabecera de `mat-table` liviana y un guiño humanista (Fraunces + terracota) acotado al `app-empty-state`. **No cambió paleta, familias tipográficas, tagline, logo ni tono.** Ver sección "Densidad y escala tipográfica compacta (v2.1)" más abajo, la guía HTML `src/Guia_de_estilo/propuesta-densidad-tipografia.html`, y la entrada 2026-08-27 (Propuesta v2.1) de `.agents/progress.md`.

## Paleta de colores (v2.0 — "Añil + Terracota")

Roles, no nombres libres — pensada para cargar directo como tema Angular Material 3 en `src/frontend`. Reemplaza la paleta "Iris teal + Ámbar lente" de v1.0.

| Token | Hex | Rol |
|---|---|---|
| `primary-dark` | `#1B3A66` | Headers, texto sobre fondos claros, estado pressed |
| `primary` | `#2F5D8A` | Color de marca — botones primarios, links, foco de formularios, navegación |
| `primary-light` | `#5B87B3` | Hover, iconografía activa |
| `primary-container` | `#DCE8F5` | Fondos suaves, chips claros |
| `accent` | `#E2703B` | CTA secundario puntual (ej. "Nueva OT"). Uso deliberadamente escaso — pierde valor si se satura la interfaz |
| `accent-dark` | `#A8471E` | Texto/ícono sobre fondos con acento claro |
| `accent-container` | `#FCE3D2` | Fondos de alerta suave |
| `surface` | `#F7F9FB` | Fondo de aplicación |
| `surface-dim` | `#E8ECF1` | Fondos alternos, franjas de tabla |
| `outline` | `#C7CFD9` | Bordes, separadores |
| `on-surface` | `#1A2230` | Texto principal |
| `on-surface-variant` | `#55606F` | Texto secundario/metadatos |

No introducir colores nuevos fuera de esta tabla sin verificar antes su contraste (ver sección Accesibilidad) y sin agregarlos aquí. La razón del cambio de v1.0 a v2.0: un azul añil más profundo se aleja del teal genérico que domina casi todo SaaS de salud/óptica, sin sacrificar el ecosistema Material ni los ratios de contraste ya exigidos.

## Color con significado: catálogos reales del dominio

No uses semáforo genérico (verde/ámbar/rojo) para estos dos catálogos — ya tienen una resolución de color definida y verificada:

### `OPT_EstadoOT` (8 valores, en orden de `Id` — proceso lineal, no evaluación de éxito/fracaso)

| Id | Nombre | Hex fondo | Color de texto |
|---|---|---|---|
| 0 | INGRESADO | `#DCE8F5` | oscuro (`on-surface`) |
| 1 | EN PROCESO | `#B7D0EA` | oscuro |
| 2 | MONTAJE | `#8FB4DA` | oscuro |
| 3 | LABORATORIO | `#5F8FC0` | oscuro |
| 4 | CALIDAD | `#3A6EA0` | blanco |
| 5 | DESPACHO | `#1E4A78` | blanco |
| 6 | ENTREGADO | `#1F8A4C` | blanco |
| 7 | ANULADO | `#D6D3D1` | `#3F3A37` (7,5:1) |

`ANULADO` (id 7) se agregó en la sesión 2026-08-27, al construir el frontend del módulo Comercial:
no existe en el legacy — lo introduce el script `005` como segundo estado terminal, para modelar
como estado lo que el legacy resolvía borrando la OT. Queda **fuera de la rampa añil a propósito**
(no es una etapa del proceso sino su interrupción) y en **gris neutro, nunca en rojo**: anular una
orden es una operación legítima del mesón, no un error. En el chip se acompaña de tachado
(`text-decoration: line-through`), así el estado no depende solo del color. Contraste 7,5:1 — AA y
AAA para texto normal.

La rampa es de un solo matiz (más clara al ingresar, más intensa al avanzar); el verde se reserva solo para el cierre exitoso (`ENTREGADO`, sin cambios respecto a v1.0). El corte oscuro/blanco de texto se movió a `CALIDAD` (índice 4) — un escalón antes que en v1.0, donde ocurría en `DESPACHO` — porque el nuevo azul alcanza la luminosidad crítica un paso antes (ver Accesibilidad). No desplazar sin recalcular.

### `OPT_FormaPago` (5 valores)

Chips neutros, sin jerarquía de color: `primary-container` de fondo + `primary-dark` de texto para los 4 medios reales (EFECTIVO, TARJETA CREDITO, TARJETA DEBITO, TRANSFERENCIA); `SIN INFORMACION` en gris apagado (`surface-dim` / `on-surface-variant`) para diferenciarlo visualmente como caso incompleto. Sin cambios respecto a v1.0 — solo cambia el hex subyacente de `primary-container`/`primary-dark`.

## Tipografía (v2.0)

| Rol | Familia | Fallback de sistema | Uso |
|---|---|---|---|
| Titulares / marca | **Fraunces** (500/600/700) | Georgia, serif | Portada, login, material de marketing |
| Interfaz y cuerpo | **IBM Plex Sans** (400/500/600/700) | Segoe UI, sans-serif | Toda la aplicación — ya configurado como `typography` del tema Material, reemplaza a Roboto |
| Datos numéricos / códigos | **IBM Plex Mono** (400/500) | Consolas, monospace | Folios de OT, RUT, montos, fechas en tablas densas — clase utilitaria `.opt-mono` en `styles.scss` |

Reemplaza a Manrope/Poppins + Inter/Roboto de v1.0 — elegido para tener menos "cara de SaaS genérico" mientras se mantiene legibilidad y buen soporte de tildes/ñ. Las 3 familias se cargan vía Google Fonts en `src/index.html`.

**Tamaños (v2.1, sesión 2026-08-27):** las familias y pesos de arriba no cambiaron, pero los **tamaños** de la escala se recalibraron para una interfaz de trabajo de escritorio (M3 por defecto es táctil/móvil). Ver "Densidad y escala tipográfica compacta (v2.1)" más abajo para la tabla completa. Resumen: texto de input 16→14px (escritorio; vuelve a 16px bajo 600px), cuerpo/celdas 14→13.5px, hint/error 12→11.5px, texto de botón 14→13px, título de diálogo 22→19px, y encabezados de página H1 27→24 / H2 21→19 / H3 16.5→15.

## Logotipo (concepto — sin cambios de forma, cambia el color)

Isotipo: un anillo (lente óptica) con un punto de luz/glint en el cuadrante superior derecho — funciona como monograma de la "O" de OPT. El concepto no fue objetado al proponer v2.0, solo se recolorea con la paleta nueva (`primary`/`accent` en vez de los hex de v1.0). Lockup completo = isotipo + wordmark "OPT" (tipografía Fraunces) + subtítulo "GESTIÓN DE ÓPTICAS". Sigue sin existir un vector final de producción — ver sección "Pendiente".

Reglas duras: círculo perfecto (nunca deformado ni rotado), colores solo de la paleta de arriba, área de protección mínima = radio del anillo, tamaño mínimo del lockup 120px / isotipo solo 24px.

## Naming y tagline (v2.0)

Marca: **OPT** (se conserva — ya es nombre de proyecto y prefijo de tablas `OPT_`, no fue objetado). Tagline: **"Todo en foco."** — reemplaza a "Visión clara, gestión clara." (v1.0, considerado genérico de rubro salud). Juega con el lente óptico y con tener el negocio bajo control; uso en login/portada/marketing, nunca dentro de la interfaz de trabajo diario. Ya aplicado en `features/auth/login/login.html` y `layout/auth-layout/auth-layout.html`.

## Voz y tono

| Contexto | Tono | Ejemplo |
|---|---|---|
| Interfaz interna (staff) | Directo, cercano, tuteo | "Guarda los cambios antes de salir" |
| Comunicación al cliente (boletas, correos) | Formal, cordial, usted | "Su orden de trabajo está lista para retiro" |
| Mensajes de error | Sin culpar al usuario, con salida clara | "No pudimos guardar la receta. Intenta de nuevo" |
| Datos clínicos/sensibles | Explícito sobre privacidad | "Esta información es visible solo para personal autorizado" |

Sin cambios respecto a v1.0 — la tabla de voz y tono no fue objetada. Evitar en cualquier tono: jerga corporativa vacía ("sinergia", "revolucionario", "best-in-class").

## Accesibilidad — regla de trabajo obligatoria

**Ningún par texto/fondo se aprueba por percepción visual — siempre calcular el ratio de contraste WCAG 2.1 antes de usarlo.** Fórmula de luminancia relativa estándar (coeficientes 0.2126/0.7152/0.0722 sobre RGB linealizado). Mínimos: 4.5:1 texto normal AA, 3:1 texto grande (≥14pt bold / ≥18pt regular) AA, 7:1 AAA.

Pares recalculados para v2.0 y aprobados (no repetir el cálculo, ya está hecho):

| Combinación | Ratio | Resultado |
|---|---|---|
| `on-surface` sobre `surface` | 15.1:1 | AAA |
| `on-surface-variant` sobre `surface` | 6.0:1 | AA |
| Blanco sobre `primary` | 6.9:1 | AA |
| Blanco sobre `primary-dark` | 11.4:1 | AAA |
| Oscuro (`on-surface`) sobre `accent` | 5.0:1 | AA |
| `accent-dark` sobre `accent-container` | 4.76:1 | AA |
| Oscuro sobre chips INGRESADO/EN PROCESO/MONTAJE/LABORATORIO | 4.7–15.1:1 | AA/AAA |
| Blanco sobre chips CALIDAD/DESPACHO | 5.35–9.1:1 | AA/AAA |
| Blanco sobre ENTREGADO | 4.4:1 | AA texto normal |

Adicional, sin cambios: el estado nunca se comunica solo con color — siempre color + texto o color + ícono (accesibilidad para daltonismo).

## Componentes UI — lineamientos

- Radio de esquina generoso (pill/rounded) en botones y chips.
- Densidad Material **`-2`** (v2.1) — no volver a `density: 0` (calibración táctil, infla inputs/filas). Un componente nuevo hereda la densidad del `mat.theme()`, no necesita ajuste propio.
- Tipografía de titular (Fraunces, `.opt-titular`): solo en login/portada/marketing. **Única excepción dentro de la app:** el título de `app-empty-state` (ver "Densidad y escala tipográfica compacta (v2.1)"). Nunca en tablas, formularios ni encabezados de página.
- Un solo botón primario relleno por vista; el resto, outline o texto.
- Foco de formulario siempre visible con `primary` + fondo `primary-container`, nunca solo borde de 1px (uso en mesón con luz variable).
- Toda tarjeta/fila que represente una OT muestra el estado como chip (fondo + texto), nunca solo color de fondo de tarjeta ni solo texto plano.
- Set de iconos: Material Symbols, variante Rounded, peso 400 (700 solo para estados activos/alerta), grilla 24×24, área de toque táctil mínima 40×40.
- Estado vacío y estado de error de un listado usan el mismo componente compartido (`app-empty-state`, ver sección "Patrones de UI agregados" más abajo) — nunca un `<p>` de texto suelto, y nunca el mismo ícono/tono para ambos casos.
- La navegación es un navbar horizontal en la barra superior (ver "Navegación horizontal" en "Patrones de UI agregados" más abajo) responsive por defecto en toda pantalla nueva — no asumir un ancho de viewport mínimo.
- Datos tabulares/numéricos (RUT, folios de OT, montos) usan la clase `.opt-mono` (IBM Plex Mono) — nueva en v2.0, aún no aplicada a los listados existentes (Clientes/Usuarios/Empresas muestran RUT sin esta clase); aplicar al tocar esas pantallas de nuevo, no requiere una sesión dedicada.

## Señal visual de dato sensible (Ley 21.719)

Toda pantalla que muestre Anamnesis o RecetaCristales debe llevar un badge persistente de "Información clínica" (mismo lenguaje visual que los chips de estado, color `primary-dark`). Esto es refuerzo perceptible de la protección que ya da `PublicId` a nivel de backend (ver ADR `0004` y regla de `PublicId` en `CLAUDE.md`) — no la reemplaza.

## Implementación en código (v2.0, sesión 2026-08-27 — reemplaza la de v1.0)

El tema Angular Material M3 y las piezas de marca ya están regeneradas en `src/frontend` con la
paleta/tipografía/tono de v2.0. Antes de tocar cualquiera de estos archivos, leer esta sección para
no reinventar dónde vive cada cosa:

| Elemento | Dónde vive | Notas |
|---|---|---|
| Paleta M3 (primary/tertiary) | `src/theme-colors.scss` | Regenerado a partir de `#2F5D8A`/`#E2703B` (algoritmo M3 de tonal palettes, `CorePalette.fromColors`) — no editar a mano, regenerar con `ng generate @angular/material:theme-color --primary-color=#2F5D8A --tertiary-color=#E2703B` si cambia un hex. |
| Tema aplicado (`mat.theme(...)`) | `src/styles.scss` | `theme-type: light`, tipografía base **`IBM Plex Sans`** (antes `Roboto`). |
| Tokens de marca sin equivalente M3 (`OPT_EstadoOT`, `OPT_FormaPago`, `primary-dark`, tipografía de titulares/mono) | `src/theme-tokens.scss` | Custom properties `--opt-*`, transcritas 1:1 de las tablas de este documento. Incluye el nuevo `--opt-font-mono`. Cualquier componente nuevo que necesite estos colores consume la variable CSS, nunca un hex suelto. |
| Foco de formulario reforzado (halo + color, no solo borde 1px) | `.mat-mdc-form-field.mat-focused` en `src/styles.scss` | Sin cambios respecto a v1.0. |
| Utilidad de titular/marca (`Fraunces`) | clase `.opt-titular` en `src/styles.scss`, fuente cargada en `src/index.html` | Antes usaba Manrope. Usar solo en login/portada/marketing — nunca en la interfaz de trabajo diario. |
| Utilidad de dato numérico/código (`IBM Plex Mono`) | clase `.opt-mono` en `src/styles.scss` | Nueva en v2.0 — aún sin consumidores (ver "Componentes UI"). |
| Utilidad de badge de dato sensible | clase `.opt-badge-clinico` en `src/styles.scss` | Sin cambios de estructura; el color (`--opt-primary-dark`) sigue el hex nuevo. |
| Set de iconos (Material Symbols Rounded, peso 400) | `MAT_ICON_DEFAULT_OPTIONS` en `src/app/app.config.ts` + fuente en `index.html` | Sin cambios. |
| Lockup de marca + tagline | `features/auth/login/login.html`/`.scss`, `layout/auth-layout/auth-layout.html` | Tagline actualizado a "Todo en foco." en ambos lugares. |

Verificación de esta sesión: se compiló `styles.scss` con `sass` de forma aislada (fuera de `ng build`,
que en este entorno de ejecución resultó impráctico por la lentitud de E/S de la carpeta montada del
usuario) y se confirmó que `--mat-sys-primary`, `--mat-sys-body-medium` y todos los `--opt-*` emiten
los valores esperados de v2.0. Se armó además una vista de referencia estática (login + tabla de OT +
rampa de estados) con esos mismos tokens para verificar contraste y tipografía a ojo antes de cerrar
la sesión — recomendable correr igual `npm run build`/`lint`/`test` localmente antes de dar por
cerrado el cambio, no se pudieron ejecutar en esta sesión.

Pendiente dentro de lo ya implementado: los chips de color de `OPT_EstadoOT`/`OPT_FormaPago` siguen
sin un componente que los consuma en pantalla real — `OrdenesDeTrabajoController` sigue siendo un stub
en el backend (ver `CLAUDE.md` raíz). Cuando se implemente esa pantalla, usar las variables de
`theme-tokens.scss` (`--opt-estado-ot-*`, `--opt-forma-pago-*`) en vez de recalcular los hex de
las tablas de arriba.

## Patrones de UI agregados (sesión 2026-08-26)

Mejora de UX/UI transversal sobre el frontend existente — no cambia con la evolución de paleta/tipografía de v2.0, agrega patrones de interacción/estructura que faltaban. Usar estos patrones en toda pantalla nueva en vez de reinventarlos:

| Patrón | Dónde vive | Cuándo usarlo |
|---|---|---|
| Estado vacío / estado de error de un listado | `shared/components/empty-state/` (`app-empty-state`) | Reemplaza el `<p class="vacio">` repetido. Inputs: `icon`, `title` (opcional), `message`, `tone` (`'neutral'` default o `'error'`). La acción (botón "Reintentar" o "Crear el primero") se proyecta como `<ng-content>`. Un listado siempre distingue "sin datos" (tone neutral) de "falló la carga" (tone error, con botón Reintentar) — nunca el mismo estado visual para ambos, porque un fallo de red no debe verse igual que una lista realmente vacía. |
| Login con panel de marca | `layout/auth-layout/` + `features/auth/login/` | En viewports ≥900px, `AuthLayout` muestra un panel lateral con degradado `primary` → `primary-dark` y un acento sutil (`--opt-accent`, radial en la esquina opuesta al texto, opacidad ≤0.35 para no ensuciar el color de marca) con el lockup + tagline + una línea de propuesta de valor. Por debajo de 900px el panel se oculta y el lockup/tagline reaparecen dentro de la card de login (nunca duplicados en ambos lugares a la vez — usar `@media` espejado a 900px en los dos archivos). |
| Scroll horizontal de tablas anchas | `layout/shell/shell.scss` (`.contenido { overflow-x: auto }`) | Ya aplica a toda pantalla bajo `Shell` — no agregar `overflow-x` por tabla individual. |
| Aviso de bloqueo | `orden-de-trabajo-ficha.html`/`.scss` (clase `.aviso--bloqueo`, sesión 2026-09-08, ADR `0010`) | Cuando una entidad no admite edición por su propio estado (OT `Entregado`/anulada), mostrar un aviso fijo con ícono (`block`/`lock`) y texto explicando por qué, no solo deshabilitar los controles en silencio. Reusar la clase antes de inventar una nueva para el mismo propósito en otra ficha. |
| Combo de opción reciente + historial colapsable | `orden-de-trabajo-form.ts`/`.html`, paso Receta (sesión 2026-09-08, ADR `0010`) | Cuando una lista tiene una opción "obvia" (la más reciente dentro de una ventana de tiempo) pero no siempre es la correcta, ofrecer un `mat-select` con esa opción preseleccionada y un botón "Ver historial completo" que revela el listado completo (`mat-radio-group` u otro) recién cuando hace falta — no mostrar ambos a la vez ni obligar a abrir el historial siempre. |
| Tarjetas de modalidad excluyente | `orden-de-trabajo-form.html`/`.scss`, paso Pago (`.modalidades`/`.modalidad`, sesión 2026-09-08, ADR `0010`) | Cuando el usuario debe elegir una de varias formas mutuamente excluyentes de completar un flujo (no una casilla que se puede olvidar marcar), usar un `mat-radio-group` de tarjetas con ícono + texto principal/secundario en vez de un `<select>` o checkboxes sueltos, y que cada opción habilite/deshabilite el resto del formulario según corresponda. |

Ver el resumen de la sesión completa (incluidas las capturas antes/después) en `.agents/progress.md`, entrada 2026-08-26 "Mejora de UX/UI del frontend", y la sección 11/12 actualizada de `Manual_Tecnico_UX_OPT.docx`. Los tres patrones agregados el 2026-09-08 están resumidos en la entrada correspondiente de `.agents/progress.md` y en el ADR `0010`.

| Checkbox como título de bloque | `receta-cristales-form.html`/`.ts` (clase `.check-bloque`, sesión 2026-09-08) | Cuando un checkbox activa/desactiva un bloque completo de campos, ponerlo en la `<caption>`/encabezado de ese bloque en vez de agruparlo con checks no relacionados en una fila aparte — pasa a ser el título del bloque. Acompañarlo de un signal (`toSignal(control.valueChanges, {{ initialValue: control.value }})`) que atenúe (`opacity`/`border`) el bloque completo mientras está desactivado, más notorio que el gris nativo de los inputs deshabilitados. |
| Ancho explícito en diálogos con tablas anchas | `receta-cristales-form.scss` (`:host { width: 960px }`) + `width`/`maxWidth` en cada `dialog.open(...)` (sesión 2026-09-08) | El CDK de Material limita el panel a `80vw` por defecto aunque el `:host` del componente pida más — hay que pasar el mismo ancho en la config de cada `.open()` que abre ese diálogo, no solo en su `:host`. |

## Navegación horizontal (sesión 2026-08-27, reemplaza el sidenav)

El sidenav lateral colapsable por grupo (`mat-sidenav` con acordeón) se reemplazó por un navbar
horizontal dentro del `mat-toolbar` — decisión explícita del usuario, no un ajuste de accesibilidad.
Vive enteramente en `layout/shell/shell.ts`/`.html`/`.scss`, ya no hay `mat-sidenav-container` en el
árbol de `Shell`. Estructural, no afectado por el cambio de paleta/tipografía de v2.0.

- **Desktop (≥960px)**: cada `GrupoNav` con un solo ítem se renderiza como link directo (`mat-button`
  + `routerLink`) en la barra; un grupo con más de un ítem (desde 2026-09-23 los 4: Administración, Comercial, Inventario y un Reportes de ítem único) se renderiza como
  botón con `[matMenuTriggerFor]` que abre un `mat-menu` (dropdown) con sus ítems. El template ref del
  `mat-menu` se declara dentro del mismo bloque `@for` — cada iteración obtiene su propia instancia,
  no hace falta un array paralelo de referencias.
- **Móvil (<960px)**: la barra colapsa a un único botón de hamburguesa que abre un `mat-menu` con
  todos los grupos; un grupo multi-ítem usa un **submenú anidado** (`mat-menu-item` con su propio
  `[matMenuTriggerFor]` apuntando a un segundo `mat-menu`) en vez de aplanar la jerarquía — evita
  tener que rediseñar la agrupación por dominio solo para el layout móvil.
- El mismo breakpoint `(max-width: 959.98px)` del sidenav anterior se reutilizó tal cual — sigue
  siendo el punto donde no cabe la fila horizontal de grupos, no una elección nueva.
- Ítem activo: en la barra horizontal (desktop) es un subrayado de 3px en `--opt-accent` sobre el
  fondo `primary` de la toolbar (`.nav-horizontal .activo`) — coherente con la regla de "accent uso
  deliberadamente escaso" (un trazo, no un relleno). Dentro de un dropdown o del menú móvil (ambos
  renderizados en un overlay de CDK, fuera del árbol de `Shell`) el ítem activo usa el mismo tinte
  `primary-container`/`on-primary-container` que el resto de "Aplicación de color en listados y
  formularios" de abajo — requiere `::ng-deep` en `shell.scss` porque el contenido del `mat-menu`
  vive en un overlay, no alcanza con un selector scoped normal.
- Verificado visualmente con Playwright contra el dev server (login mockeado vía `sessionStorage` +
  interceptación de `**/api/**`, sin escribir en `dbOPT_NET` real) en tres anchos: desktop con
  dropdown abierto, dialog de alta con el título ya coloreado, y móvil con el menú de hamburguesa y
  su submenú anidado. No requirió cambios en `shell.spec.ts` (el test existente no aserta sobre el
  sidenav, solo que el componente se crea).

## Aplicación de color en listados y formularios (sesión 2026-08-27, nivel "moderado")

Hasta la sesión original, fuera del login y de la toolbar (`color="primary"`), el resto de la interfaz
(listados y formularios de Sucursales/Empresas/Usuarios/Roles/Clientes/Anamnesis/RecetaCristales)
era enteramente gris/neutro (`--mat-sys-on-surface-variant`, `--mat-sys-outline`) — se verificó
grepeando `color`/`background` en `features/**/*.scss` antes de esa sesión. Se aplicó un nivel
"moderado" de color de marca (elegido explícitamente por el usuario sobre "sutil" y "marcado") como
**reglas globales en `src/styles.scss`**, no archivo por feature — mismo patrón ya usado para el halo
de foco y `.opt-badge-clinico`, así se garantiza armonía automática en todos los módulos presentes y
futuros sin tocarlos uno por uno:

| Elemento | Regla | Dónde vive |
|---|---|---|
| Encabezado de cualquier `mat-table` | **v2.1 (2026-08-27):** fondo `--mat-sys-surface` + borde inferior 2px `--mat-sys-primary` + texto `--mat-sys-primary`, `font-weight: 600`. Antes era una banda `primary-container` plena — se aligeró para reforzar el atributo "sencillo" sin perder la señal de marca. | `.mat-mdc-table .mat-mdc-header-row`/`.mat-mdc-header-cell` en `styles.scss` |
| Fila de tabla en hover | Fondo `--mat-sys-primary-container` (feedback puntual, no permanente — sin cambio en v2.1) | `.mat-mdc-table .mat-mdc-row:hover` en `styles.scss` |
| Título de diálogo/formulario | Color `--mat-sys-primary` | `.mat-mdc-dialog-title` en `styles.scss` — cubre los 7 módulos porque todo el CRUD del proyecto abre el formulario en `MatDialog` (ver "CRUD simple: lista + diálogo" en `src/frontend/CLAUDE.md`) |
| Ícono neutro de `app-empty-state` (tono `'neutral'`, no `'error'`) | **v2.1 (2026-08-27):** color `--opt-accent-dark` (terracota) — el estado vacío es el lugar elegido para dejar respirar el acento cálido. Antes `--mat-sys-primary`; antes de eso `--mat-sys-outline` gris. El tono `'error'` sigue en `--mat-sys-error`. | `shared/components/empty-state/empty-state.scss` — un solo archivo, se propaga a todo listado que use el componente |

Estructural — no se tocó al aplicar v2.0, solo cambió el hex subyacente de los roles M3 que ya
consumía (`primary`, `primary-container`, `on-primary-container`). El `accent` (`#E2703B` en v2.0,
antes `#F2A93B`) deliberadamente sigue sin usarse como fondo de nada nuevo — reservado para CTAs
puntuales y el subrayado activo del navbar, misma regla de "uso escaso" que en v1.0. Nivel "marcado"
(barra de acento en cada `h1` de listado, bordes de tarjeta con color) se evaluó y se descartó
explícitamente por el usuario en la sesión original — no reconsiderarlo sin que lo pida de nuevo.

## Modo oscuro (2ª pasada UX/UI, sesión 2026-08-27)

La app tiene tema claro y oscuro. La preferencia vive en el servicio `Tema`
(`src/app/shared/services/tema.ts`, `providedIn: 'root'`): `light` | `dark` | `system`
(por defecto `system` = seguir a `prefers-color-scheme`). Se persiste en `localStorage`
(`opt.tema`) envuelto en try/catch — a diferencia del token de sesión, que va en
`sessionStorage`; el tema no es dato sensible. El servicio escribe `data-opt-theme` en
`<html>`; `styles.scss` reacciona a ese atributo y, en su ausencia, a `prefers-color-scheme`.
El toggle está en la toolbar del `Shell` (menú con Claro / Oscuro / Según el sistema).

- Angular Material (esta versión) **no** acepta `theme-type: light dark` en una sola emisión
  → `styles.scss` emite `mat.theme` una vez para claro (`:root`) y otra para oscuro (dentro
  de `@media (prefers-color-scheme: dark)` y de `html[data-opt-theme='dark']`). Si se
  actualiza Angular Material, revisar si ya soporta la emisión única y simplificar.
- Por el 2º set de tokens M3 en CSS, el `maximumWarning` de budget `initial` en
  `angular.json` se subió de 500 kB a 550 kB (documentado, no accidental).
- Los chips de `OPT_EstadoOT` llevan fondo **y** color de texto explícitos (isla de color):
  se ven igual en claro y oscuro **a propósito** — un estado de proceso es semántico, no
  debe invertirse con el tema, y los pares ya están verificados AA. No agregar variante dark
  a `--opt-estado-ot-*`.
- Componente nuevo: usar siempre `var(--mat-sys-*)` / `var(--opt-*)`, nunca un hex suelto ni
  `#fff`/`#000` literales (salvo sobre un fondo de marca fijo como `.opt-badge-clinico`).
  Así el componente sale correcto en ambos temas sin trabajo extra.

## Escalas de sistema en tokens (2ª pasada UX/UI, sesión 2026-08-27)

`theme-tokens.scss` ahora define, además de los colores con significado:

| Grupo | Variables | Uso |
|---|---|---|
| Espaciado (base 4px) | `--opt-space-3xs` … `--opt-space-2xl` (2/4/8/12/16/24/32/48) | Todo `margin`/`gap`/`padding` en SCSS nuevo — no píxeles sueltos |
| Radio | `--opt-radius-sm` (8) / `-md` (12) / `-lg` (16) / `-pill` (999) | Tarjetas → `md`; botones y chips → `pill` |
| Elevación | `--opt-elevation-1` / `-2` | Sombras (reforzadas en oscuro con negro más opaco) |
| Movimiento | `--opt-motion-fast/base/slow` (120/200/320 ms), `--opt-easing-standard/emphasized` | Transiciones — respetar `prefers-reduced-motion` (ya hay un reset global en `styles.scss`) |

## Componentes compartidos de layout (2ª pasada UX/UI, sesión 2026-08-27)

| Componente | Dónde vive | Reemplaza a | Notas |
|---|---|---|---|
| `app-page-header` | `shared/components/page-header/` | El `<div class="encabezado">` reimplementado en cada feature | Inputs: `title` (req.), `subtitle?`, `backTo?` (ruta del botón de volver), `backLabel`. La acción primaria de la pantalla se proyecta como `<ng-content>`. Aplicado a los 7 listados + stubs + `cliente-ficha`. |
| `app-list-skeleton` | `shared/components/list-skeleton/` | El `<mat-spinner diameter="32">` suelto flotando arriba a la izquierda | Inputs: `rows` (def. 6), `header` (def. true). Reserva alto → el layout no "salta" al llegar los datos. Respeta `prefers-reduced-motion`. `role="status"` + texto "Cargando…" para lectores de pantalla. |

## Menú de overflow para acciones de fila (2ª pasada UX/UI, sesión 2026-08-27)

Una fila de tabla con **más de 2 acciones** deja 1 acción primaria como `mat-icon-button`
inline y mueve el resto — **incluida la destructiva** — a un `mat-menu` disparado por un
botón `more_vert`. El contenido del menú va en un `<ng-template matMenuContent let-x="x">`
con `[matMenuTriggerData]="{ x: fila }"` para no crear un menú por fila. Aplicado a
`clientes-list` (ver ficha inline; editar/eliminar en el menú) y `usuarios-list` (editar
inline; clave/sucursales/activar/eliminar en el menú). Con exactamente 2 acciones
(`empresas-list`, `sucursales-list`) se dejan las dos inline.

- Todo `mat-icon-button` lleva `[attr.aria-label]` descriptivo (no basta `matTooltip` — no
  es nombre accesible). La acción destructiva lleva además la clase global
  `.opt-accion-destructiva` (`styles.scss`), que la tiñe con `--mat-sys-error`.

## Formularios (2ª pasada UX/UI, sesión 2026-08-27)

- **Mensajes de validación**: cada campo con validadores muestra `<mat-error>` por tipo de
  error (`@if (form.controls.x.hasError('required'))` …) y `<mat-hint>` cuando aporta
  (mínimos de longitud, "el RUT no se puede modificar"). Antes el usuario solo veía el botón
  Guardar deshabilitado sin saber qué faltaba.
- **Clave**: todo campo de contraseña (login, alta de usuario, cambio de clave) tiene un
  botón `matSuffix` de mostrar/ocultar (`visibility`/`visibility_off`) con `aria-label` y
  `aria-pressed`. El `type` alterna con un signal `ocultarClave`.
- **Formulario largo en diálogo**: si tiene más de ~6 campos se agrupa con subtítulos de
  sección (`<p class="opt-form-section">`) y grilla de 2 columnas (`class="opt-form-grid"` en
  `<mat-dialog-content>`; campos de ancho completo con `.opt-col-2`). Clases globales en
  `styles.scss`, colapsan a 1 columna bajo 600px. Referencia: `cliente-form` (11 campos →
  Identificación / Contacto / Ubicación). El resto de formularios (Sucursal, Empresa,
  Usuario: ≤8 campos) siguen en 1 columna. **No** promover a ruta de alta/edición — el
  patrón sigue siendo `MatDialog` (ver `src/frontend/CLAUDE.md`).
- **Fechas**: mostrar siempre con `DatePipe` (`| date: 'dd-MM-yyyy'`), nunca el string ISO
  crudo del backend.
- **Badge de dato sensible**: el texto es siempre **"Información clínica"** (se unificó; antes
  había "Salud" y "Dato de salud — Ley 21.719" mezclados). En `cliente-ficha` el badge es
  persistente en el `app-page-header`, con ícono `lock`.

## Encabezados de página (2ª pasada UX/UI, sesión 2026-08-27)

`styles.scss` define la escala para encabezados "desnudos" del contenido. **Valores v2.1
(sesión 2026-08-27, un escalón más bajos que la guía §4 original):** `h1:not([class])`
**24px**/600, `h2:not([class])` **19px**/600, `h3:not([class])` **15px**/600 (eran 27/21/16.5).
Los encabezados de componentes Material (`h2[mat-dialog-title]`, que trae clase) quedan
excluidos por el `:not([class])`. `app-page-header` replica la escala del H1 en su propia
clase (`page-header.scss`, también actualizado a 24px). Un `<h1>` con clase propia no hereda
esto — quítale la clase o replica la escala.

## Densidad y escala tipográfica compacta (v2.1, sesión 2026-08-27)

Corrige la percepción de "todo grande, en especial los inputs". La causa raíz era doble y
global (no `font-size` sueltos): `density: 0` + la escala tipográfica M3 por defecto, ambas
calibradas por Google para uso táctil/móvil. Todo el cambio vive en `src/styles.scss` (más
3 ajustes menores de espaciado en `shell.scss`/`page-header.scss`/`clientes-list.scss` y el
`empty-state.scss`). Ningún `.scss` de feature cambió por tamaño. Guía HTML de referencia con
comparativa antes/después: `src/Guia_de_estilo/propuesta-densidad-tipografia.html`.

### Densidad Material

`mat.theme(( … density: -2 … ))` en `styles.scss` (antes `0`). Efecto: form-field ~56→~48px,
fila de `mat-table` ~52→~44px, botón ~40→~32px, ítem de menú ~48→~40px. Se eligió `-2` y no
`-3` porque `-3` requiere QA visual del notch de `appearance="outline"` (el único appearance
usado en el proyecto, 52 instancias) — bug histórico de Material en densidades bajas. **No
volver a `density: 0`.** Un componente nuevo hereda esta densidad, no necesita ajuste propio.

### Escala tipográfica (override de tokens `--mat-sys-*` tras `mat.theme()`)

Se sobrescriben los tokens de sistema M3 **después** del `@include mat.theme()`, dentro del
bloque `html { }`. Las familias y los pesos siguen saliendo del tema (`var(--mat-sys-*-font)`
/ `-weight`); solo cambian tamaño e interlineado. Se redefine cada rol granular
(`--mat-sys-<rol>-size` / `-line-height`) **y** su shorthand (`--mat-sys-<rol>`), porque
`styles.scss` y algunos componentes Material usan `font: var(--mat-sys-body-medium)` (el
shorthand resetea la familia si se define después — por eso también se redefine).

| Rol M3 | Uso | M3 stock (px) | v2.1 (px) |
|---|---|---|---|
| `body-large` | texto dentro de los inputs | 16 / 24 | **14 / 20** (escritorio) |
| `body-medium` | cuerpo, celdas de `mat-table` | 14 / 20 | **13.5 / 19** |
| `body-small` | `mat-hint`, `mat-error`, metadatos | 12 / 16 | **11.5 / 16** |
| `label-large` | texto de botón, etiqueta de campo | 14 / 20 | **13 / 18** |
| `title-medium` | título de `app-empty-state`, cards | 16 / 24 | **15 / 22** |
| `title-large` | título de `MatDialog` | 22 / 28 | **19 / 26** |

**Excepción móvil (obligatoria):** `@media (max-width: 599.98px) { html { --mat-sys-body-large-size: 1rem } }`
— el texto del input vuelve a 16px bajo 600px porque Safari iOS hace auto-zoom al enfocar un
`<input>` con texto <16px. No quitar esta regla.

### Espaciado (ajustes v2.1, no cambia la escala `--opt-space-*`)

| Lugar | Antes | v2.1 |
|---|---|---|
| `.contenido` padding (escritorio) | 24px | 20px (móvil sigue 16) |
| `app-page-header` `margin-bottom` | `--opt-space-lg` (24) | `--opt-space-md` (16) |
| `.filtro` de `clientes-list` `margin-bottom` | `--opt-space-md` (16) | `--opt-space-sm` (12) |
| `app-empty-state` padding | 48/24px | 36/24px |

### Toque humanista acotado en `app-empty-state` (atributo "amigable")

`empty-state.scss`: el título usa **Fraunces** (`var(--opt-font-titular)` tras el shorthand
`font:`) y el ícono neutral usa **`--opt-accent-dark`** (terracota). Es una excepción
deliberada y acotada a "nunca Fraunces / poco terracota en la interfaz de trabajo": aplica
solo al estado vacío/error ("momento de pausa"), nunca a tablas ni formularios. El tono
`'error'` del ícono se mantiene en `--mat-sys-error`.

## Asistentes por pasos e impresión (sesión 2026-08-28, 2ª)

Dos patrones nuevos que introdujo el alta de una Orden de Trabajo (ADR `0009`).

**Alta larga = `mat-stepper`, no una página de scroll único.** Cuando un alta reproduce un flujo
de mesón con varias etapas (el legacy las hacía con pestañas y guardado parcial), va como
stepper: un `FormGroup` por paso enlazado con `[stepControl]`, **lineal al crear** (el orden de
los pasos es el orden en que ocurren las cosas) y **no lineal al editar** (el operador ya sabe a
qué va). Un paso que solo aplica al crear —el de Pago en la OT— simplemente no se declara en
modo edición. Referencia: `features/ordenes-de-trabajo/pages/orden-de-trabajo-form/`.

**Crear una entidad relacionada sin salir de la pantalla** se resuelve abriendo su `MatDialog` ya
existente (`ClienteForm`, `RecetaCristalesForm`) y consumiendo `afterClosed()`, nunca duplicando
sus campos dentro del formulario anfitrión. El anfitrión pone la señal de que hace falta (el
aviso "no hay clientes que coincidan con «…»" con su botón) y la ficha resumida de lo ya elegido,
con acción de editar.

**Impresión — reglas globales, nunca por componente.** El bloque `@media print` vive en
`styles.scss`: un `@media print` dentro del SCSS encapsulado de un componente no puede ocultar el
resto de la aplicación. El contrato es:

- Quien imprime agrega la clase `opt-imprimiendo` al `<body>` mientras dura el `window.print()` y
  la quita después (`afterprint` + `finally`, porque `afterprint` no dispara en todos los
  navegadores si se cancela).
- Todo lo que no va en papel se marca con `.opt-no-imprimir` (título del diálogo, botonera).
- El bloque global oculta la app y devuelve el overlay de Material (`position: fixed`) al flujo
  del documento — sin eso, el papel se recorta en la primera página.
- El comprobante de una OT es `features/ordenes-de-trabajo/components/ticket-ot/`
  (`app-ticket-ot`, input `orden`), pensado para hoja angosta (`max-width: 420px`). Reusa
  `app-receta-graduacion` para la graduación. Usa tokens del tema para que la vista previa
  respete el modo oscuro; al imprimir, el navegador aplica su propio blanco.

## Pendiente (no inventar, preguntar o derivar de otra fuente)

- Copy exacto de consentimiento explícito para datos de salud y de notificación de brechas: pendiente con equipo legal/DPO (punto 5 de pendientes en `CLAUDE.md`).
- Validación de esta propuesta (v2.0, igual que v1.0 antes) con stakeholders reales (dueños de óptica, equipo de atención) — fase 6 de la hoja de ruta en `Manual_Tecnico_UX_OPT.docx`. Aplicar v2.0 al código no es una validación de stakeholders.
- Vectores finales del logotipo (SVG) — hoy solo existe como descripción + mockups PNG dentro de `Manual_Tecnico_UX_OPT.docx` (con los hex de v1.0 — pendiente de regenerar esos mockups con la paleta de v2.0 si se decide mantenerla) y una referencia de proporción/color dentro de `src/Guia_de_estilo/index.html`. El login usa un lockup solo de texto (`OPT` + "Gestión de ópticas") mientras no exista el vector — no fabricar un isotipo aproximado.
- Aplicar `.opt-mono` a los listados existentes que muestran RUT/folios (Clientes, Usuarios, Empresas) — no se tocó en esta sesión para no expandir el alcance a features ya estables sin que se pida.
- Correr `npm run build`/`lint`/`test` localmente para confirmar que el cambio de tema no rompe nada — esta sesión solo pudo verificar el SCSS de forma aislada (ver "Implementación en código").

## Cómo aplicar este contexto

- Generando un componente Angular/Material nuevo → usar los tokens ya cargados (`var(--mat-sys-*)` para roles M3, `var(--opt-*)` de `theme-tokens.scss` para los colores con significado), no valores hex sueltos inventados en el momento. **No fijar tamaños de fuente ni alturas de control por componente** — la densidad (`-2`) y la escala tipográfica compacta (v2.1) ya son globales; si un texto se ve grande, el problema es un rol M3 mal elegido, no un `font-size` que falte.
- Mostrando un estado de OT o una forma de pago → usar exactamente los pares fondo/texto de las tablas de catálogos vía las variables `--opt-estado-ot-*`/`--opt-forma-pago-*` de `theme-tokens.scss`, no reinterpretar ni recalcular el hex.
- Escribiendo copy de UI, error o correo → aplicar la tabla de voz y tono según el contexto (interno vs. cliente).
- Mostrando Anamnesis o RecetaCristales → agregar la clase `.opt-badge-clinico` (ya definida en `src/styles.scss`) en la pantalla.
- Mostrando un RUT, folio de OT o monto → agregar la clase `.opt-mono` (IBM Plex Mono, ya definida en `src/styles.scss`).
- Construyendo un listado nuevo → usar `app-empty-state` para los estados vacío y de error (nunca un `<p>` suelto), y no asumir un ancho de viewport mínimo (el navbar ya es responsive, ver "Patrones de UI agregados").
- Construyendo un alta con varias etapas o algo que se imprima → ver "Asistentes por pasos e impresión": stepper con un `FormGroup` por paso, diálogos existentes para crear entidades relacionadas, y `@media print` solo en `styles.scss`.
- Bloqueando la edición de una entidad por su propio estado, mostrando una opción "reciente" entre muchas, o pidiendo elegir una de varias modalidades excluyentes → ver la tabla de "Patrones de UI agregados" (aviso de bloqueo, combo + historial colapsable, tarjetas de modalidad).
- Proponiendo un color/combinación no cubierta aquí → calcular su contraste WCAG antes de proponerla, y si se adopta, añadirla a este archivo y a `Manual_Tecnico_UX_OPT.docx` en la misma sesión.

### Agrupación del menú (sesión 2026-09-23)

`gruposNav` en `layout/shell/shell.ts` tiene 4 grupos en orden alfabético: **Administración** (Empresas, Sucursales, Usuarios), **Comercial** (Clientes, Cobranza, Cobranza: Reporte, Operativos, Órdenes de Trabajo), **Inventario** (Ajustes, Compras, Enviar, Productos, Recibir, Stock) y **Reportes** (ítem único). Ítems dentro de cada grupo también alfabéticos. Un ítem nuevo va al grupo funcional que corresponda; grupo nuevo solo si no encaja. Solo cambió la lista de datos, no el template ni las rutas.
