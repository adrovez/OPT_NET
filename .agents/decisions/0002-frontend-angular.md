# 0002 — Frontend: Angular sobre Blazor

**Estado:** Aceptada
**Fecha:** 2026-08-19 (confirmada 2026-08-21)

## Contexto

El frontend legacy es server-rendered (Razor + jQuery/Bootstrap + DataTables + SweetAlert + Chosen), sin separación entre cliente y servidor. La propuesta inicial del proyecto dejó abierta la elección entre Angular y Blazor para el nuevo frontend SPA, ambos consumiendo el backend API definido en `0001`.

## Alternativas consideradas

Comparación completa en `src/documentos/OPT_Propuesta_Arquitectura.docx`, sección 6.2. Resumen de los criterios que más pesaron:

- **Angular**: ecosistema muy maduro para UI empresarial con grillas, filtros y formularios complejos (el patrón dominante en el legacy: listados + modales de crear/editar + reportes); mayor disponibilidad de desarrolladores en el mercado regional; lenguaje distinto al backend (TypeScript), sin reutilización directa de DTOs/validaciones con C#.
- **Blazor (WebAssembly o Server)**: mismo lenguaje que el backend (C#), permite compartir modelos y validaciones; ecosistema de componentes de UI menos maduro; menor oferta de desarrolladores especializados en el mercado regional; Blazor Server requiere conexión SignalR persistente, Blazor WebAssembly tiene una descarga inicial más pesada.

## Decisión

Se recomienda **Angular** como frontend, principalmente por la madurez de su ecosistema de componentes para interfaces con alta densidad de datos (el escenario dominante en OPT) y por la mayor disponibilidad de desarrolladores si el equipo necesita crecer.

Confirmada por el equipo el 2026-08-21, al arrancar el scaffold de `src/frontend/`.

## Consecuencias

- El equipo necesita competencia en TypeScript/RxJS además de C#, o incorporarla.
- Los contratos entre backend y frontend se mantienen sincronizados vía la documentación OpenAPI generada por la API (no hay tipos compartidos de forma nativa como sí ocurriría con Blazor).
- Se puede aprovechar la skill `angular-developer` disponible en el asistente de código para generar componentes, servicios y convenciones de Angular consistentes con las buenas prácticas actuales del framework (signals, standalone components, lazy loading).
- Scaffold generado en `src/frontend/` (Angular 21, standalone, zoneless, Angular Material, ESLint + Prettier) — ver `src/frontend/README.md` para la estructura y las convenciones de código.
