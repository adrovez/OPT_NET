# 0006 — Módulo Comercial: tablas `Pago`/`Cuota` y migración por SQL directo

**Estado:** Aceptada
**Fecha:** 2026-08-27

## Contexto

El módulo Comercial del legacy (`db_a25cfd_opt2`) tiene seis tablas de datos transaccionales: `OPT_OrdenDeTrabajo` (12.578), `OPT_OrdenDeTrabajoDetalle` (20.573), `OPT_BitacoraOT` (31.964), `OPT_Abono` (3.277), `OPT_Pago` (3.736) y `OPT_Cuota` (34.110). Al ir a migrarlas aparecieron cuatro brechas contra el esquema Fase 0 de `dbOPT_NET`:

1. **`OPT_Pago` y `OPT_Cuota` no tenían tabla destino.** El scaffold modeló solo `Abono`. El ADR `0003` y `reglas-negocio-legado.md` ya habían marcado como *[Reconsiderar]* la coexistencia de `Abono` y `Pago` en el legacy, sin resolverla.
2. **`OPT_Producto` estaba vacío en destino** y `OPT_DetalleOT.ProductoId` es FK `NOT NULL` — sin productos no hay detalle de OT.
3. **El `Saldo` del legacy no descuenta `OPT_Pago`.** Verificado sobre los datos reales: `OrdenDeTrabajo.Abono` = `SUM(OPT_Abono.Monto)` en 12.577 de 12.578 filas, y `Saldo = Precio − Abono`. Los 293.944.093 de `OPT_Pago` nunca entraron en ese cálculo: 3.472 OT arrastran un saldo mayor al real.
4. **Siete columnas de la OT legacy no tenían destino:** `Beneficiario`, `FechaAtencion`, `HoraEntrega`, `NumeroCuota`, `EstadoPago`, `FormaAbono`, `Usuario`.

Además, el usuario acotó explícitamente la sesión a **"solo base de datos"** — sin tocar `OPT.Domain`, `OPT.Application`, `OPT.Infrastructure`, `OPT.API` ni el frontend.

## Alternativas consideradas

**Para `Pago` y `Cuota`:**

- *Consolidar `Pago` dentro de `OPT_Abono`* (ambos son dinero contra una OT, con `Referencia` marcando el origen). Descartada: pierde la distinción entre el abono inicial y los pagos posteriores, que el legacy sí registra por separado y que el negocio usa distinto.
- *Descartar `Cuota`* porque sus 34.110 filas están todas en `PENDIENTE`. Descartada: el dato útil no es el estado, es el **calendario de vencimientos** (número, valor y fecha de cada cuota), que sí está completo y no es reconstruible desde ninguna otra tabla.

**Para `Producto`:**

- *Migrar el catálogo completo (4.677) + `ProductoSucursal` (13.776)*. Descartada por el usuario: amplía el alcance al módulo Inventario, que no era lo pedido.
- *Migrar la OT sin su detalle*. Descartada: dejaría 20.573 líneas fuera y la OT sin desglose.

**Para el `Saldo`:**

- *Preservar los valores legacy tal cual*, para que los totales del sistema nuevo cuadren exactamente con los reportes históricos. Descartada: perpetúa un saldo incorrecto en 3.472 OT y contradice la regla del proyecto de recalcular todo campo calculado que se persiste.

**Para la ejecución de la migración:**

- *Agregar una fase Comercial a `OPT.Migracion`* (el camino usado en Organización y Clínico, ADR `0005`). Descartada **solo para esta sesión** por el alcance "solo base de datos" que fijó el usuario.

## Decisión

1. **Crear `OPT_Pago` y `OPT_Cuota`** como tablas propias con forma `AuditableEntity`, más el catálogo `OPT_EstadoCuota` (PENDIENTE / PAGADA / ANULADA) — el estado de cuota era texto libre en el legacy y la regla del proyecto prohíbe eso. Se agrega también `OPT_FormaPago (5, 'CHEQUE')`, valor que el legacy usaba como texto libre sin tenerlo en su catálogo.
2. **Migrar solo los 4.018 productos** referenciados por algún detalle de OT, sin `ProductoSucursal`.
3. **Recalcular** `TotalAbonado = abonos + pagos` y `Saldo = Precio − TotalAbonado`.
4. **Agregar a `OPT_OrdenDeTrabajo`** las columnas `Beneficiario`, `FechaAtencion`, `HoraEntrega` y `NumeroCuotas`. `EstadoPago` y `FormaAbono` se descartan (derivables, y sucios: 1.565 vacíos y 2.050 "SIN INFORMACION"); `Usuario` se resuelve a `CreadoPor`.
5. **Ejecutar la migración con dos scripts SQL** en vez de `OPT.Migracion`: `src/basedatos/004_comercial_pagos_cuotas.sql` (esquema) y `src/basedatos/migracion/M004_datos_comercial.sql` (datos, `INSERT ... SELECT` cross-database en una sola transacción).

Esto **resuelve** el *[Reconsiderar]* de `reglas-negocio-legado.md` sobre `Abono` vs. `Pago`: son dos flujos distintos y se modelan por separado.

## Consecuencias

**Beneficios**

- El módulo Comercial queda migrado y verificado fila a fila (13 comprobaciones cruzadas contra el legacy, 0 discrepancias; los 5 totales de dinero cuadran al peso).
- Los saldos del sistema nuevo son correctos, no heredan el error del legacy.
- El calendario de cuotas y el detalle de pagos se preservan íntegros, no se aplanan en una sola tabla.

**Costos y riesgos aceptados**

- **Desalineación de EF Core.** `004` se escribió a mano, no con `dotnet ef migrations script`. `OPT.Domain` no tiene las entidades `Pago`, `Cuota` ni `EstadoCuota`, ni las 4 propiedades nuevas de `OrdenDeTrabajo`. **Hasta que se creen esas entidades y sus `IEntityTypeConfiguration<T>`, no se puede correr `dotnet ef migrations add`** — sin migration previa, EF genera un script que recrea todo el esquema (ver el gotcha ya documentado en `CLAUDE.md`) y además ignoraría las tablas que no conoce. Es la deuda técnica principal que deja esta decisión.
- **`OPT.Migracion` no sabe que este módulo ya está migrado.** Si alguien le agrega una fase Comercial, debe respetar el guard "si el destino ya tiene OT, omitir" que usan las demás fases.
- **Los totales no cuadran con los reportes del legacy**, por diseño: saldo legacy 1.185.383.027 → nuevo 890.873.420; abonado legacy 246.687.186 → nuevo 540.631.294. Cualquier conciliación histórica tiene que partir de esta diferencia conocida, no tratarla como un error de migración.
- **Filas históricas que violarían invariantes futuras.** Se preservaron tal cual 23 pagos, 20 abonos y 5 cuotas con monto ≤ 0, 33 detalles con `Cantidad ≤ 0` y 18 OT con `Precio` 0. `dbOPT_NET` no tiene ningún CHECK constraint, así que entraron sin resistencia; si el dominio agrega `Monto > 0`, estas filas lo violan.
- **`ProductoSucursal` (13.776) y 659 productos legacy sin uso quedan pendientes** — el módulo Inventario está parcialmente poblado.
- Se abre un segundo camino de migración (SQL) además de `OPT.Migracion`. Para fases futuras, **el camino por defecto sigue siendo `OPT.Migracion`** (ADR `0005`); el SQL directo fue una excepción por el alcance de esta sesión, no un cambio de estrategia.

## Referencias

- Scripts: `src/basedatos/004_comercial_pagos_cuotas.sql`, `src/basedatos/migracion/M004_datos_comercial.sql`
- Contexto operativo y gotchas de datos: `.agents/context/migracion-datos-legacy.md`
- Historial de la sesión con todos los números verificados: `.agents/progress.md`, entrada 2026-08-27
- ADR relacionado: `[[0005-migracion-datos-herramienta-standalone]]`
