# Módulo Operativo — Operativos Oftalmológicos en terreno

Requerimiento levantado: 2026-09-15. Estado: **Etapas 1 (esquema), 2 (Domain/Application/API) y 3 (frontend) completadas, misma fecha** — solo falta aplicar el script `009` a `dbOPT_NET` y verificar todo end-to-end contra una base real. Ver ADR `[[0011-api-modulo-operativo]]` para las decisiones de diseño de la Etapa 2 (agregado propio, snapshot de montos, flujo sin retroceso, reuso de roles) y `src/frontend/CLAUDE.md` § "Módulo Operativo" para el detalle de la Etapa 3. Leer este documento completo antes de continuar cualquiera de esas etapas: fija las decisiones ya cerradas con el usuario para no volver a preguntarlas.

## 1. Qué problema resuelve

Se realizan Operativos Oftalmológicos en las empresas de los clientes (jornadas de atención en terreno). Hoy el sistema no modela el Operativo como entidad:

- Las OT generadas en terreno se ingresan asociadas a una Empresa, pero no quedan agrupadas bajo el evento/jornada que las originó.
- El reporte de cristales solo filtra por rango de fecha (no por Operativo).
- Cobranza extrae el listado de OT por rango de fecha (debería poder hacerlo por Operativo).
- Cobranza registra manualmente en Excel qué OT fueron pagadas total o parcialmente — no existe ese registro en el sistema.
- No hay forma de comparar vendido/cobrado vs. gastos de una jornada para saber si un Operativo dejó ganancia o pérdida.

Objetivo: agrupar OT bajo un Operativo, registrar sus gastos, filtrar cobranza y reporte de cristales por Operativo, y calcular ganancia/pérdida.

## 2. Origen del documento — traducción de convenciones obligatoria

El requerimiento original (`OPT_Requerimiento_Modulo_Operativo.md`, aportado por el usuario) **fue escrito para otro proyecto**, no para OPT_NET — usa convenciones que no existen acá. Antes de tomar cualquier decisión de este documento al pie de la letra, tradúzcala con esta tabla:

| El requerimiento dice | En OPT_NET es | Por qué |
|---|---|---|
| `TenantId` en toda tabla, multi-tenant | No existe — no se agrega | OPT_NET no es multi-tenant |
| PK `UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID()` | PK `int IDENTITY` (`AuditableEntity.Id`) + `PublicId` (`Guid`, `DEFAULT NEWID()`) solo en recursos de primer nivel expuestos por la API | ADR `0004`; `NEWSEQUENTIALID()` nunca se usó en este esquema |
| `IsDeleted` | `Eliminado` / `EliminadoEn` / `EliminadoPor` | Nombres de `AuditableEntity` |
| "OrdenTrabajo... figura como módulo pendiente, script `025_`" | **Falso para OPT_NET**: `OrdenDeTrabajo` está completa (agregado con Detalle/Abono/Pago/Cuota/Bitácora, API pública) desde la sesión 2026-08-27 (ADR `0007`). El próximo script correlativo real era `009_` | El documento parece generado sin ver el estado real del repo — **siempre verificar `CLAUDE.md` § Estado actual antes de creer un requerimiento sobre "qué falta"** |
| Header `X-Sucursal-Id` obligatorio, módulos "Agenda"/"Stock" | No existen. La autorización por sucursal real es `AutorizacionSucursal.ValidarAcceso` (`OPT.Application/Common/Security/`), sobre la sucursal asignada al usuario vía JWT | Ver `.agents/context/seguridad-apis.md` |
| `PATCH /api/operativos/{id}/estado`, patrón "usado en Agenda y Atención" | Esos módulos no existen; el patrón real a imitar es `POST /api/ordenes-de-trabajo/{publicId}/estado` de `OrdenesDeTrabajoController` | — |
| Signal Forms, IDs `string` (UUID) en frontend | El frontend usa Angular standalone/zoneless con Signals para estado (no "Signal Forms" como forms API), y expone `PublicId` como `string` (Guid) solo para las 6 entidades del ADR `0004` — no todos los IDs | `src/frontend/CLAUDE.md` |

**Lección para sesiones futuras**: un documento de requerimiento externo puede tener su propio vocabulario de arquitectura de otro proyecto. Nunca aplicar sus convenciones de esquema/código sin contrastarlas primero contra `CLAUDE.md` y el estado real de `src/backend/`.

Una consecuencia concreta y favorable de la traducción: como `OrdenDeTrabajo` ya existe y ya tiene `TotalAbonado` (abonos + pagos, recalculado transaccionalmente) y `Saldo`, el punto abierto 8.2 del requerimiento ("¿cómo se calcula Monto Total Pagado?") queda resuelto sin tocar `OrdenDeTrabajo`: `MontoTotalPagado` de un Operativo = `SUM(OT.TotalAbonado)` de las OT vinculadas. No hizo falta agregar `EstadoPago`/`MontoPagado` a la OT como sugería el documento.

## 3. Decisiones cerradas con el usuario (2026-09-15)

Resuelven los puntos abiertos de la sección 8 del requerimiento original que bloqueaban el diseño del esquema:

| Punto | Decisión | Detalle |
|---|---|---|
| 8.5 Correlativo | **Autogenerado por la BD** | `SEQUENCE SEQ_CorrelativoOperativo`, no se ingresa manualmente (a diferencia de `OrdenDeTrabajo.NumeroOT`, que sí es manual desde `008_numero_ot_manual.sql` — no confundir los dos patrones) |
| 8.8 Alcance de Sucursal | **`SucursalId` obligatorio en `Operativo`** | Igual que `OrdenDeTrabajo` — permite reusar `AutorizacionSucursal.ValidarAcceso` tal cual cuando se implemente la API, sin diseñar un mecanismo de autorización nuevo |
| 8.4 Campos de `GastoOperativo` | **Solo Monto + NúmeroDocumento + Observación** | Tal como estaba redactado en el documento — sin fecha propia (se ubica temporalmente por el Operativo) ni categoría/catálogo nuevo |
| 8.6 Transición a Anulado | **Solo desde PROSPECTO o INGRESADO** | Una vez en COBRANZA ya no se puede anular el Operativo, solo Cerrar. Regla de flujo — vive en `OPT.Domain` cuando se implemente (análoga a `OrdenDeTrabajo.CambiarEstado`/`EstadosOT`), **no se modela como CHECK constraint** en la base |

Cobertura de `PublicId` (mismo criterio del ADR `0004`, aplicado por analogía sin haberlo preguntado explícitamente — revisar si el usuario objeta): `Operativo` es recurso de primer nivel → tiene `PublicId`. `OperativoOT` y `GastoOperativo` se exponen anidados bajo `/api/operativos/{publicId}/...`, un padre ya protegido → **sin** `PublicId`, mismo criterio que `Abono`/`Pago`/`Cuota`/`DetalleOT` bajo `OrdenDeTrabajo`.

## 4. Esquema — `src/basedatos/009_modulo_operativo.sql`

**No aplicado aún a `dbOPT_NET`** (pendiente correr el script). El script fue **extendido en la sesión de Domain/Application/API** (misma fecha) con dos columnas que la Etapa 1 no incluía — ver el porqué en el ADR `[[0011-api-modulo-operativo]]` § decisión 2. Contenido actual:

1. **`OPT_EstadoOperativo`** — catálogo (`CatalogEntity`: Id + Nombre). Seed: `1 PROSPECTO`, `2 INGRESADO`, `3 COBRANZA`, `4 CERRADO`, `5 ANULADO`.
2. **`SEQ_CorrelativoOperativo`** — `SEQUENCE int START WITH 1`, fuente del `DEFAULT` de `Operativo.Correlativo`.
3. **`OPT_Operativo`** (`AuditableEntity` + `PublicId`):
   `EmpresaId` (FK `OPT_Empresa`, obligatorio), `SucursalId` (FK `OPT_Sucursal`, obligatorio), `EstadoOperativoId` (FK, default `1` PROSPECTO), `Correlativo` (único, autogenerado), `Fecha` (`date`), `Observacion` (`nvarchar(500)` NULL), `MontoTotalVendido`/`MontoTotalPagado`/`MontoTotalGastos` (`decimal(18,2)`, default `0`, **persistidos**).
4. **`OPT_OperativoOT`** — tabla de unión (sin auditoría, mismo patrón que `OPT_EmpresaSucursal`/`OPT_UsuarioSucursal`): `OperativoId` (FK, `ON DELETE CASCADE`), `OrdenDeTrabajoId` (FK, `ON DELETE NO ACTION`), con **índice único en `OrdenDeTrabajoId`** — una OT pertenece a lo sumo un Operativo. **Agregadas en la sesión de dominio**: `MontoVendidoSnapshot`/`MontoPagadoSnapshot` (`decimal(18,2)`, `DEFAULT 0`) — el precio/abonado de la OT al momento de asociarla (o del último refresco vía `POST /recalcular-montos`); sin esto, `Operativo` no podía mantener sus totales sin referenciar `OrdenDeTrabajo` desde `OPT.Domain`.
5. **`OPT_GastoOperativo`** (`AuditableEntity`): `OperativoId` (FK), `Monto` (`decimal(18,2)`), `NumeroDocumento` (`nvarchar(50)` NULL), `Observacion` (`nvarchar(500)` NULL).

Índices: `UQ_Operativos_PublicId`, `UQ_Operativos_Correlativo`, `IX_Operativos_{EmpresaId,SucursalId,EstadoOperativoId,Fecha}`, `UQ_OperativoOT_OrdenDeTrabajoId`, `IX_OperativoOT_OperativoId`, `IX_GastosOperativo_OperativoId`.

Registrado en `src/basedatos/README.md` (tabla "Estado actual") y en `CLAUDE.md` (§ Pendiente, punto 7).

## 5. Dominio, Application y API — completados (misma sesión)

Todo el detalle de diseño está en el ADR `[[0011-api-modulo-operativo]]`; resumen de dónde vive cada pieza:

- **`OPT.Domain/Entities/Operativo/`** — `Operativo` (agregado raíz; **no** referencia `OrdenDeTrabajo`, ver decisión 1 del ADR), `OperativoOT`, `GastoOperativo`, `EstadoOperativo`, `EstadosOperativo` (flujo `Prospecto→Ingresado→Cobranza→Cerrado`, sin retroceso; `Anulado` solo desde `Prospecto`/`Ingresado`).
- **`OPT.Domain/Interfaces/Repositories/`** — `IOperativoRepositorio`, `IEstadoOperativoRepositorio`; implementaciones en `OPT.Infrastructure/Persistence/Repositories/`, configuraciones EF en `OPT.Infrastructure/Persistence/Configurations/Operativo/`.
- **`OPT.Application/Features/Operativos/`** — Commands: Crear, Actualizar, CambiarEstado, Anular, AsociarOrden, QuitarOrden, RecalcularMontos, RegistrarGasto, EliminarGasto. Queries: ObtenerTodos (paginado), ObtenerPorId. `OperativoDtoFactory` ensambla la vista completa (registrada a mano en `AddApplication`, igual que `OrdenDeTrabajoDtoFactory`).
- **`OPT.Application/Features/EstadosOperativo/`** — catálogo de solo lectura.
- **`OPT.API/Controllers/OperativosController.cs`** (`/api/operativos`, siempre por `PublicId`) y **`EstadosOperativoController.cs`**.
- **Filtro por Operativo en Cobranza** — `OperativoPublicId` en `ObtenerOrdenesDeTrabajoQuery` + `operativoId` en `IOrdenDeTrabajoRepositorio.BuscarPaginadoAsync`, mismo mecanismo que ya usaba `empresaPublicId`. `CobranzaController` no se tocó.

Puntos abiertos del requerimiento, todos resueltos por decisión de esta sesión (sin `AskUserQuestion` — documentados como tal en el ADR `0011`, revisar si el usuario objeta):

- **8.1** `MontoTotalVendido`/`MontoTotalPagado` — snapshot en `OperativoOT`, refrescado a demanda (no hay sincronización automática si se abona/paga una OT ya asociada — ver ADR `0011` § riesgos aceptados).
- **8.3** Fórmula de ganancia/pérdida — se calculan **ambas** (`Pagado−Gastos` y `Vendido−Gastos`) en el DTO, sin persistir columna.
- **8.7** Permisos — se reusaron los grupos de `RolesOPT` ya existentes (`OperacionComercial`/`AnulacionComercial`), sin crear un grupo nuevo para Operativo.
- **Invariantes de negocio** que el requerimiento pedía y no se modelaron como constraint de BD: "una OT con Operativo debe tener Empresa" vive en `AsociarOrdenAOperativoCommandHandler`; "no se puede ingresar un gasto si el Operativo está ANULADO" vive en `Operativo.RegistrarGasto`.
- **Sin bitácora propia** — el módulo no tiene una tabla análoga a `OPT_BitacoraOT` (el requerimiento no la pidió); el motivo de una anulación queda concatenado en `Operativo.Observacion`.

Lo que **sigue** sin resolver:

- **Migración de OT históricas a Operativos** — fuera de alcance (sección 7 del requerimiento original); su propio script `M0NN_*.sql` cuando se aborde.
- **Filtro por Operativo en el reporte de cristales** — no implementado porque ese reporte no existe como endpoint en el sistema nuevo.
- **Verificación end-to-end** — solo se corrió `dotnet build OPT.sln` y `dotnet ef dbcontext info` (el modelo valida). El script `009` no se aplicó a ninguna base real; no hubo sesión autenticada de prueba.

## 5a. Frontend — completado (sesión 2026-09-15, 2ª)

`src/frontend/src/app/features/operativos/` — modelos, servicios (`Operativos`, `EstadosOperativo`), listado paginado, diálogo de alta/edición (`OperativoForm`) y ficha ruteada (`operativo-ficha`, pestañas Órdenes/Gastos). Detalle completo de las decisiones de UI en `src/frontend/CLAUDE.md` § "Módulo Operativo (sesión 2026-09-15, 2ª)" — no se repite acá para no duplicar la fuente de verdad. Resumen de lo relevante para quien siga con este módulo:

- Alta/edición es diálogo (no página ruteada) porque el formulario es chico — Empresa/Sucursal inmutables tras crear.
- El flujo de estados en la ficha solo permite avanzar (nunca retroceder), reflejando que `EstadosOperativo` del dominio no admite volver atrás — a diferencia de la ficha de OT, que sí ofrece retroceder una etapa.
- Asociar una OT reutiliza `<app-selector-orden>` ya existente (envuelto en `AsociarOrdenDialog`), sin duplicar ese buscador.
- El chip de estado no usa tokens de color nuevos (`--opt-estado-operativo-*`): reutiliza `.opt-chip--info/si/alerta` de `styles.scss`. Si el negocio pide diferenciar visualmente más de 3 tonos entre los 5 estados, ahí sí corresponde abrir el proceso de tokens dedicados (auditoría WCAG primero, branding doc).
- Se agregó el filtro de contexto `operativoPublicId` al listado de OT existente (`FiltrosOrdenesDeTrabajo`, `OrdenesDeTrabajo.buscar()`, `ordenes-de-trabajo-list`) — el backend ya lo soportaba desde la Etapa 2, solo faltaba consumirlo.
- Verificado con `npm run lint`/`npm run build` (limpios) y `npm test` (65/66 archivos, 105/107 tests — los 2 que fallan son de `orden-de-trabajo-form.spec.ts`, preexistentes a esta sesión, no relacionados con Operativo). **No verificado en navegador contra `dbOPT_NET` real** — sigue bloqueado en que se aplique `009_modulo_operativo.sql` (punto pendiente de la Etapa 1/2, ver arriba).

## 6. Próximos pasos para completar el módulo

1. Aplicar `009_modulo_operativo.sql` (ya con las columnas de snapshot) contra `dbOPT_NET` (con `sqlcmd`, ver `CLAUDE.md` § Comandos) y confirmarlo en este documento y en `src/basedatos/README.md`.
2. Probar los endpoints de `OperativosController` y el frontend con una sesión autenticada real contra `dbOPT_NET`.
3. Migración de OT históricas a Operativos (fuera de alcance hasta ahora) y filtro de Operativo en las pantallas de Cobranza si el negocio lo pide (hoy `CobranzaController` no expone ese parámetro).
4. Si el negocio pide que el total del Operativo se actualice solo al abonar/pagar una OT ya asociada, revisar la decisión 2 del ADR `0011` (hoy requiere `POST /recalcular-montos` manual).
5. Confirmar con el usuario los puntos que la Etapa 2 resolvió sin preguntarle (8.1 refresco manual, 8.7 reuso de roles, ausencia de bitácora) — ver ADR `[[0011-api-modulo-operativo]]`.

## 7. Requerimiento original (texto completo, para trazabilidad)

> Fuente: `OPT_Requerimiento_Modulo_Operativo.md`, aportado por el usuario el 2026-09-15. Se conserva tal cual para no perder matices al reinterpretarlo — usar la sección 2 de este documento para traducir sus convenciones antes de aplicarlas.

### 1. Contexto y problema actual

Se realizan Operativos Oftalmológicos en las empresas de los clientes (jornadas de atención en terreno). Hoy:

- No existe en el sistema una entidad "Operativo": las OT (Órdenes de Trabajo) generadas en terreno se ingresan asociadas a una Empresa, pero no quedan agrupadas bajo el evento/jornada que las originó.
- El reporte de cristales se filtra solo por rango de fecha.
- Cobranza extrae el listado de OT por rango de fecha (debería poder hacerlo por Operativo).
- Cobranza registra manualmente en Excel qué OT fueron pagadas totalmente y cuáles parcialmente — no existe este registro en el sistema.
- No existe forma de comparar lo vendido, lo cobrado y los gastos de una jornada para saber si un Operativo dejó ganancia o pérdida.

### 2. Objetivo

Crear el Módulo Operativo para:

1. Agrupar las OT generadas en una jornada/Operativo específico.
2. Registrar los gastos asociados a cada Operativo.
3. Permitir filtrar los reportes de cristales y de cobranza por Operativo.
4. Calcular y mostrar ganancia/pérdida por Operativo (vendido / cobrado vs. gastos).

### 3. Dependencia técnica detectada (según el documento original — no aplica a OPT_NET, ver sección 2 de arriba)

Según el estado actual del proyecto [otro proyecto, no OPT_NET], la entidad OrdenTrabajo (OT) — que es la que se asociaría al Operativo — figuraba como módulo pendiente ("Salida por documentos: OrdenTrabajo, Devoluciones, OtroEgreso — script `025_`"), es decir, aún no estaba implementada en el backend. Implicancia (para ese otro proyecto): el Módulo Operativo no podría completarse hasta que exista la entidad OT.

### 4. Entidades

#### 4.1 Operativo

| Campo | Tipo sugerido | Notas |
|---|---|---|
| OperativoId | `UNIQUEIDENTIFIER` (NEWSEQUENTIALID) | PK, según convención del proyecto original |
| EmpresaId | `UNIQUEIDENTIFIER` (FK Cliente/Empresa) | Obligatorio |
| Fecha | `DATE` | Fecha del Operativo |
| Estado | `VARCHAR` / enum | Ver sección 5 |
| Correlativo | `INT` | Ver punto abierto 8.5 (autogenerado vs. manual) |
| Observacion | `NVARCHAR` | Opcional |
| MontoTotalVendido | `DECIMAL` (calculado o almacenado) | Ver punto abierto 8.1 |
| MontoTotalPagado | `DECIMAL` (calculado o almacenado) | Ver punto abierto 8.2 |
| MontoTotalGastos | `DECIMAL` (calculado) | Suma de GastoOperativo |
| TenantId | `UNIQUEIDENTIFIER` | Multi-tenant, obligatorio (no aplica en OPT_NET) |
| Auditoría | IsDeleted, CreatedBy, CreatedAt, etc. | Estándar del proyecto original |

#### 4.2 OperativoOT (tabla de relación)

| Campo | Tipo sugerido | Notas |
|---|---|---|
| OperativoOTId | `UNIQUEIDENTIFIER` | PK |
| OperativoId | `UNIQUEIDENTIFIER` (FK) | |
| OrdenTrabajoId | `UNIQUEIDENTIFIER` (FK) | |
| TenantId | `UNIQUEIDENTIFIER` | |

Reglas:
- Una OT no está obligada a tener un Operativo asociado.
- Si una OT tiene Operativo asociado, debe tener Empresa asociada (validación cruzada).
- Relación 1 Operativo → N OT.

#### 4.3 GastoOperativo

| Campo | Tipo sugerido | Notas |
|---|---|---|
| GastoOperativoId | `UNIQUEIDENTIFIER` | PK |
| OperativoId | `UNIQUEIDENTIFIER` (FK) | |
| Monto | `DECIMAL` | |
| NumeroDocumento | `VARCHAR` | N° boleta o factura |
| Observacion | `NVARCHAR` | Opcional |
| TenantId | `UNIQUEIDENTIFIER` | |
| Auditoría | Estándar | |

Regla: se puede ingresar un gasto en cualquier estado del Operativo excepto Anulado.

#### 4.4 Impacto en OT (pendiente de confirmar — ver 8.2; resuelto para OPT_NET sin necesidad de estos campos, ver sección 2)

Para poder calcular `MontoTotalPagado` por Operativo, probablemente la OT necesite registrar su estado de pago (hoy vive en un Excel de Cobranza). Sugerido (a validar):

| Campo | Tipo sugerido | Notas |
|---|---|---|
| EstadoPago | enum (`Pendiente`, `PagoParcial`, `PagoTotal`) | |
| MontoPagado | `DECIMAL` | Abono acumulado |

### 5. Estados del Operativo

```
Prospecto → Ingresado → Cobranza → Cerrado
                                  ↘
                                Anulado
```

| Estado | Significado |
|---|---|
| Prospecto | Posible Operativo para la fecha indicada (aún sin OT) |
| Ingresado | Se ingresan las primeras OT |
| Cobranza | El proceso de venta/atención terminó y se está cobrando |
| Cerrado | Cobranza indica el cierre; se cobró todo o parte de las OT |
| Anulado | El Operativo se anula por alguna razón |

Regla de gastos: se pueden ingresar gastos en cualquier estado excepto Anulado.

### 6. Reportes afectados

- Reporte de cristales: agregar filtro por Operativo (además del rango de fecha existente).
- Listado de Cobranza: agregar filtro por Operativo (además del rango de fecha existente).
- Nuevo: vista/detalle de Operativo mostrando Monto Total Vendido, Monto Total Pagado, Monto Total Gastos y Ganancia/Pérdida (fórmula a confirmar, ver 8.3).

### 7. Migración de datos históricos

Es posible asociar las OT ya existentes a Operativos — se abordará en una etapa posterior, no forma parte del alcance inicial de este requerimiento.

### 8. Puntos abiertos (pendientes de confirmar con el cliente antes de codificar)

1. Origen de Monto Total Vendido: ¿se calcula automáticamente sumando el valor de las OT asociadas al Operativo, o se ingresa manualmente?
2. Origen de Monto Total Pagado: ¿el registro de pagos totales/parciales por OT pasa a hacerse dentro del sistema (reemplazando el Excel de Cobranza), permitiendo calcular este monto automáticamente?
3. Fórmula de ganancia/pérdida: ¿`Monto Total Pagado − Monto Total Gastos` (lo efectivamente cobrado), o también se requiere `Monto Total Vendido − Monto Total Gastos` (lo comprometido, esté o no cobrado)? Podrían mostrarse ambos indicadores.
4. Gastos — fecha y categoría: ¿el gasto lleva fecha propia y/o tipo/categoría (arriendo, movilización, insumos), o solo monto + N° documento + observación como se indicó?
5. N° correlativo: ¿autogenerado por el sistema o definido manualmente al crear el Operativo?
6. Transiciones de estado: ¿desde qué estado(s) se puede pasar a Anulado? ¿Es reversible un Cierre? ¿Quién gatilla cada transición (rol)?
7. Permisos: quién puede crear/editar Operativos, ingresar gastos, y cambiar estados — pendiente, se validará más adelante.
8. Alcance de "Sucursal": los módulos de terreno del proyecto original usan `X-Sucursal-Id` obligatorio. ¿El Operativo es sucursal-scoped o es transversal?

### 9. Notas técnicas del documento original (convenciones de OTRO proyecto — no usar sin traducir, ver sección 2)

- PK de entidades de negocio: `UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID()`.
- Soft-delete obligatorio (`IsDeleted`).
- Transiciones de estado vía `PATCH /api/operativos/{id}/estado`.
- Controllers extraen `TenantId` y lo pasan al command.
- DTOs como `record` inmutables (esto sí coincide con la convención real de OPT_NET).
- Frontend: standalone components, Signal Forms, servicio en `core/services/`, IDs `string` (UUID).
