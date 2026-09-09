# 0008 — Receta vinculada a la Orden de Trabajo y vista "Ver Orden"

**Estado:** Aceptada
**Fecha:** 2026-08-28

## Contexto

El negocio revisó la ficha de la OT del sistema nuevo contra el modal "Detalle Orden de Trabajo" del legacy (`Areas/OrdenTrabajo/Views/Ingreso/_ModalDetalle.cshtml`) y pidió dos cosas:

1. Conservar ese formulario, con sus cuatro pestañas: **Cliente**, **Receta**, **Lentes** y **Abonos**.
2. Que el **listado de OT no muestre datos al entrar**, como en el legacy — donde traer las 12.578 órdenes completas daba timeout.

Al mapear esas cuatro pestañas contra el sistema nuevo aparecieron tres huecos:

- **Receta.** En el legacy `OPT_RecetaCristales` tiene `idOT`: la receta se materializa en una orden concreta (12.574 de 13.183 filas la tienen poblada). En el esquema nuevo la receta cuelga **solo** del Cliente — decisión correcta en su momento (ver "Atención" en `glosario-dominio.md`), pero que hizo desaparecer el vínculo con la OT, y con él la posibilidad de responder "¿con qué graduación se fabricaron *estos* cristales?".
- **Cliente.** `OrdenDeTrabajoDto` solo llevaba RUT y nombre; la pestaña del legacy muestra además fecha de nacimiento, previsión, celular, correo, región, comuna y dirección.
- **Lentes.** El detalle del legacy tiene una columna `Comentario` por línea (el modelo y color del armazón, p. ej. `"FORMOSA F4 C2"`), poblada en 11.168 de las 20.573 líneas migradas. `OPT_DetalleOT` no tenía columna equivalente.

Las tres decisiones que cambiaban el alcance se consultaron con el usuario antes de escribir código.

## Alternativas consideradas

**Para la receta**, se evaluó **no** tocar el esquema y mostrar en la pestaña la receta del cliente más reciente con fecha igual o anterior a la OT. Es solo frontend y no arriesga la migración, pero es una **aproximación**: si el cliente tuvo dos recetas el mismo día, la pantalla podría mostrar una graduación que no es la que se fabricó — inaceptable en un dato clínico. Descartada por el usuario.

**Para la vista**, se evaluó reproducir el legacy literalmente con un `MatDialog` de solo lectura abierto desde el listado, conservando la ficha ruteada aparte para operar la orden. Descartada: deja dos pantallas mostrando lo mismo, con el costo de mantenimiento duplicado.

**Para el listado**, se evaluó mantener la carga de la primera página al entrar (el patrón de los otros cuatro listados del sistema, que ya es paginado server-side y no sufre el timeout del legacy). Descartada: una primera página arbitraria de 12.578 órdenes no le sirve a nadie, y el usuario pidió explícitamente el comportamiento del legacy.

## Decisión

### 1. La receta se vincula a la OT (`OPT_RecetaCristales.OrdenDeTrabajoId`)

Se recupera el `idOT` del legacy como `OrdenDeTrabajoId` (`int NULL`, FK `FK_RecetasCristales_OrdenesDeTrabajo`, índice `IX_RecetasCristales_OrdenDeTrabajoId` — script `006_receta_ot_detalle_comentario.sql`).

- **Nullable a propósito:** la ficha clínica puede tomar una receta que todavía no se emite en ninguna orden (en el legacy son las filas con `idOT = NULL`).
- **La receta sigue colgando del Cliente.** `ClienteId` sigue siendo obligatorio: el vínculo con la OT es adicional, no lo reemplaza. La receta no pasa a ser un hijo del agregado OT — es un recurso clínico propio, con su `PublicId` (ADR `0004`), y por eso sí puede direccionarse por sí sola.
- **La relación es 1:N**, no 1:1: 2 de las 12.574 órdenes migradas tienen dos recetas. El DTO expone una lista.
- El vínculo vive en la receta, no en la orden. `Crear`/`ActualizarOrdenDeTrabajoCommand` aceptan `RecetaPublicId` y validan que la receta sea **del mismo cliente** de la OT: vincular la de otra persona sería fabricar cristales con la graduación equivocada.
- En `Actualizar`, `RecetaPublicId` es **estado final**, igual que `EmpresaPublicId`: si no viene, la orden queda sin receta. Es coherente con el resto del comando, y queda registrado como gotcha porque es la clase de contrato que se olvida al escribir un cliente de la API.

### 2. `OPT_DetalleOT.Comentario` (`nvarchar(200)`)

Se agrega la anotación libre por línea del legacy. El usuario no respondió esta pregunta directamente (respondió, en cambio, que "Lentes" es simplemente el **Detalle** — el nombre del legacy no es el correcto), y se resolvió por defecto a favor de conservar el dato: ya se estaba tocando el esquema, y son 11.168 líneas históricas con contenido real que si no se migraba se perdía. Se le informó explícitamente para que pudiera revertirlo.

### 3. La vista "Ver Orden" es la ficha ruteada, con las pestañas del legacy

`/ordenes-de-trabajo/:publicId` adopta el layout del modal legacy —cabecera de la orden arriba, a todo el ancho y en solo lectura; pestañas debajo— con **Cliente · Receta · Detalle · Abonos** (las del legacy) más **Pagos · Cuotas · Bitácora**, que son propias del sistema nuevo. Sigue siendo una página, no un modal: es también donde se opera el flujo de estados (ADR `0007`).

Las pestañas Cliente y Receta se pintan con lo que ya trae `OrdenDeTrabajoDto` — que gana `Cliente` (`ClienteOTDto`, con comuna y región **resueltas en el backend**: hacerlo en el frontend obligaría a bajar las 346 comunas) y `Recetas` (`IReadOnlyList<RecetaCristalesDto>`). No se hace ninguna llamada adicional.

### 4. El listado de OT no consulta hasta que haya un criterio

La pantalla arranca en un estado vacío ("Busca una orden de trabajo") y consulta recién con la primera búsqueda o filtro. Llegar con un filtro de contexto en la URL (`clientePublicId`, `empresaPublicId`, `soloConSaldo` — desde la ficha del cliente o desde Cobranza) **sí** dispara la carga: ya es un criterio. Quitar ese contexto devuelve la pantalla al estado inicial en vez de traer todo.

Es una excepción acotada al patrón de listados del proyecto, justificada por el volumen. No se extiende a los otros listados (Clientes, Empresas, Usuarios, Sucursales), donde entrar y ver la primera página sí es útil.

## Consecuencias

**A favor:**

- La pestaña Receta responde la pregunta correcta —la graduación con la que se fabricaron *estos* cristales— y no una aproximación por fecha.
- Se recuperan dos datos históricos que la migración había dejado fuera: 12.574 vínculos receta↔OT y 11.168 comentarios de línea, ambos al 100% de lo esperado.
- El backfill (`migracion/M006_backfill_receta_ot_detalle_comentario.sql`) es autoverificable: el mapeo receta legacy→destino es **ordinal** (`OPT.Migracion` insertó las 13.183 en orden y sin saltarse ninguna) y el script lo contrasta fila a fila contra `ClienteId` y `FechaIngreso` antes de escribir, abortando si dejara de calzar.
- El listado deja de ser una consulta cara y sin propósito al entrar a la pantalla.

**Costos y riesgos aceptados:**

- **Un script de esquema más escrito a mano** (como `004` y `005`), con la regla asociada: las entidades y sus `IEntityTypeConfiguration` se actualizaron en la **misma** sesión, así que el modelo de EF Core y `dbOPT_NET` siguen alineados. Si se escribe otro sin hacerlo, la desalineación vuelve.
- **`RecetaPublicId` como estado final** es fácil de omitir por accidente y desvincula la receta en silencio. Mitigado con el gotcha documentado y con el formulario, que preselecciona y reenvía la receta actual.
- **El comentario del detalle se re-derivó**, no se copió por id: `M004` no conservó el `idOTDetalle` legacy, así que la correspondencia se reconstruyó por `(OT, Producto, Cantidad, ValorUnitario)` con `ROW_NUMBER` de desempate. Si una OT tuviera dos líneas idénticas con comentarios distintos, el par podría intercambiarse entre esas dos líneas — no se pierde ni se inventa dato, pero no es una copia bit a bit. Lección registrada en `.agents/context/migracion-datos-legacy.md`: **preservar el id de origen** en los migradores futuros.
- **La receta ya no es exclusivamente "del cliente"** al leer el modelo: quien escriba una consulta clínica nueva debe decidir si le interesan todas las recetas del cliente o solo las de una orden.
- **Queda sin verificar por HTTP**: los usuarios migrados conservan la clave de 4 caracteres del legacy y `LoginCommandValidator` exige 6, así que hoy no hay credenciales usables en `dbOPT_NET` para una sesión autenticada. Los datos se verificaron directamente en SQL contra las capturas del legacy.
