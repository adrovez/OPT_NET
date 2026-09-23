# Requerimiento Funcional — Módulo Operativo (OPT)

**Fecha:** 2026-09-15
**Estado del documento:** Levantamiento inicial — contiene puntos abiertos pendientes de confirmación (ver sección 8)

---

## 1. Contexto y problema actual

Se realizan Operativos Oftalmológicos en las empresas de los clientes (jornadas de atención en terreno). Hoy:

- No existe en el sistema una entidad "Operativo": las OT (Órdenes de Trabajo) generadas en terreno se ingresan asociadas a una Empresa, pero **no quedan agrupadas** bajo el evento/jornada que las originó.
- El reporte de cristales se filtra solo por rango de fecha.
- Cobranza extrae el listado de OT por rango de fecha (debería poder hacerlo por Operativo).
- Cobranza registra manualmente en Excel qué OT fueron pagadas totalmente y cuáles parcialmente — no existe este registro en el sistema.
- No existe forma de comparar lo vendido, lo cobrado y los gastos de una jornada para saber si un Operativo dejó ganancia o pérdida.

## 2. Objetivo

Crear el Módulo Operativo para:

1. Agrupar las OT generadas en una jornada/Operativo específico.
2. Registrar los gastos asociados a cada Operativo.
3. Permitir filtrar los reportes de cristales y de cobranza por Operativo.
4. Calcular y mostrar ganancia/pérdida por Operativo (vendido / cobrado vs. gastos).

## 3. Dependencia técnica detectada ⚠️

Según el estado actual del proyecto OPT, la entidad **OrdenTrabajo (OT)** — que es la que se asociaría al Operativo — figura como **módulo pendiente** ("Salida por documentos: OrdenTrabajo, Devoluciones, OtroEgreso — script `025_`"), es decir, **aún no está implementada** en el backend.

**Implicancia:** el Módulo Operativo no puede completarse (tabla de relación Operativo↔OT) hasta que exista la entidad OT, o bien debe implementarse en paralelo. Confirmar orden de desarrollo antes de codificar.

## 4. Entidades

### 4.1 Operativo

| Campo | Tipo sugerido | Notas |
|---|---|---|
| OperativoId | `UNIQUEIDENTIFIER` (NEWSEQUENTIALID) | PK, según convención del proyecto |
| EmpresaId | `UNIQUEIDENTIFIER` (FK Cliente/Empresa) | Obligatorio |
| Fecha | `DATE` | Fecha del Operativo |
| Estado | `VARCHAR` / enum | Ver sección 5 |
| Correlativo | `INT` | Ver punto abierto 8.5 (autogenerado vs. manual) |
| Observacion | `NVARCHAR` | Opcional |
| MontoTotalVendido | `DECIMAL` (calculado o almacenado) | Ver punto abierto 8.1 |
| MontoTotalPagado | `DECIMAL` (calculado o almacenado) | Ver punto abierto 8.2 |
| MontoTotalGastos | `DECIMAL` (calculado) | Suma de GastoOperativo |
| TenantId | `UNIQUEIDENTIFIER` | Multi-tenant, obligatorio |
| Auditoría | IsDeleted, CreatedBy, CreatedAt, etc. | Estándar del proyecto |

### 4.2 OperativoOT (tabla de relación)

| Campo | Tipo sugerido | Notas |
|---|---|---|
| OperativoOTId | `UNIQUEIDENTIFIER` | PK |
| OperativoId | `UNIQUEIDENTIFIER` (FK) | |
| OrdenTrabajoId | `UNIQUEIDENTIFIER` (FK) | |
| TenantId | `UNIQUEIDENTIFIER` | |

**Reglas:**
- Una OT **no está obligada** a tener un Operativo asociado.
- Si una OT tiene Operativo asociado, **debe** tener Empresa asociada (validación cruzada).
- Relación 1 Operativo → N OT.

### 4.3 GastoOperativo

| Campo | Tipo sugerido | Notas |
|---|---|---|
| GastoOperativoId | `UNIQUEIDENTIFIER` | PK |
| OperativoId | `UNIQUEIDENTIFIER` (FK) | |
| Monto | `DECIMAL` | |
| NumeroDocumento | `VARCHAR` | N° boleta o factura |
| Observacion | `NVARCHAR` | Opcional |
| TenantId | `UNIQUEIDENTIFIER` | |
| Auditoría | Estándar | |

**Regla:** se puede ingresar un gasto en cualquier estado del Operativo **excepto Anulado**.

### 4.4 Impacto en OT (pendiente de confirmar — ver 8.2)

Para poder calcular `MontoTotalPagado` por Operativo, probablemente la OT necesite registrar su estado de pago (hoy vive en un Excel de Cobranza). Sugerido (a validar):

| Campo | Tipo sugerido | Notas |
|---|---|---|
| EstadoPago | enum (`Pendiente`, `PagoParcial`, `PagoTotal`) | |
| MontoPagado | `DECIMAL` | Abono acumulado |

## 5. Estados del Operativo

```
Prospecto → Ingresado → Cobranza → Cerrado
                                 ↘
                               Anulado
```

| Estado | Significado |
|---|---|
| **Prospecto** | Posible Operativo para la fecha indicada (aún sin OT) |
| **Ingresado** | Se ingresan las primeras OT |
| **Cobranza** | El proceso de venta/atención terminó y se está cobrando |
| **Cerrado** | Cobranza indica el cierre; se cobró todo o parte de las OT |
| **Anulado** | El Operativo se anula por alguna razón |

**Regla de gastos:** se pueden ingresar gastos en cualquier estado **excepto Anulado**.

**Puntos abiertos sobre transición de estados** — ver 8.6.

## 6. Reportes afectados

- **Reporte de cristales**: agregar filtro por Operativo (además del rango de fecha existente).
- **Listado de Cobranza**: agregar filtro por Operativo (además del rango de fecha existente).
- **Nuevo:** vista/detalle de Operativo mostrando Monto Total Vendido, Monto Total Pagado, Monto Total Gastos y Ganancia/Pérdida (fórmula a confirmar, ver 8.3).

## 7. Migración de datos históricos

Es posible asociar las OT ya existentes a Operativos — **se abordará en una etapa posterior**, no forma parte del alcance inicial de este requerimiento.

## 8. Puntos abiertos (pendientes de confirmar con el cliente antes de codificar)

1. **Origen de Monto Total Vendido**: ¿se calcula automáticamente sumando el valor de las OT asociadas al Operativo, o se ingresa manualmente?
2. **Origen de Monto Total Pagado**: ¿el registro de pagos totales/parciales por OT pasa a hacerse dentro del sistema (reemplazando el Excel de Cobranza), permitiendo calcular este monto automáticamente? Esto es crítico: sin esto, el sistema no puede calcular ganancia/pérdida real sin ingreso manual.
3. **Fórmula de ganancia/pérdida**: ¿`Monto Total Pagado – Monto Total Gastos` (lo efectivamente cobrado), o también se requiere `Monto Total Vendido – Monto Total Gastos` (lo comprometido, esté o no cobrado)? Podrían mostrarse ambos indicadores.
4. **Gastos — fecha y categoría**: ¿el gasto lleva fecha propia y/o tipo/categoría (arriendo, movilización, insumos), o solo monto + N° documento + observación como se indicó?
5. **N° correlativo**: ¿autogenerado por el sistema o definido manualmente al crear el Operativo?
6. **Transiciones de estado**: ¿desde qué estado(s) se puede pasar a Anulado? ¿Es reversible un Cierre? ¿Quién gatilla cada transición (rol)?
7. **Permisos**: quién puede crear/editar Operativos, ingresar gastos, y cambiar estados — pendiente, se validará más adelante.
8. **Alcance de "Sucursal"**: los módulos de terreno en OPT usan `X-Sucursal-Id` obligatorio (Agenda, Stock). ¿El Operativo es sucursal-scoped o es transversal (ya que ocurre en la empresa del cliente, no en una sucursal propia)?

## 9. Notas técnicas para la sesión de codificación (convenciones del proyecto OPT)

- PK de entidades de negocio: `UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID()` — nunca `NEWID()`.
- Soft-delete obligatorio (`IsDeleted`), nunca DELETE físico.
- Script SQL incremental nuevo en `src/basedatos/` — próximo correlativo depende de si `025_` ya se usó para OrdenTrabajo (ver sección 3); si no, Operativo podría tomar `025_` o correr después.
- Transiciones de estado vía `PATCH /api/operativos/{id}/estado` (patrón usado en Agenda y Atención), no vía PUT genérico.
- Controllers extraen `TenantId` y lo pasan al command; handlers de Application no inyectan `ICurrentTenantService`.
- DTOs como `record` inmutables.
- Frontend: standalone components, Signal Forms, servicio en `core/services/`, IDs `string` (UUID).

---

**Siguiente paso sugerido:** resolver los puntos de la sección 8 con el cliente, definir si OrdenTrabajo se desarrolla antes o en paralelo, y luego generar el script SQL + entidades de dominio.
