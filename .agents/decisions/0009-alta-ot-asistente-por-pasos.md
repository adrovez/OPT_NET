# 0009 — Alta de Orden de Trabajo como asistente por pasos: cliente y receta en línea, y ticket imprimible

**Estado:** Aceptada
**Fecha:** 2026-08-28

## Contexto

El formulario de alta de una OT del sistema nuevo se escribió el 2026-08-27 mirando el contrato de la API, no el flujo real del mesón. Al contrastarlo contra `old/Fuente/OPT.Web/Areas/OrdenTrabajo/Controllers/IngresoController.cs` y sus cinco vistas parciales aparecieron cuatro capacidades del legacy que se habían perdido:

1. **Alta del cliente dentro del ingreso.** El legacy busca por RUT (`DatoCliente`) y, si no existe, lo crea en el mismo acto (`ClienteInsertar`, invocado desde `POST Cliente`); si existe, reescribe sus datos con lo que el operador corrija. En el sistema nuevo el cliente solo se podía **elegir**: si llegaba alguien nuevo al mesón había que abandonar la orden, ir a Clientes, crearlo y volver a empezar.
2. **Toma de la receta dentro del ingreso.** El legacy tiene una pestaña Receta con las tablas de graduación completas (`POST Receta` → `RecetaInsertar`). El sistema nuevo solo permitía **vincular** una receta ya existente, y si el cliente no tenía ninguna el formulario indicaba ir a la ficha clínica y volver a editar la orden — peor que el legacy.
3. **Cierre con confirmación e impresión.** `Finaliza.cshtml` mostraba "OT N° X, Cliente…" con dos botones: *Nueva OT* e *Imprimir*, este último contra el reporte `Areas/Imprimir/Rpt/rptTicketOT.rdlc`. El sistema nuevo redirigía a la ficha, sin comprobante.
4. **Regla de cuotas.** `POST Finaliza` lanzaba excepción si quedaba saldo sin número de cuotas (`"Debe ingresar numero de cuotas para el saldo pendiente."`) y calculaba el valor de cuota en vivo.

Además, el legacy organizaba todo esto en cuatro pestañas con avance explícito (Cliente → Receta → Lentes → Abonos), mientras que el formulario nuevo era una única página de scroll largo.

Las decisiones de alcance se consultaron con el usuario vía `AskUserQuestion` antes de escribir código.

## Alternativas consideradas

**Para el layout**, se evaluó (a) mantener la página única agregando las secciones nuevas y (b) reproducir literalmente un `mat-tab-group` Cliente/Receta/Lentes/Abonos sin orden de avance. El usuario eligió el **stepper**: conserva la secuencia del legacy —que refleja el orden en que ocurren las cosas en el mesón— sin la página de scroll largo, y permite validar por paso.

**Para el alta de cliente y receta**, se evaluó un **endpoint transaccional nuevo** que aceptara cliente y receta embebidos dentro de `CrearOrdenDeTrabajoCommand` y creara todo en una sola transacción. Es más atómico, pero implica cambiar Application y API (comando, validadores, handler) para un caso que el legacy ya resolvía con llamadas sucesivas. El usuario eligió **reusar las APIs existentes**.

**Para la regla de cuotas**, se evaluó reproducir el bloqueo duro del legacy. Se descartó parcialmente: el sistema nuevo sí permite cobrar un saldo con pagos sueltos (ADR `0007`), así que bloquear siempre sería una regresión funcional.

## Decisión

### 1. El alta de una OT es un asistente por pasos

`orden-de-trabajo-form` pasa a ser un `mat-stepper` de cuatro pasos —**Cliente · Receta · Detalle · Pago**— que es la secuencia de las cuatro pestañas del legacy, con el nombre corregido del tercero ("Lentes" en el legacy es el Detalle, ver ADR `0008`).

- **Lineal al crear, no lineal al editar.** Al editar una orden existente el operador ya sabe a qué va.
- **El paso Pago solo existe al crear**: el abono inicial y el plan de cuotas viajan en el mismo `POST`; el dinero de una OT ya creada se maneja desde su ficha (Abonos / Pagos / Cuotas).
- Un `FormGroup` por paso (`formCliente` / `formOrden` / `formPago`) enlazado con `[stepControl]`. El detalle vive en un signal, así que el paso lo valida a través de un control espejo (`hayDetalle`, `Validators.requiredTrue`) sincronizado desde un `effect` — `mat-step` valida por control, no por signal.

### 2. Cliente y receta se crean reusando sus diálogos, con llamadas sucesivas

Cuando la búsqueda de cliente no arroja resultados, el paso 1 ofrece *Crear cliente* y abre el diálogo `ClienteForm` con el RUT ya tecleado (campo nuevo `ClienteFormDialogData.rutInicial`); al cerrarse, el cliente creado queda seleccionado. El cliente elegido se muestra en una ficha resumida con *Editar datos*, que abre el mismo diálogo en modo edición — es el equivalente a que el legacy reescribiera los datos del cliente al guardar la OT.

El paso 2 ofrece *Tomar receta nueva*, que abre `RecetaCristalesForm` con el `clientePublicId`; la receta creada se agrega al historial y queda seleccionada para vincularse a la orden.

**Se reusan los diálogos existentes en vez de duplicar sus campos dentro del stepper.** `ClienteForm` ya trae la cascada Región→Comuna y todas sus validaciones; `RecetaCristalesForm`, las tablas de graduación con sus rangos. Duplicarlos habría creado dos definiciones del mismo formulario que se desincronizan.

### 3. Un saldo sin plan de cuotas hay que declararlo, no se bloquea

Validador de grupo en `formPago`: si tras el abono inicial queda saldo, hay que indicar número de cuotas **y** primer vencimiento, o marcar explícitamente **"Sin plan de cuotas (cobro directo)"**. Es la intención del legacy —que un saldo no quede sin plan por descuido— sin la regresión de prohibir el cobro con pagos sueltos, que el sistema nuevo sí soporta. El valor de cuota estimado se muestra en vivo, como allá.

### 4. El ticket es un componente Angular impreso por el navegador, no un reporte del servidor

`components/ticket-ot/` (`app-ticket-ot`, input `orden`) reemplaza a `rptTicketOT.rdlc`: N° de OT, cliente, fechas de atención y entrega, detalle con su comentario, receta (reusando `app-receta-graduacion`) y el resumen de dinero con el plan de cuotas. Se muestra dentro de `components/orden-creada-dialog/`, el equivalente a `Finaliza.cshtml`, con *Imprimir ticket* / *Nueva OT* / *Ver orden*; *Nueva OT* reinicia el asistente sin recargar la página.

Las reglas de `@media print` viven en `styles.scss`, **no** en el SCSS del componente: los estilos encapsulados de un componente no pueden ocultar el resto de la aplicación. Quien imprime marca el `<body>` con `opt-imprimiendo` mientras dura el `window.print()`, y lo que no va en papel se marca con `.opt-no-imprimir`.

### 5. Lo que la migración ya mejoró no se revierte

El **N° de OT** lo sigue generando la base de datos (el legacy lo tecleaba el operador y validaba duplicados a mano con `POST Existe`) y el **precio** sigue siendo la suma del detalle (el legacy lo digitaba). Tampoco se recupera la hora de entrega fija en 12:00 que el legacy escribía ignorando el campo del formulario.

## Consecuencias

**A favor:**

- El caso más común del mesón —cliente nuevo que llega a encargar lentes— vuelve a resolverse en una sola pantalla, sin abandonar la orden a medias.
- La receta se puede tomar en el mismo acto, que es cuando el optometrista la tiene en la mano.
- El cliente se lleva un comprobante, como en el legacy, sin depender de un motor de reportes del servidor.
- Cero cambios de backend, base de datos o esquema: la sesión completa es frontend.

**Costos y riesgos aceptados:**

- **El alta no es atómica.** Cliente y receta se persisten **antes** que la orden, así que si el `POST` de la OT falla quedan creados. Es aceptable porque son datos válidos por sí mismos (un cliente y una receta existen independientemente de que se emita la orden) y es exactamente lo que hacía el legacy, que insertaba en cada pestaña. Si el negocio pidiera atomicidad, la salida es el endpoint transaccional descartado arriba.
- **La regla de cuotas es más laxa que la del legacy** por diseño: se puede seguir adelante marcando el cobro directo. Si el negocio quiere el bloqueo duro, es quitar esa casilla.
- **La impresión no está cubierta por tests.** El `@media print` y el `window.print()` solo se pueden verificar a mano en un navegador real.
- **Sin verificación end-to-end**, por el mismo bloqueo de credenciales del ADR `0008`: los usuarios migrados conservan la clave de 4 caracteres del legacy y `LoginCommandValidator` exige 6.
