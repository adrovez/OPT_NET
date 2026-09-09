# 0007 — API del módulo Comercial: agregado OT, flujo de estados y alcance de `PublicId`

**Estado:** Aceptada
**Fecha:** 2026-08-27

## Contexto

El módulo Comercial tenía la base de datos migrada (ADR `0006`) pero ninguna API: `OrdenesDeTrabajoController` era un stub y `OPT.Domain` no conocía tres tablas que ya existían en `dbOPT_NET` (`OPT_Pago`, `OPT_Cuota`, `OPT_EstadoCuota`) ni las cuatro columnas que `004_comercial_pagos_cuotas.sql` agregó a `OPT_OrdenDeTrabajo`. Además, el ADR `0004` dejó registrado que `OrdenDeTrabajo` **debe** tener `PublicId` **antes** de publicar su primer endpoint.

Al diseñar la API aparecieron cinco decisiones que el scaffold no resolvía y que el legacy o no tenía o resolvía mal. Las cinco se consultaron con el usuario antes de escribir código.

## Decisión

### 1. Alcance de `PublicId`: solo `OrdenDeTrabajo`

`OPT_OrdenDeTrabajo` gana `PublicId` (`uniqueidentifier NOT NULL DEFAULT NEWID()` + índice único, script `005_ot_publicid_estado_anulado.sql`, 12.578 filas pobladas). `Abono`, `Pago`, `Cuota`, `DetalleOT` y `BitacoraOT` **no** lo llevan: se exponen como subrecursos bajo `/api/ordenes-de-trabajo/{publicId}/...`, una ruta ya protegida por el identificador opaco del padre más la autorización del endpoint. Su Id interno solo es alcanzable por quien ya tiene acceso a esa OT.

*Alternativa descartada:* darle `PublicId` también a `Abono`/`Pago`/`Cuota` (prioridad media del ADR `0004`). Se reconsiderará solo si alguna vez se direccionan como recurso de primer nivel.

### 2. Flujo de estados: secuencial con avance y retroceso de a un paso

`INGRESADO(0) → EN PROCESO(1) → MONTAJE(2) → LABORATORIO(3) → CALIDAD(4) → DESPACHO(5) → ENTREGADO(6)`.

- Avanza **al estado siguiente**; no se saltan etapas.
- Retrocede **una sola etapa**, y solo con **observación obligatoria**, que queda en `OPT_BitacoraOT` como justificación.
- `ENTREGADO` y `ANULADO` son **terminales**.
- Toda transición registra bitácora con estado anterior y nuevo (el legacy solo guardaba el nuevo).

*Alternativa descartada:* transición libre entre cualquier par de estados (comportamiento del legacy) — no da ninguna garantía de proceso. También se descartó el avance estricto sin retroceso: obliga a anular la OT por un error de digitación.

### 3. Anulación: estado terminal `ANULADO` en el catálogo, no borrado lógico

Se agrega `OPT_EstadoOT (7, 'ANULADO')`. Anular exige motivo, registra bitácora y deja la OT **visible en los listados** con su historial completo. Es el reemplazo del `SP_OTEliminar` del legacy (lógica de negocio que vivía en la base de datos). Una OT `ENTREGADO` no se puede anular; una OT anulada no admite ninguna modificación posterior.

*Alternativa descartada:* borrado lógico (`Eliminado = true`). Haría desaparecer del listado una OT con dinero asociado, que es justamente lo que hay que poder auditar.

### 4. `Precio` derivado del detalle; `TotalAbonado = abonos + pagos`

El `Precio` **no se recibe** en la API: es la suma de `Cantidad × ValorUnitario` de las líneas vigentes, recalculada por el dominio cada vez que el detalle cambia. `TotalAbonado` suma abonos **y** pagos (corrige el error del legacy, ADR `0006`) y `Saldo = Precio − TotalAbonado`, todo dentro de la misma transacción del movimiento que lo origina (ADR `0003`).

**Se acepta el sobrepago**: un abono o pago puede superar el saldo y dejarlo negativo. El scaffold lo rechazaba con `DomainException`; el negocio pidió permitirlo.

### 5. Cuotas: plan generado por el sistema e imputación automática al pagar

- Al crear la OT con `NumeroCuotas > 0` (o vía `POST {publicId}/cuotas`), el dominio genera N cuotas de valor parejo — la diferencia por redondeo va a la última — con vencimiento mensual desde la fecha indicada.
- **Registrar un pago marca como PAGADA cada cuota pendiente, de la más antigua a la más nueva, mientras el monto restante alcance a cubrirla completa**, en la misma transacción. Un pago parcial baja el saldo pero no marca la cuota.
- Solo puede haber un plan vigente: para rehacerlo hay que anular las cuotas pendientes (estado `ANULADA`).

Esto **cierra el ciclo** que el legacy dejaba abierto: generaba el plan al crear la OT y nunca lo actualizaba, por eso sus 34.110 cuotas quedaron todas en PENDIENTE mientras los cobros reales vivían en `OPT_Pago` (ver `reglas-negocio-legado.md`).

## Consecuencias

**Beneficios**

- **La deuda de EF Core del ADR `0006` queda cerrada.** `OPT.Domain` ya tiene `Pago`, `Cuota` y `EstadoCuota` con sus `IEntityTypeConfiguration<T>`, más `Beneficiario`/`FechaAtencion`/`HoraEntrega`/`NumeroCuotas` en `OrdenDeTrabajo`: el modelo y `dbOPT_NET` vuelven a estar alineados y se puede volver a usar el procedimiento de regeneración de scripts de `CLAUDE.md`.
- La OT es un agregado real: detalles, abonos, pagos, cuotas y bitácora solo se modifican a través de `OrdenDeTrabajo`, que es el único punto donde se recalculan los totales. No hay forma de mover dinero sin que el saldo se actualice en la misma transacción.
- Las reglas de transición viven en el dominio (`EstadosOT` + `OrdenDeTrabajo.CambiarEstado`), no en el controller ni en el frontend. El endpoint de catálogo `/api/estados-ot` expone `esTerminal` para que la UI no las duplique.

**Costos y riesgos aceptados**

- **Datos históricos que violan las invariantes nuevas.** La migración preservó 23 pagos, 20 abonos y 5 cuotas con monto ≤ 0, 33 detalles con `Cantidad ≤ 0` y 18 OT con `Precio` 0 (ADR `0006`). El dominio ahora los rechazaría, pero ya están en la base: cualquier operación que recalcule totales sobre esas OT usará los valores tal cual están. No se agregaron CHECK constraints para no romper la carga histórica.
- **El `Precio` de las OT migradas no se recalcula solo.** Se recalcula la primera vez que alguien edite el detalle de esa OT; hasta entonces conserva el valor que trajo la migración.
- **Sobrepago sin tope.** Al aceptar montos por sobre el saldo, un error de digitación (un cero de más) no lo detiene ninguna validación. Queda registrado y auditable, pero no bloqueado.
- **Autorización por sucursal todavía no implementada.** Los endpoints son `[Authorize]` genérico: cualquier usuario autenticado ve cualquier OT. El ADR `0004` señala que ésa —y no el identificador opaco— es la medida que protege la superficie de `NumeroOT`, que es visible por diseño. Queda pendiente.
- **`005_ot_publicid_estado_anulado.sql` se escribió a mano**, igual que `004`, en vez de generarse con `dotnet ef migrations script`. Con el modelo ya alineado, el próximo cambio de esquema sí debería seguir el procedimiento estándar.

## Referencias

- Script de esquema: `src/basedatos/005_ot_publicid_estado_anulado.sql`
- Dominio: `OPT.Domain/Entities/Comercial/` (`OrdenDeTrabajo`, `Pago`, `Cuota`, `EstadoCuota`, `EstadosOT`, `EstadosCuota`)
- Casos de uso: `OPT.Application/Features/OrdenesDeTrabajo/`
- API: `OPT.API/Controllers/OrdenesDeTrabajoController.cs`, `CatalogosComercialControllers.cs`
- ADRs relacionados: `[[0003-mejoras-base-de-datos]]`, `[[0004-cumplimiento-ley-21719]]`, `[[0006-modulo-comercial-pago-cuota-y-migracion-sql]]`
