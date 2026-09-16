# `src/basedatos/` — scripts de base de datos

Todo lo que se aplica a una base de datos de OPT vive aquí. **No se usa `dotnet ef database update` contra ningún ambiente** (desarrollo incluido): el modelo de EF Core es la fuente de verdad del *diseño*, pero el artefacto que se ejecuta es siempre el `.sql` versionado.

## Estructura

```
src/basedatos/
├── 00N_*.sql          # ESQUEMA — numerados, secuenciales, idempotentes
└── migracion/
    └── M00N_*.sql     # DATOS — carga desde el legacy db_a25cfd_opt2
```

Los dos grupos son distintos y no se mezclan:

| | `00N_*.sql` | `migracion/M00N_*.sql` |
|---|---|---|
| Qué hace | Crea/altera tablas, índices, FKs y siembra catálogos | Copia datos reales del legacy a `dbOPT_NET` |
| Se aplica en | Todos los ambientes (dev, QA, producción) | Solo donde se necesite cargar el histórico |
| Re-ejecutable | Sí, siempre (idempotente) | No: aborta si el destino ya tiene datos, salvo `@Force = 1` |
| Numeración | `00N` correlativo | `M00N`, alineado al script de esquema que lo habilita |

## Estado actual

| Script | Contenido |
|---|---|
| `001_esquema_inicial.sql` | 17 tablas, `SEQ_NumeroOT`, seeds de Región (16), Comuna (346) y Rol (3) |
| `002_catalogos.sql` | +3 catálogos (`EstadoOT`, `FormaPago`, `CategoriaProducto`), +5 roles, +3 FKs |
| `003_extras_cliente_receta.sql` | +5 columnas en `Cliente`/`RecetaCristales` (sin tablas nuevas) |
| `004_comercial_pagos_cuotas.sql` | +3 tablas (`EstadoCuota`, `Pago`, `Cuota`), +4 columnas en `OrdenDeTrabajo`, +1 forma de pago (CHEQUE) |
| `005_ot_publicid_estado_anulado.sql` | `+PublicId` en `OrdenDeTrabajo` (12.578 filas pobladas) + estado `ANULADO` (id 7) en `EstadoOT` |
| `006_receta_ot_detalle_comentario.sql` | `+OrdenDeTrabajoId` en `RecetaCristales` (FK + índice) y `+Comentario` en `DetalleOT` — habilitan las pestañas Receta y Detalle de la vista "Ver Orden" |
| `007_receta_incluir_observaciones_detalle.sql` | `+IncluirLejos`/`+IncluirCerca` (bit) y 6 columnas `+Observacion{Od,Oi,Dp}{Lejos,Cerca}` (nvarchar(50)) en `RecetaCristales` — habilitan los checks "Incluir Cristales Lejos/Cerca" y sus observaciones por ojo/DP del formulario de Receta |
| `008_numero_ot_manual.sql` | Quita el `DEFAULT (NEXT VALUE FOR SEQ_NumeroOT)` de `OrdenDeTrabajo.NumeroOT` (pasa a ingresarse manualmente) y reemplaza `UQ_OrdenesDeTrabajo_NumeroOT` (único global) por `UQ_OrdenesDeTrabajo_NumeroOT_Vigente` (único filtrado, excluye `EstadoOTId = 7` ANULADO) |
| `009_modulo_operativo.sql` | Módulo Operativo (Operativos Oftalmológicos en terreno): +4 tablas (`EstadoOperativo`, `Operativo`, `OperativoOT` —con `MontoVendidoSnapshot`/`MontoPagadoSnapshot`, agregadas al construir el dominio— y `GastoOperativo`) + `SEQ_CorrelativoOperativo`. Esquema y `OPT.Domain`/`IEntityTypeConfiguration<T>`/Application/API completos, ambos en la sesión 2026-09-15 — **no aplicado aún a ninguna base real** |
| `migracion/M004_datos_comercial.sql` | Datos del módulo Comercial + los 4.018 productos que necesita el detalle de OT |
| `migracion/M006_backfill_receta_ot_detalle_comentario.sql` | Backfill de las 2 columnas de `006`: 12.574 recetas vinculadas a su OT y 11.168 comentarios de línea |

Total aplicado en `dbOPT_NET`: **23 tablas** (`006`, `007` y `008` no agregan tablas). Tras `009` (aún no aplicado a una base real): **27 tablas**. Referencia columna por columna: `src/documentos/Diccionario_Datos_OPT.docx`.

## Qué NO hacer

- **No correr `dotnet ef migrations add` sin verificar que el modelo de EF Core esté alineado con la base.** Lo estuvo desalineado entre `004` y la sesión del 2026-08-27; **hoy ya no lo está** (`Pago`, `Cuota`, `EstadoCuota`, las 4 propiedades de `OrdenDeTrabajo`, `PublicId`, el estado `ANULADO` y —desde `006`— `RecetaCristales.OrdenDeTrabajoId` y `DetalleOT.Comentario` están todos en el modelo, con su `IEntityTypeConfiguration`). Si vuelve a escribirse un script de esquema a mano, la regla se reactiva: un `migrations add` con el modelo desalineado genera un script que **recrea todo el esquema** e ignora las tablas que el modelo no conoce.
- **No versionar la carpeta `Migrations/` de EF Core.** Se genera, se exporta el `.sql` y se borra (`dotnet ef migrations remove`). Es la razón del gotcha del punto anterior.
- **No re-ejecutar un `M00N_*.sql` "para ver qué pasa".** No borra nada antes de insertar: con `@Force = 1` duplica filas. Si hay que rehacer una carga, vaciar primero las tablas destino a mano.
- **No insertar valores explícitos en una columna autogenerada sin reposicionar su generador al final.** `M004` cargó `OPT_OrdenDeTrabajo.NumeroOT` con el número del legacy pero dejó la `SEQUENCE SEQ_NumeroOT` en 1 → la primera OT creada desde la app choca con `UQ_OrdenesDeTrabajo_NumeroOT` (`duplicate key`). Todo migrador que inserte `NumeroOT` explícito, o ids con `SET IDENTITY_INSERT`, debe cerrar con `ALTER SEQUENCE ... RESTART WITH <MAX+1>` (o `DBCC CHECKIDENT(tabla, RESEED, <MAX>)`) en la misma transacción. Detalle en `.agents/context/migracion-datos-legacy.md` (gotcha `SEQ_NumeroOT`, 2026-09-06). **Desde `008` (2026-09-11) este gotcha ya no aplica a `NumeroOT`**: la columna dejó de tener `DEFAULT`/`SEQUENCE` — se ingresa manualmente y la unicidad la valida la app (por año, entre OT no anuladas) más un índice único filtrado como respaldo (`UQ_OrdenesDeTrabajo_NumeroOT_Vigente`, excluye `EstadoOTId = 7`).
- **No comparar strings entre `db_a25cfd_opt2` y `dbOPT_NET` sin `COLLATE DATABASE_DEFAULT`.** Tienen collation distinta (`Modern_Spanish_CI_AS` vs. `SQL_Latin1_General_CP1_CI_AS`) y el `JOIN` falla con *"Cannot resolve the collation conflict"*.
- **No copiar un id de catálogo del legacy sin verificar.** `EstadoOT` y `FormaPago` coinciden por casualidad; `Region`, `Comuna`, `Rol` y `Empresa` **no**. Ver `.agents/context/migracion-datos-legacy.md`.
- **No apuntar a una base que no sea de desarrollo** sin confirmarlo explícitamente con el usuario.

## Qué SÍ hacer

- Perfilar los datos reales con `sqlcmd` **antes** de escribir cualquier migrador: nulos donde el destino exige `NOT NULL`, duplicados donde exige `UNIQUE`, valores centinela.
- Envolver toda carga de datos en una única transacción, con guardas al inicio (base correcta, prerequisitos aplicados, destino vacío) y un bloque de verificación de conteos al final.
- Registrar cada script nuevo en tres lugares: la tabla de arriba, `CLAUDE.md` (§ Estado actual) y `Diccionario_Datos_OPT.docx` (§6 Historial de scripts).

## Cómo aplicar

```bash
sqlcmd -S localhost -d dbOPT_NET -E -b -i 004_comercial_pagos_cuotas.sql
```

El procedimiento para **regenerar** un script de esquema desde el modelo de EF Core está en `CLAUDE.md`, sección "Base de datos" — incluye el gotcha de los scripts incrementales.
