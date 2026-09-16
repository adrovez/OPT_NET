# 0011 — API del módulo Operativo: agregado propio, snapshot de montos y reuso de roles

**Estado:** Aceptada
**Fecha:** 2026-09-15

## Contexto

La sesión anterior (2026-09-15, misma fecha) dejó solo el esquema del módulo Operativo (`009_modulo_operativo.sql`, ver `.agents/context/modulo-operativo.md`): 4 tablas nuevas sin ninguna entidad de `OPT.Domain` que las conociera — la misma deuda de alineación que dejó `004_comercial_pagos_cuotas.sql` con `Pago`/`Cuota`/`EstadoCuota` hasta que el ADR `0007` la cerró.

Esta sesión construye `OPT.Domain`/`OPT.Application`/`OPT.API` completos para el módulo, a partir de la traducción de convenciones y las 4 decisiones ya cerradas con el usuario en `.agents/context/modulo-operativo.md` § 3 (correlativo autogenerado, `SucursalId` obligatorio, campos de `GastoOperativo`, transición a `ANULADO` solo desde `PROSPECTO`/`INGRESADO`). Quedaban por resolver, sin acceso al usuario durante la sesión, los puntos abiertos 8.1, 8.3 y 8.7 del requerimiento — se resolvieron por analogía directa con patrones ya aceptados en `OrdenDeTrabajo` (ADR `0007`), documentando la decisión en vez de preguntarla de nuevo.

## Decisión

### 1. `Operativo` es un agregado propio que NO referencia `OrdenDeTrabajo` en `OPT.Domain`

`Operativo` (raíz), `OperativoOT` y `GastoOperativo` viven en `OPT.Domain/Entities/Operativo/`, un namespace separado de `OPT.Domain/Entities/Comercial/`. `Operativo` no tiene ninguna propiedad de navegación ni referencia de tipo a `OrdenDeTrabajo`: la relación es solo el `int OrdenDeTrabajoId` en `OperativoOT`. Esto evita acoplar dos agregados de módulos distintos dentro del dominio — la única forma en que `OrdenDeTrabajo` "entra" al agregado Operativo es a través de valores primitivos que la capa de Application le pasa explícitamente.

*Alternativa descartada:* que `Operativo` cargue y navegue `OrdenDeTrabajo` como si fuera parte del mismo agregado (como hace `OrdenDeTrabajo` con sus propios `Abono`/`Pago`/`Cuota`/`DetalleOT`). Se descartó porque `OrdenDeTrabajo` ya es raíz de su propio agregado (ADR `0007`) — dos raíces no pueden compartir la misma colección hija sin romper la regla de "un agregado, una raíz".

### 2. `MontoTotalVendido`/`MontoTotalPagado` se calculan desde un *snapshot* en `OperativoOT`, no en vivo

`OPT_OperativoOT` gana `MontoVendidoSnapshot`/`MontoPagadoSnapshot` (`decimal(18,2)`, ambos con `DEFAULT 0`) — columnas que **no estaban** en el script `009` original de la sesión de esquema. Se agregaron en esta sesión porque, sin persistir el precio/abonado de la OT en el momento de asociarla, `Operativo.RecalcularTotales()` no tiene de dónde sumar sin volver a consultar `OrdenDeTrabajo` en cada operación — lo que habría forzado a inyectar `IOrdenDeTrabajoRepositorio` dentro de `OPT.Domain.Entities.Operativo.Operativo`, violando la decisión 1.

El snapshot se fija al asociar la OT (`Operativo.AsociarOrden`, con el `Precio`/`TotalAbonado` vigentes que le pasa el handler) y se **refresca explícitamente** vía `POST /api/operativos/{publicId}/recalcular-montos` (`Operativo.RecalcularMontosDesdeOT`). Esto resuelve el punto abierto 8.1 del requerimiento, con una limitación documentada: un abono/pago registrado en una OT **después** de asociarla a un Operativo no actualiza sola el total del Operativo — hay que llamar al endpoint de refresco.

*Alternativa descartada:* hacer que `OrdenDeTrabajo.RegistrarAbono`/`RegistrarPago` (ya implementados, ADR `0007`) busquen si la OT tiene un Operativo asociado y lo actualicen en la misma transacción. Se descartó por alcance: habría requerido modificar código ya en producción del módulo Comercial para una funcionalidad que el requerimiento no pidió como sincronización automática, y habría acoplado el agregado Comercial al agregado Operativo en la dirección contraria a la decisión 1. Queda como mejora candidata si el negocio pide que el total se vea siempre actualizado sin un paso manual.

### 3. Flujo de estados: avance secuencial sin retroceso; `ANULADO` solo desde el inicio

`PROSPECTO(1) → INGRESADO(2) → COBRANZA(3) → CERRADO(4)`, con `ANULADO(5)` como salida alcanzable solo desde `PROSPECTO` o `INGRESADO` (decisión ya cerrada con el usuario, ver `.agents/context/modulo-operativo.md` § 3). A diferencia de `EstadosOT` (ADR `0007`), **no hay retroceso de etapa**: el requerimiento no lo pidió para el Operativo, y agregarlo habría sido una funcionalidad no solicitada.

El módulo tampoco tiene una tabla de bitácora análoga a `OPT_BitacoraOT` — el requerimiento no la incluyó y agregarla habría sido un cambio de esquema no pedido en esta sesión (que se limitó a Domain/Application/API sobre el esquema ya definido). El motivo de una anulación queda registrado concatenado en `Operativo.Observacion` en vez de una fila de bitácora propia.

*Alternativa descartada:* reservar una sesión aparte para agregar `OPT_BitacoraOperativo` antes de escribir el dominio. Se descartó por alcance — es una mejora candidata, no un bloqueante, y el texto del requerimiento nunca la pidió explícitamente.

### 4. Ganancia/pérdida: ambas fórmulas derivadas, ningún campo persistido

El DTO de detalle (`OperativoDto`/`OperativoResumenDto`) expone `GananciaPagado` (`MontoTotalPagado − MontoTotalGastos`) y `GananciaVendido` (`MontoTotalVendido − MontoTotalGastos`), calculadas en `OperativoDtoFactory`/los query handlers — nunca persistidas. Resuelve el punto abierto 8.3 sin comprometerse a una única fórmula que el negocio todavía no confirmó.

### 5. Permisos por rol: reuso directo de los grupos existentes de `RolesOPT`, sin grupo nuevo

`OperativosController` usa `RolesOPT.Administrador/Supervisor/JefeSucursal/Vendedor/Operador` para las operaciones normales (mismo conjunto que `OrdenesDeTrabajoController` usa para su flujo estándar) y restringe `Anular` a `Administrador/Supervisor/JefeSucursal` (mismo criterio que `AnulacionComercial`). No se agregó un grupo `OperacionOperativo` nuevo a `RolesOPT` porque el conjunto de roles resultó ser idéntico al ya usado para OT — introducir un alias hubiera sido indirección sin beneficio.

### 6. Filtro por Operativo en Cobranza: se resuelve extendiendo el listado de OT, no `CobranzaController`

El requerimiento (sección 6) pide poder filtrar Cobranza por Operativo. `CobranzaController.ObtenerDeudores` ya no tiene detalle propio por deudor — reusa `GET /api/ordenes-de-trabajo?empresaPublicId=...` (ver comentario en el propio controller). Se agregó `OperativoPublicId` a `ObtenerOrdenesDeTrabajoQuery` y `operativoId` a `IOrdenDeTrabajoRepositorio.BuscarPaginadoAsync`, exactamente el mismo mecanismo que ya existe para `empresaPublicId` — sin tocar `CobranzaController` ni duplicar la proyección de la OT.

El filtro por Operativo en el "reporte de cristales" que menciona el requerimiento **no se implementó**: ese reporte no existe como endpoint en el sistema nuevo (no hay ningún controller ni query que lo materialice todavía), así que no hay nada que filtrar.

## Consecuencias

**Beneficios**

- El modelo de EF Core y `dbOPT_NET` quedan alineados en la misma sesión que se documentó el esquema (a diferencia de `004`, no queda una brecha de sesión a sesión) — una vez que se aplique `009_modulo_operativo.sql`, que ya incluye las columnas de snapshot.
- `Operativo` es un agregado real: las OT asociadas y los gastos solo se modifican a través de él, con las mismas garantías de recálculo transaccional que `OrdenDeTrabajo` (ADR `0003`/`0006`).
- Cobranza-por-Operativo funciona sin nueva superficie de API, reusando el filtro que el listado de OT ya exponía para empresa.

**Costos y riesgos aceptados**

- **El total del Operativo puede quedar desactualizado.** Un abono o pago registrado en una OT ya asociada no actualiza `MontoTotalVendido`/`MontoTotalPagado` hasta que alguien llame `POST /recalcular-montos`. Es una limitación conocida y documentada (ver decisión 2), no un bug — pero el frontend deberá dejarlo claro en la UI o llamar al endpoint automáticamente al entrar a la ficha del Operativo.
- **Sin bitácora propia.** A diferencia de la OT, un cambio de estado del Operativo no queda registrado con quién/cuándo más allá de los campos de auditoría estándar (`ModificadoEn`/`ModificadoPor`); el motivo de una anulación vive concatenado en un campo de texto libre, no en una tabla consultable.
- **Sin verificación end-to-end.** El script `009_modulo_operativo.sql` no se aplicó a `dbOPT_NET` en esta sesión (ni en la anterior); todo lo que se construyó se verificó con `dotnet build OPT.sln` y `dotnet ef dbcontext info` (el modelo de EF Core valida), no contra una base real ni con una sesión autenticada.
- ~~**Sin frontend.**~~ Resuelto en la sesión 2026-09-15 (2ª): `src/frontend/src/app/features/operativos/` completo (ver `src/frontend/CLAUDE.md` § "Módulo Operativo" y `.agents/context/modulo-operativo.md` § 5a). El riesgo del punto anterior (montos desactualizados) se mitigó con un botón manual "Recalcular montos" en la ficha, **no** con una llamada automática al entrar — se prefirió no ocultar la limitación disparando una petición extra silenciosa en cada carga de la ficha; si el negocio la nota molesta, ahí sí conviene automatizarla.

## Referencias

- Script de esquema: `src/basedatos/009_modulo_operativo.sql` (extendido en esta sesión con `MontoVendidoSnapshot`/`MontoPagadoSnapshot`)
- Dominio: `OPT.Domain/Entities/Operativo/` (`Operativo`, `OperativoOT`, `GastoOperativo`, `EstadoOperativo`, `EstadosOperativo`)
- Casos de uso: `OPT.Application/Features/Operativos/`, `OPT.Application/Features/EstadosOperativo/`
- API: `OPT.API/Controllers/OperativosController.cs`, `EstadosOperativoController.cs`
- Contexto de negocio: `.agents/context/modulo-operativo.md`
- ADRs relacionados: `[[0003-mejoras-base-de-datos]]`, `[[0004-cumplimiento-ley-21719]]`, `[[0007-api-comercial-flujo-estados-ot]]`
