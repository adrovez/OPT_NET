# 0005 — `OPT.Migracion` como herramienta standalone fuera de Clean Architecture

**Estado:** Aceptada
**Fecha:** 2026-08-21 (diseño) / 2026-08-24 (implementación y primera ejecución real)

## Contexto

La migración de datos desde el legacy (`db_a25cfd_opt2`) hacia el esquema nuevo (`dbOPT_NET`) necesita escribir en las mismas tablas que usa `OPT.API` en producción. La opción por defecto sería reutilizar el código de dominio ya existente (`OPT.Domain`/`OPT.Application`/`OPT.Infrastructure`, el mismo `AppDbContext`) para no duplicar reglas de negocio ni mapeo EF Core.

Al diseñar el enfoque (sesión 2026-08-21) se identificaron dos bloqueos concretos que hacen inviable ese camino directo:

1. **`AuditableEntity.SetCreacion(int usuarioId)` fuerza `CreadoEn = DateTimeOffset.UtcNow`** — no expone forma de inyectar una fecha histórica. El legacy sí tiene fechas reales (`OPT_Sucursal.FechaRegistro`, `OPT_Usuario.FechaIngreso`) que se quieren preservar, no reemplazar por la fecha de la corrida de migración.
2. **Varios métodos de fábrica de dominio son `internal`** (p. ej. en `Abono`, `BitacoraOT`) y sus invariantes de negocio están pensadas para altas una por una desde la API, no para cargar datos históricos masivos donde algunas invariantes ya vienen "rotas" por años de uso real del legacy (ver `[[migracion-datos-legacy]]` para los casos concretos encontrados: RUTs con formato inconsistente, dos sucursales marcadas como matriz, roles cuyo id no coincide entre legacy y nuevo esquema).
3. Adicionalmente, `OrdenDeTrabajo.NumeroOT` debe preservar el número visible que el legacy ya le mostró al cliente en tickets físicos — no es asignable explícitamente vía las fábricas de dominio actuales (usan `NEXT VALUE FOR SEQ_NumeroOT`).

## Alternativas consideradas

- **Reutilizar `OPT.Infrastructure`/`AppDbContext` desde un proyecto de consola nuevo**: descartada — requeriría modificar las fábricas de dominio y `AuditableEntity` para aceptar fechas/ids arbitrarios "solo para migración", contaminando el modelo de dominio (pensado para altas transaccionales normales) con casos de uso de carga masiva. También obligaría a exponer como `public` varios miembros hoy `internal` por buenas razones.
- **Escribir la migración como scripts T-SQL puros (`INSERT ... SELECT` entre bases)**: descartada — pierde la posibilidad de aplicar lógica de transformación en C# (hash de contraseñas con BCrypt, normalización de RUT/email, mapeo de catálogos por nombre, reportes de datos sospechosos) de forma legible y testeable.
- **Proyecto de consola standalone con ADO.NET directo (`Microsoft.Data.SqlClient` + `Dapper`), sin referenciar `OPT.Domain`/`OPT.Application`/`OPT.Infrastructure`**: **seleccionada**. Reutiliza únicamente `BCrypt.Net-Next` (mismo work factor 12 que `PasswordService`) para no reinventar el hashing, pero escribe con SQL crudo — control total sobre fechas, ids y orden de inserción, sin tocar el dominio.

## Decisión

`OPT.Migracion` (`src/backend/OPT.Migracion/`) es un proyecto de consola **independiente** dentro de `OPT.sln`:

- Depende únicamente de `Microsoft.Data.SqlClient`, `Dapper` y `BCrypt.Net-Next` — **cero** `ProjectReference` a `OPT.Domain`/`OPT.Application`/`OPT.Infrastructure`.
- Lee el legacy siempre de solo lectura (nunca escribe en `db_a25cfd_opt2`).
- Escribe en `dbOPT_NET` con SQL directo, dentro de una única transacción por corrida (rollback completo ante cualquier error a mitad de camino).
- Corre en modo **dry-run por defecto** (reporta el plan sin escribir); requiere `--execute` explícito para aplicar cambios, y `--force` explícito para re-ejecutar sobre tablas de destino que ya tengan filas.
- Cada fase de entidades (Región/Comuna/Rol → Sucursal/Empresa/Usuario → Producto → Cliente/Anamnesis/RecetaCristales → OrdenDeTrabajo/...) se agrega incrementalmente al mismo proyecto, no como proyectos separados.

Ver `[[migracion-datos-legacy]]` para el detalle operativo (qué se migró, decisiones de mapeo específicas, gotchas de datos encontrados) — este ADR documenta el *porqué arquitectónico*, ese archivo documenta el *cómo y con qué reglas*.

## Consecuencias

- **Duplicación deliberada**: las reglas de normalización que sí importan replicar (RUT `Trim().ToUpperInvariant()`, email `Trim().ToLowerInvariant()`) están copiadas manualmente en `OPT.Migracion/Normalizacion.cs` en vez de reutilizar `Usuario.Crear()`. Si esa normalización cambia en el dominio, hay que recordar actualizarla también aquí — riesgo aceptado por ser un cambio de baja frecuencia y una herramienta de vida corta.
- **`OPT.Migracion` es temporal**: no es parte del sistema en producción, no lo referencia `OPT.API` ni ningún otro proyecto, y no hay flujo de negocio que dependa de él en runtime. Es candidato a eliminarse de `OPT.sln` una vez que (a) todas las fases de migración estén completas y validadas, y (b) el legacy se dé de baja definitivamente. Hasta entonces, conviene conservarlo en el repositorio (no solo en el historial de git) como referencia auditable de cómo se transformó cada dato.
- **Sin invariantes de dominio automáticas**: al no pasar por `OPT.Domain`, ninguna regla de negocio (unicidad de RUT, formato, etc.) se valida en C# antes del `INSERT` — las únicas garantías son las constraints de la base de datos (`UNIQUE`, `NOT NULL`, índices filtrados). Esto es aceptable porque los datos ya fueron perfilados con `sqlcmd` antes de escribir cada migrador, pero significa que agregar una fase nueva **siempre** requiere perfilar los datos reales primero, no asumir que "va a andar igual que la fase anterior".
- **No hay FK real sobre `CreadoPor`/`ModificadoPor`/`EliminadoPor`** en ninguna `IEntityTypeConfiguration<T>` (son `int` simples, verificado explícitamente antes de decidir el patrón de usuario "bootstrap" autoreferenciado) — esto es lo que permite que `OPT.Migracion` inserte auditoría histórica sin depender de que el `AuditInterceptor` la genere.
