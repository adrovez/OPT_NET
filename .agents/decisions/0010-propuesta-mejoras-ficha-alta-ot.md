# 0010 — Propuesta: observaciones de mesón sobre la ficha/alta de OT

**Estado:** Aceptada
**Fecha:** 2026-09-08

## Contexto

Alexis levantó seis observaciones sobre el módulo Orden de Trabajo tras revisar el sistema nuevo (asistente por pasos de `0009` y ficha ruteada de `0008`). Antes de proponer cambios se revisó el código actual (`src/backend/OPT.Domain/Entities/Comercial/OrdenDeTrabajo.cs`, `src/frontend/.../ordenes-de-trabajo/pages/orden-de-trabajo-form/*`, `.../orden-de-trabajo-ficha/*`, `src/frontend/.../receta-cristales/pages/receta-cristales-form/*`) para separar lo que ya está resuelto de lo que es una brecha real, y para que la propuesta sea concreta en vez de repetir el enunciado de cada observación.

Resultado del contraste: de las seis observaciones, **una ya está implementada** (1), **tres son un problema real de ubicación de campos** que se resuelve reorganizando el asistente (2, 3, 5 — están relacionadas entre sí), y **dos son brechas genuinas de funcionalidad/UX** (4, 6).

## Alternativas consideradas

**Para el reordenamiento de campos (2/3/5):** se evaluó agregar una cabecera fija fuera del `mat-stepper`, visible en los cuatro pasos (como la cabecera de solo lectura de la ficha `0008`). Se descarta para el alta: el patrón del asistente (`0009`) es deliberadamente secuencial —cada paso pide solo lo que corresponde a ese momento del mesón— y una cabecera fija duplicaría campos fuera de esa secuencia. Se prefiere mover los campos al paso donde el negocio los levanta (Cliente), no sacarlos del stepper.

**Para el combo de receta (4):** se evaluó reemplazar completamente la lista de historial por un combo. Se descarta: el historial completo sigue siendo necesario para el caso clínico (una receta antigua puede ser la correcta si el cliente pide repetir un par igual). Se propone un combo como atajo por defecto, no como reemplazo.

**Para el plan de cuotas (6):** se evaluó cambiar `GenerarPlanCuotas` de vencimiento mensual (`AddMonths`) a vencimiento cada 30 días corridos. Ver pregunta abierta más abajo — es un cambio de regla de negocio en el dominio, no solo de UI, y no debe asumirse sin confirmación.

## Decisión (propuesta)

### 1. "Las OT no se pueden modificar una vez Entregado" — ya implementado, verificar despliegue

`OrdenDeTrabajo.GarantizarModificable()` bloquea `Actualizar`, `AgregarDetalle`, `ReemplazarDetalles`, `RegistrarAbono`, `RegistrarPago` y `GenerarPlanCuotas` cuando `EstadoOTId == EstadosOT.Entregado` (o anulada), lanzando `DomainException`. En el frontend, `orden-de-trabajo-ficha.ts` (`esTerminal`) deshabilita los botones *Editar*, *Registrar abono*, *Registrar pago* y *Generar cuotas* apenas la OT llega a ese estado.

**Propuesta:** no hay cambio de regla — la regla ya existe en dominio y UI. Dos mejoras menores de UX, no de negocio:

- La ficha no muestra un mensaje explícito cuando una OT está en estado terminal; solo deshabilita botones sin explicar por qué. Agregar un aviso (`app-empty-state` o banner) tipo "Esta orden fue entregada el dd-mm-aaaa y no admite modificaciones" cuando `esTerminal()`.
- Confirmar que el ambiente que Alexis probó tiene el build más reciente del frontend — si la observación vino de una versión anterior a la sesión del 2026-08-27 (ADR `0007`), ya está resuelta y solo falta redesplegar.

### 2 + 3 + 5. Cabecera de OT, Beneficiario en Cliente — reorganizar el paso 1 del asistente

Las tres observaciones describen el mismo hallazgo desde tres ángulos: **los campos de cabecera están repartidos entre el paso 1 (Cliente) y el paso 3 (Detalle) del asistente**, y el negocio los espera juntos, al principio.

Estado actual campo por campo (`orden-de-trabajo-form.html`):

| Campo | Dónde está hoy | Falta o está mal ubicado |
|---|---|---|
| N° de OT | No se pide — lo genera la BD (`NumeroOT`, `SEQUENCE`) | Correcto tal cual: no es un campo de formulario, es un dato que se muestra recién en la confirmación (`orden-creada-dialog`) y en la ficha. Si Alexis lo esperaba como campo editable, es una regresión intencional del legacy (`004`/`0003`), no una omisión. |
| Fecha atención | Paso 3 · Detalle, ya con default `new Date()` (hoy) | Ubicación: debería estar en el paso 1, no en el 3. El default a hoy ya funciona. |
| Fecha entrega | Paso 3 · Detalle | Ubicación: mismo caso. |
| Hora entrega | Paso 3 · Detalle | Ubicación: mismo caso. |
| Empresa convenio | Paso 1 · Cliente | Ya está donde corresponde. |
| Beneficiario | Paso 3 · Detalle | Ubicación: es la observación 5, textual. Debe ir en el paso 1 · Cliente. |

**Propuesta:** mover `fechaAtencion`, `fechaEntrega`, `horaEntrega` y `beneficiario` del `formOrden` (paso 3 · Detalle) al `formCliente` (paso 1 · Cliente), junto a Empresa convenio. El paso 3 queda enfocado solo en las líneas de producto (Detalle propiamente tal) más Observaciones. Esto resuelve 2, 3 y 5 con un solo cambio de estructura, sin tocar backend: los cuatro campos ya existen en `OrdenDeTrabajo` y en `CrearOrdenDeTrabajoCommand`, solo cambian de paso en el formulario.

Ajuste correspondiente en la ficha de solo lectura (`orden-de-trabajo-ficha.html`): ya muestra estos cinco datos juntos en la cabecera (`<dl class="cabecera__datos">`), así que no requiere cambios — el alta pasa a reflejar el mismo agrupamiento que la vista ya tiene.

### 4. Pestaña Receta — combo con la receta más reciente (≤ 3 meses) + corrección del botón Guardar

**a) Combo de receta reciente.** Hoy el paso 2 (`orden-de-trabajo-form.html`, líneas ~147-199) muestra un `mat-radio-group` con **todo** el historial de recetas del cliente, sin distinguir la vigente. Se propone anteponer un selector (`mat-select` o el mismo radio-group reordenado) que:

- Preseleccione automáticamente la receta más reciente del cliente cuya `fechaRegistro` esté dentro de los últimos 3 meses (si existe).
- Si no hay ninguna receta en ese rango, no preselecciona nada y deja "Sin receta" como hoy.
- Mantiene visible el resto del historial (recetas de más de 3 meses) en una sección secundaria colapsable ("Ver historial completo"), porque sigue siendo un dato clínico necesario para el caso en que corresponda repetir una graduación antigua.

Es un cambio de frontend únicamente: no se necesita ninguna columna ni endpoint nuevo — `recetasCliente()` ya trae la lista completa ordenada, solo falta filtrar/ordenar por fecha al construir la preselección.

**b) Botón Guardar deshabilitado en "Nueva Receta".** Causa raíz identificada en `receta-cristales-form.ts`: el botón usa `[disabled]="form.invalid || guardando()"`, y al marcar "Incluir Cristales Lejos" o "Incluir Cristales Cerca" el formulario agrega `Validators.required` a las tres observaciones de ese bloque (OD, OI, DP — líneas 148-183). El formulario **no muestra ningún `mat-error` bajo esos tres campos** (a diferencia de otros campos del sistema, que sí lo hacen), así que para quien lo usa el botón se ve "roto" en vez de "esperando datos obligatorios".

No es un bug de lógica — es una falta de retroalimentación visual. Propuesta:

- Agregar `mat-error` visible bajo `observacionOdLejos/Cerca`, `observacionOiLejos/Cerca` y `observacionDpLejos/Cerca` cuando están vacías y su bloque está incluido.
- Agregar un resumen o tooltip en el botón Guardar cuando está deshabilitado (p. ej. "Completa las observaciones OD/OI/DP del bloque incluido para guardar"), consistente con el patrón ya usado en otros formularios del sistema.
- Pregunta abierta para el negocio: ¿esas tres observaciones deberían ser realmente obligatorias siempre que se incluye Lejos/Cerca, o debería bastar con los valores numéricos de graduación y dejar la observación como opcional? El legacy las exigía (`Receta.js`); si el mesón hoy las omite con frecuencia, vale la pena reconsiderarlo — ver preguntas abiertas.

### 6. Pestaña Pago — rediseño visual con tres modalidades explícitas

Estado actual (`orden-de-trabajo-form.html`, paso 4 · Pago): un formulario plano con Abono inicial + Forma de pago + Referencia, más un bloque de cuotas (N° de cuotas, primer vencimiento, checkbox "Sin plan de cuotas") y un solo valor de "cuota estimada" en texto — sin distinguir explícitamente las tres formas de cobro que describe la observación, y sin mostrar el calendario de vencimientos antes de guardar.

**Propuesta:**

- Reemplazar el bloque único por un selector de modalidad (`mat-button-toggle-group` o tres `mat-radio-button` grandes, con ícono), que determina qué campos se muestran:
  1. **Paga el total** — un solo campo de forma de pago (con las cuatro opciones que menciona Alexis: efectivo, débito, crédito, cheque — ya existen en `OPT_FormaPago`, catálogo `formasPago()`); `abonoInicial` se fija al total, sin cuotas.
  2. **Abona y el resto en cuotas** — abono inicial + forma de pago del abono, luego N° de cuotas y primer vencimiento para el saldo restante.
  3. **Paga en cuotas** — sin abono inicial; todo el precio se reparte en cuotas desde el primer vencimiento.
- Para las modalidades 2 y 3, mostrar una **tabla de vista previa de cuotas** (N°, fecha de vencimiento, valor) calculada en el frontend con la misma regla que `OrdenDeTrabajo.GenerarPlanCuotas` (valor parejo, diferencia de redondeo en la última cuota), antes de confirmar — hoy solo se ve "cuota estimada" como número suelto, no el calendario completo que pide la observación.
- Reutilizar el estilo de `app-resumen-financiero` para la tarjeta de resumen (Total / Abono / Saldo), en vez de la fila de campos plana actual, para la "mejora visual" que pide el punto 6.
- No requiere cambios de backend: `GenerarPlanCuotasCommand` y `RegistrarAbonoCommand` ya existen; el cambio es de estructura y presentación del formulario, más la función de cálculo de vista previa en el cliente (duplicando en TypeScript la misma fórmula del dominio, ya usada hoy para "cuota estimada").

## Preguntas abiertas (resueltas 2026-09-08, antes de implementar)

1. **Punto 1:** no requería confirmación — la regla ya estaba implementada; solo se agregó el aviso explícito.
2. **Punto 4a:** confirmado — los 3 meses se cuentan **desde hoy**, no desde la fecha de atención de la orden.
3. **Punto 4b:** confirmado — las observaciones OD/OI/DP **pasan a ser opcionales**. Se quitó el `Validators.required` dinámico que se agregaba al incluir Lejos/Cerca; esa era la causa raíz del botón Guardar "roto".
4. **Punto 6:** confirmado — las cuotas siguen siendo **mensuales**, igual que hoy (`OrdenDeTrabajo.GenerarPlanCuotas`, `AddMonths`). No hubo cambio de regla de negocio en el dominio; la vista previa de cuotas del frontend replica esa misma fórmula (mensual, no cada 30 días corridos).

## Implementación (2026-09-08)

Casi todo el alcance quedó en frontend — ningún endpoint, comando ni columna nuevos. La única excepción es el punto 4b: la primera pasada solo tocó el frontend y dejó un defecto real (ver más abajo), corregido en la misma sesión con un cambio de dos líneas en Application. Cambios por archivo:

- `orden-de-trabajo-ficha.{html,scss}`: aviso `.aviso--bloqueo` cuando la orden está entregada o anulada, con la fecha de entrega cuando corresponde.
- `orden-de-trabajo-form.ts`: `fechaAtencion`, `fechaEntrega`, `horaEntrega` y `beneficiario` se movieron de `formOrden` (paso Detalle) a `formCliente` (paso Cliente) — mismo mapeo hacia `CrearOrdenDeTrabajoCommand`/`ActualizarOrdenDeTrabajoCommand`, sin cambios de contrato. `formOrden` queda solo con `observaciones` y el espejo `hayDetalle`.
- `orden-de-trabajo-form.html`: los cuatro campos de cabecera pasan al paso 1 · Cliente, junto a Empresa convenio; el paso 3 · Detalle queda enfocado en el detalle de productos y Observaciones.
- Paso 2 · Receta: `recetasRecientes` (computed, ventana de 3 meses desde hoy) alimenta un combo (`mat-select`) que preselecciona automáticamente la receta más reciente al elegir cliente — solo al crear, nunca al editar. El historial completo (`mat-radio-group`, sin cambios) queda detrás de "Ver historial completo", expandido por defecto solo si no hay recetas recientes.
- `receta-cristales-form.ts`: se quitó el `Validators.required` dinámico de las 6 observaciones OD/OI/DP (Lejos/Cerca) — quedan editables pero opcionales. Resuelve la causa raíz del botón Guardar deshabilitado.
- **Corrección backend (detectada al documentar esta sesión):** `CrearRecetaCristalesCommandValidator`/`ActualizarRecetaCristalesCommandValidator` (agregados en `007_receta_incluir_observaciones_detalle.sql`, sesión previa del mismo día) todavía exigían esas 6 observaciones con `NotEmpty().When(x => x.IncluirLejos/IncluirCerca)` — el cambio de frontend por sí solo habría dejado el botón Guardar habilitado pero la API habría seguido rechazando con 422. Se quitaron las 6 reglas `NotEmpty` de ambos validadores (las de `MaximumLength(50)` se mantienen). Sin cambio de esquema ni de columnas — solo de validación en `OPT.Application`. No se pudo compilar `OPT.sln` para verificar (`dotnet` no está instalado en el entorno donde se hizo el cambio) — **pendiente `dotnet build OPT.sln` antes de desplegar**.
- Paso 4 · Pago: nuevo control `modalidadPago` (`'total' | 'abonoCuotas' | 'cuotas' | null`, `Validators.required`, sin preseleccionar — obliga a elegir explícitamente) que habilita/deshabilita `abonoInicial`/`numeroCuotas`/`primerVencimiento` según la modalidad elegida, vía un `effect()`. Nueva `cuotasPreview` (computed) replica en TypeScript la fórmula de `OrdenDeTrabajo.GenerarPlanCuotas` (valor parejo, resto en la última cuota, vencimiento mensual) para mostrar la tabla de cuotas antes de guardar. Se reemplazó la fila de campos plana por `app-resumen-financiero` (mismo componente que usa la ficha) con los valores proyectados.
- Se quitó el checkbox "Sin plan de cuotas (cobro directo)" de la interfaz: esa salida explícita del legacy queda representada ahora por la modalidad "Paga el total" — el control `sinPlanCuotas` se mantiene internamente porque lo sigue usando `validarPlanCuotas`, pero ya no se expone.

**Verificación:** `ng build --configuration development` compila sin errores (incluye el chequeo estricto de plantillas de Angular sobre los cuatro archivos tocados). No se pudo ejecutar la batería de pruebas unitarias (`ng test`) en este entorno — el corredor (Vitest) no devolvió resultado dentro del tiempo disponible, una limitación del entorno de ejecución remoto, no del código. Los dos casos de `orden-de-trabajo-form.spec.ts` sobre `formPago` (`planRequerido`, `vencimientoRequerido`) se revisaron a mano contra el nuevo `effect` de modalidad: como `modalidadPago` no trae valor por defecto, ninguno de los dos casos activa el `effect` (que solo actúa sobre `'total'`, `'abonoCuotas'` o `'cuotas'`), así que su comportamiento no cambió. Pendiente: correr `ng test` en un entorno con el runner disponible antes de dar por cerrado el punto 6.

## Consecuencias

**A favor:**

- Los puntos 2, 3 y 5 se resuelven con un solo cambio de estructura (mover cuatro campos de paso), sin tocar backend ni esquema.
- El punto 1 no requiere ningún cambio de negocio — solo confirmar despliegue y, opcionalmente, mejorar el mensaje al usuario.
- Los puntos 4 y 6 quedan acotados a frontend (más, en el punto 6, duplicar en TypeScript una fórmula que el dominio ya expone), sin nuevas tablas ni endpoints.

**Costos y riesgos:**

- Mover Beneficiario y las fechas al paso 1 cambia el `FormGroup` que las contiene (`formCliente` en vez de `formOrden`): hay que revisar el mapeo al armar `CrearOrdenDeTrabajoCommand`/`ActualizarOrdenDeTrabajoCommand` en `guardar()` para que sigan viajando en el `POST`/`PUT`, y actualizar los tests (`orden-de-trabajo-form.spec.ts`) que hoy asumen esos campos en `formOrden`.
- El punto 6 duplica en el frontend la fórmula de reparto de cuotas del dominio (valor parejo + resto en la última). Si `GenerarPlanCuotas` cambia en el backend, hay que recordar actualizar también la vista previa — vale la pena dejarlo anotado como gotcha, igual que otros casos ya documentados en `0008`/`0009`.
- El punto 4b (relajar observaciones obligatorias) es un cambio de regla de negocio heredada del legacy (`reglas-negocio-legado.md` no la menciona explícitamente) — no debe implementarse sin la confirmación de la pregunta abierta 3.

## Referencias

- Backend: `src/backend/OPT.Domain/Entities/Comercial/OrdenDeTrabajo.cs` (`GarantizarModificable`, `GenerarPlanCuotas`)
- Frontend: `src/frontend/.../ordenes-de-trabajo/pages/orden-de-trabajo-form/orden-de-trabajo-form.{ts,html}`, `.../orden-de-trabajo-ficha/orden-de-trabajo-ficha.{ts,html}`, `.../receta-cristales/pages/receta-cristales-form/receta-cristales-form.{ts,html}`
- ADRs relacionados: `[[0007-api-comercial-flujo-estados-ot]]`, `[[0008-receta-vinculada-a-ot-y-vista-ver-orden]]`, `[[0009-alta-ot-asistente-por-pasos]]`
