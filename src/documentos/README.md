# Documentos — OPT

Esta carpeta es la única ubicación de documentación entregable del proyecto (no existe `docs/` en la raíz del repo — todo documento o manual vive aquí). Contenido destinado a lectura humana (stakeholders, equipo de desarrollo, DBA). Las decisiones técnicas formales, con su justificación y alternativas descartadas, viven como ADRs en [`.agents/decisions/`](../../.agents/decisions/) — estos documentos referencian esas decisiones sin repetirlas.

## Contenido

| Documento | Contenido |
|---|---|
| [`OPT_Propuesta_Arquitectura.docx`](OPT_Propuesta_Arquitectura.docx) | Documento entregable: análisis del sistema legado, análisis de la base de datos, arquitectura propuesta, comparación Angular vs. Blazor, propuesta de mejoras de base de datos, plan de migración por fases y preguntas abiertas. |
| [`Manual_Tecnico_Backend_OPT.docx`](Manual_Tecnico_Backend_OPT.docx) | Manual técnico de Base de Datos y Backend — esquema, arquitectura de capas, patrones de código, convenciones, para desarrolladores y DBA. Incluye la API del módulo Comercial y el flujo de estados de la OT (§3.2.3, §4.3, §6.2, §7.5-7.7). Al día con el script `007` y el ADR `0010`. |
| [`Manual_Tecnico_Frontend_OPT.docx`](Manual_Tecnico_Frontend_OPT.docx) | Manual técnico del Frontend — arquitectura Angular, estructura de carpetas, módulos, seguridad, convenciones. §5 cubre el módulo Clínico y §6 el módulo Comercial completo (ficha "Ver Orden" con bloqueo de edición, listado diferido, asistente de alta por pasos con cabecera/receta reciente/tres modalidades de pago, y ticket imprimible). |
| [`Diccionario_Datos_OPT.docx`](Diccionario_Datos_OPT.docx) | Diccionario de datos dedicado — las 23 tablas de `dbOPT_NET`, columna por columna, con índices, FKs y valores de catálogo sembrados. Fuente de verdad del esquema aplicado (al día con el script `006`: `OPT_RecetaCristales.OrdenDeTrabajoId` y `OPT_DetalleOT.Comentario`). |
| [`Manual_Tecnico_UX_OPT.docx`](Manual_Tecnico_UX_OPT.docx) | Propuesta de branding e identidad visual (v1.0, no validada aún con stakeholders): logotipo, paleta, tipografía, mockups de pantalla, accesibilidad WCAG, hoja de ruta de implementación. |

## Relación de `OPT_Propuesta_Arquitectura.docx` con `.agents/decisions/`

| Sección de la propuesta | ADR correspondiente |
|--------------------------|----------------------|
| 5. Arquitectura propuesta / 6.1 Backend | [`0001-arquitectura-backend-clean-architecture.md`](../../.agents/decisions/0001-arquitectura-backend-clean-architecture.md) |
| 6.2 Frontend: Angular vs. Blazor | [`0002-frontend-angular.md`](../../.agents/decisions/0002-frontend-angular.md) |
| 4. Análisis de BD / 7. Mejoras de BD | [`0003-mejoras-base-de-datos.md`](../../.agents/decisions/0003-mejoras-base-de-datos.md) |

Los ADRs `0004` a `0010` no provienen de la propuesta: nacieron de decisiones tomadas durante la implementación (cumplimiento Ley 21.719, herramienta de migración, módulo Comercial, API de la OT, vista "Ver Orden", asistente de alta de la OT y la segunda pasada de mejoras sobre esa ficha/alta). Se consultan directamente en [`.agents/decisions/`](../../.agents/decisions/).

Cuando el equipo confirme una decisión pendiente (ver `AGENTS.md` en la raíz del repo), el ADR correspondiente debe actualizar su estado de "Propuesta" a "Aceptada", y si corresponde, agregarse un nuevo ADR numerado que documente la confirmación o el cambio.

## Cómo mantener esta carpeta

- Todo documento o manual nuevo (entregable, no código) va aquí — nunca se recrea una carpeta `docs/` en la raíz.
- Convención de nombre para manuales técnicos: `Manual_Tecnico_<Area>_OPT.docx` (Backend, Frontend, UX). El diccionario de datos y la propuesta de arquitectura son excepciones deliberadas a ese patrón — son un tipo de documento distinto, no un manual por área.
- Al regenerar cualquiera de estos `.docx` (contenido desactualizado, nueva sección), mantener el mismo nombre de archivo para no romper las referencias desde `CLAUDE.md`, `AGENTS.md`, `src/frontend/CLAUDE.md`/`README.md` y `.agents/context/branding-ux-ui.md`.
