# 0012 — Módulo Bodegas: el stock pasa de la Sucursal a la Bodega

**Estado:** Aceptada
**Fecha:** 2026-10-19

## Contexto

Hasta hoy la Sucursal hacía de bodega: el stock colgaba de `OPT_ProductoSucursal` (`StockActual`/`StockMinimo`/`StockMaximo` + `PrecioVenta`, único por `(ProductoId, SucursalId)`). El negocio pide un módulo de Bodegas donde una Sucursal tiene una o más bodegas (los archivos de `doc_Cliente/Inventario` ya hablan de "Bodega 1 Brilaz", "Terreno" y Aquamed).

Hallazgos al analizar el impacto:

- Solo `IProductoRepositorio` (`ObtenerStockAsync`, `ObtenerConStockBajoAsync`) tocaba el stock; `Ingresar`/`Egresar` existían en el dominio pero ningún caso de uso los llamaba. Inventario es un stub en backend y frontend.
- `OPT_ProductoSucursal` está **vacía** en `dbOPT_NET` (pendiente de migración: 13.776 filas en el legacy). Por eso el cambio de esquema es barato ahora y caro después.
- El legacy tiene stock negativo (4.220 filas con stock ≠ 0 en la sucursal 1 que suman −180.737); el modelo nuevo lo prohíbe.
- OT, Operativo, Usuario y la autorización por sucursal usan `SucursalId` como "dónde se vende/atiende": no cambian.

## Decisión

Las cinco decisiones abiertas de la propuesta se cerraron con las recomendaciones del arquitecto (aprobadas por el usuario el 2026-10-19):

1. **`PrecioVenta` queda por sucursal.** `OPT_ProductoSucursal` conserva solo el precio (se vende por sucursal, no por bodega) y pierde las tres columnas de stock.
2. **Stock negativo del legacy → se carga en 0.** El modelo nuevo no admite negativos (`CK_ProductoBodegas_StockNoNegativo` + `Egresar`). El valor original debe quedar en el reporte/auditoría de la carga, no en el esquema.
3. **Los conteos físicos de Brilaz, Terreno y Aquamed** (`doc_Cliente/Inventario/*.sql`, que actualizan el legacy) **se aplican al legacy antes de migrar el stock**, para no perderlos.
4. **Esta etapa cubre Bodegas y su stock**, no movimientos. Ingresos, egresos, traslados y regularizaciones (reemplazo de `NotaTraslado` —0 filas—, `ProductoIngresoEgreso` —485— y `RegularizaProducto` —365—) son una etapa aparte, con una tabla `OPT_MovimientoStock`.
5. **Toda sucursal tiene al menos una bodega, y exactamente una principal.** La unicidad de la principal la garantiza un índice filtrado; "al menos una" se siembra para las sucursales existentes y la regla para sucursales nuevas vive en la capa de aplicación (crear la bodega principal al crear la sucursal) — no es constraint de BD.

### Esquema (`013_bodegas.sql`, escrito a mano, idempotente)

- `OPT_Bodega` (`AuditableEntity` + `PublicId`): `SucursalId`, `Codigo` (único por sucursal entre no eliminadas), `Nombre`, `EsPrincipal`, `Activa`.
- `OPT_ProductoBodega`: stock por `(ProductoId, BodegaId)`.
- Backfill del propio script: una bodega principal `BOD-<sucursal>-01` por cada sucursal no eliminada, y si `OPT_ProductoSucursal` aún tuviera stock se mueve a la bodega principal de su sucursal antes de quitar las columnas.
- Aplicado a `dbOPT_NET` (3 bodegas; la sucursal 4 de QA, eliminada, no recibió bodega) y verificado re-ejecutándolo.

### Dominio

`Bodega` y `ProductoBodega` en `OPT.Domain/Entities/Inventario/`, con sus `IEntityTypeConfiguration`. Reglas en `Bodega`: no se desactiva la principal ni una bodega con stock. `ProductoSucursal` queda solo con precio. `IProductoRepositorio.ObtenerStockAsync` ahora recibe `bodegaId` y `ObtenerConStockBajoAsync(sucursalId)` mira las bodegas activas de la sucursal.

*Alternativa descartada:* dejar `PrecioVenta` por bodega. Se descartó porque el precio es una decisión comercial de la sucursal y duplicarlo por bodega obligaría a mantener precios iguales entre bodegas de una misma sucursal.

## Consecuencias / pendiente

- **API hecha (misma fecha):** `Features/Bodegas/` (Crear, Actualizar, MarcarPrincipal, Activar, Desactivar, Eliminar + queries paginada y por `PublicId`), `BodegasController` (`/api/bodegas`; mutaciones para Administrador/Supervisor/JefeSucursal) y `AutorizacionSucursal.ValidarAcceso` sobre `bodega.SucursalId` en todos los handlers; el listado sin `sucursalId` se restringe a la sucursal activa salvo alcance nacional. `CrearSucursalCommandHandler` crea la bodega principal junto con la sucursal. Para eso `IUnitOfWork` ganó `EjecutarEnTransaccionAsync` (varios `CommitAsync` atómicos): hace falta porque el índice único de "una sola principal" obliga a quitar la marca a la anterior antes de poner la nueva, y porque la bodega necesita el Id generado de la sucursal.
- **Frontend hecho:** `features/bodegas/` (listado, diálogo, selector de sucursal), ítem "Bodegas" en el grupo Inventario y enlace "Bodegas" en cada fila de Sucursales. Verificado con lint/build/test; no en navegador.
- Eliminar una sucursal todavía no revisa ni elimina sus bodegas.
- La API compila y el modelo de EF carga contra `dbOPT_NET`, pero ningún endpoint de Bodegas se probó con una sesión autenticada (sin credenciales utilizables en la base).
- Migración del stock legacy (`ProductoSucursal`, 13.776 filas: sucursal 1 = 5.007, 2 = 4.884, 3 = 4.884) directo al modelo de bodegas: script `M013` aún no escrito, tras aplicar los conteos de la decisión 3.
- Falta decidir a qué sucursal pertenecen las bodegas adicionales (Brilaz, Terreno).
- Cuando OT descuente stock habrá que decidir de qué bodega (hoy no descuenta).
- Pendiente de verificación: `dotnet ef`/`dotnet test`; el build se hizo a un directorio aparte porque la API estaba en ejecución.
