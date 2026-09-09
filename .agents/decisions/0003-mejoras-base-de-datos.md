# 0003 — Mejoras de diseño de base de datos

**Estado:** Propuesta
**Fecha:** 2026-08-19

## Contexto

El análisis de `old/BD/Create_Table.sql` y `Create_Procedure.sql` (base `db_a25cfd_opt2`, 27 tablas, 18 procedimientos) identificó riesgos de diseño que no deben repetirse en el esquema nuevo. Detalle completo en `src/documentos/OPT_Propuesta_Arquitectura.docx`, secciones 4.2 y 7.

## Alternativas consideradas

- **Reutilizar el esquema legacy tal cual, sólo migrando el motor de acceso a datos**: descartado — perpetuaría los mismos riesgos (credenciales en texto plano, sin auditoría, PKs sobre datos de negocio) en la nueva aplicación.
- **Reescribir el esquema completo desde cero con las mejoras identificadas, migrando los datos en un proceso explícito**: seleccionada.

## Decisión

El esquema nuevo se construye desde cero (no es una copia de `db_a25cfd_opt2`) aplicando las siguientes reglas, no negociables salvo ADR posterior que las reemplace explícitamente:

1. **Identidad de registros**: ninguna tabla de negocio usa un dato de identificación externo (RUT, número de documento) como clave primaria. Se usa un identificador sintético; el dato externo se conserva como atributo con restricción de unicidad.
2. **Generación de IDs**: todo identificador secuencial se genera en la base de datos (identidad o secuencia), nunca calculado en la capa de aplicación.
3. **Seguridad de credenciales**: las contraseñas se almacenan únicamente como hash con función diseñada para contraseñas. Las credenciales de integraciones externas no se almacenan en la base de datos ni en archivos de configuración versionados.
4. **Auditoría**: toda tabla transaccional incluye campos de creación/modificación (quién, cuándo).
5. **Borrado lógico**: el borrado físico se reserva para datos verdaderamente transitorios; el resto usa una marca de inactivo/eliminado.
6. **Normalización de catálogos**: los campos que representan estado o categoría se modelan como referencia a un catálogo, nunca como texto libre.
7. **Campos calculados**: si un valor se persiste por rendimiento (p. ej. un saldo), se recalcula dentro de la misma transacción que el movimiento que lo origina.
8. **Índices**: se definen explícitamente índices secundarios sobre las columnas más consultadas en filtros y joins, ya que SQL Server no los crea automáticamente sobre claves foráneas.
9. **Restricciones nombradas**: toda restricción (PK, FK, UNIQUE, CHECK) tiene nombre explícito — nunca se dejan nombres autogenerados por el motor.
10. **Scripts versionados**: todo cambio de esquema es un script incremental, numerado secuencialmente e idempotente, nunca un cambio manual directo en el motor.
11. **Lógica de negocio fuera de la base**: la lógica actualmente en procedimientos almacenados de negocio (`SP_OTActualizaStock`, `SP_OTEliminar`) se migra a la capa de Aplicación del backend; los procedimientos almacenados quedan reservados para reportes de solo lectura cuando el rendimiento lo justifique.

Lo que queda explícitamente **fuera** del alcance de este ADR (pendiente de otra decisión): si el esquema debe diseñarse desde el inicio para aislamiento multi-tenant (ver `AGENTS.md`, "Decisiones pendientes"). Si esa decisión se toma afirmativamente, este ADR debe actualizarse o ser reemplazado antes de fijar el esquema definitivo, porque cambia el diseño de cada tabla de negocio.

## Consecuencias

- Migrar los datos existentes de `db_a25cfd_opt2` requiere un proceso de transformación explícito (mapeo de RUT a IDs sintéticos, backfill de campos de auditoría con valores por defecto razonables, recomputo de saldos), no una copia directa de tablas.
- El esquema nuevo es más verboso (más columnas de auditoría, más tablas de catálogo referenciadas) que el legacy, a cambio de integridad y trazabilidad.
- Los reportes actualmente resueltos con procedimientos almacenados (`rpt_*`) deben revisarse uno por uno: cuáles se mantienen como procedimientos de solo lectura y cuáles se resuelven desde la capa de Aplicación.
