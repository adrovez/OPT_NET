# Architectural Decision Records (ADRs) — OPT

Este directorio registra las decisiones de arquitectura del proyecto, con su contexto, las alternativas consideradas y las consecuencias aceptadas. Un ADR no se borra cuando queda obsoleto — se marca como "Reemplazado por `000X`" y se conserva, para que el historial de decisiones sea auditable.

## Convención

- Nombre de archivo: `NNNN-titulo-en-kebab-case.md`, numeración secuencial (no reutilizar números).
- Cada ADR sigue esta estructura mínima:

```
# NNNN — Título de la decisión

**Estado:** Propuesta | Aceptada | Reemplazada por 000X
**Fecha:** AAAA-MM-DD

## Contexto
Qué problema o pregunta motiva esta decisión.

## Alternativas consideradas
Qué otras opciones se evaluaron y por qué no se eligieron.

## Decisión
Qué se decidió, en una o dos frases claras.

## Consecuencias
Qué implica esta decisión — tanto beneficios como costos o riesgos aceptados.
```

## Índice

| ADR | Título | Estado |
|-----|--------|--------|
| [0001](0001-arquitectura-backend-clean-architecture.md) | Arquitectura de backend en capas (.NET) | Aceptada |
| [0002](0002-frontend-angular.md) | Frontend: Angular sobre Blazor | Aceptada |
| [0003](0003-mejoras-base-de-datos.md) | Mejoras de diseño de base de datos | Propuesta |
| [0004](0004-cumplimiento-ley-21719.md) | Cumplimiento Ley N° 21.719 y convenciones asociadas de esquema | Aceptada |
| [0005](0005-migracion-datos-herramienta-standalone.md) | `OPT.Migracion` como herramienta standalone fuera de Clean Architecture | Aceptada |
| [0006](0006-modulo-comercial-pago-cuota-y-migracion-sql.md) | Módulo Comercial: tablas `Pago`/`Cuota` y migración por SQL directo | Aceptada |
| [0007](0007-api-comercial-flujo-estados-ot.md) | API del módulo Comercial: agregado OT, flujo de estados y alcance de `PublicId` | Aceptada |
| [0008](0008-receta-vinculada-a-ot-y-vista-ver-orden.md) | Receta vinculada a la Orden de Trabajo y vista "Ver Orden" | Aceptada |
| [0009](0009-alta-ot-asistente-por-pasos.md) | Alta de OT como asistente por pasos: cliente y receta en línea, y ticket imprimible | Aceptada |
| [0010](0010-propuesta-mejoras-ficha-alta-ot.md) | Observaciones de mesón sobre la ficha/alta de OT (cabecera, receta, pago) | Aceptada |

Todos los ADRs listados como "Propuesta" provienen del documento `src/documentos/OPT_Propuesta_Arquitectura.docx` y quedan como "Aceptada" recién cuando el equipo los confirme explícitamente (ver preguntas pendientes en `AGENTS.md`). El ADR `0001` pasó de "Propuesta" a "Aceptada" el 2026-08-19, cuando se generó el scaffold del backend implementando la decisión (ver `.agents/progress.md`, entrada de esa fecha). El ADR `0004` se marca "Aceptada" desde su creación porque sus decisiones técnicas (identificador público no enumerable, convención de nombres) fueron confirmadas directamente por el equipo en la sesión que lo originó — sus puntos de cumplimiento legal pendientes (retención, consentimiento, DPO) quedan registrados como abiertos dentro del mismo documento. El ADR `0005` sigue el mismo criterio: la decisión técnica (herramienta standalone) fue confirmada por el equipo al aprobar el plan de migración (2026-08-21) y validada con la primera ejecución real (2026-08-24). El ADR `0006` también nace "Aceptada": sus cuatro decisiones se tomaron con el usuario en la sesión (vía `AskUserQuestion`) y se ejecutaron y verificaron el mismo día. El ADR `0007` sigue el mismo criterio: sus cinco decisiones (alcance de `PublicId`, flujo de estados, anulación por catálogo, precio derivado con sobrepago permitido, y cuotas con imputación automática) se consultaron con el usuario antes de escribir código, en la sesión del 2026-08-27. El ADR `0008` también nace "Aceptada": sus decisiones de alcance (vincular la receta a la OT en vez de aproximarla por fecha, y rediseñar la ficha ruteada en vez de agregar un modal aparte) se consultaron con el usuario vía `AskUserQuestion` antes de tocar el esquema, en la sesión del 2026-08-28. El ADR `0009` cierra la misma serie: su alcance (qué capacidades del legacy recuperar, layout de stepper y reuso de las APIs existentes en vez de un endpoint transaccional) se consultó con el usuario vía `AskUserQuestion` antes de escribir código, en la 2ª sesión del 2026-08-28. El ADR `0010` nace "Aceptada" en dos sesiones el mismo día: primero contrasta seis observaciones de mesón contra el código actual y deja preguntas abiertas para el negocio; el usuario las resolvió vía `AskUserQuestion` (3 meses desde hoy, observaciones de receta opcionales, cuotas mensuales) y pidió implementar de inmediato — los cinco cambios de frontend quedaron aplicados y compilando en la misma sesión.
