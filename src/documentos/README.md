# Documentos — OPT

Esta carpeta es la única ubicación de documentación entregable del proyecto (no existe `docs/` en la raíz del repo — todo documento o manual vive aquí). Contenido destinado a lectura humana (stakeholders, equipo de desarrollo, DBA). Las decisiones técnicas formales, con su justificación y alternativas descartadas, viven como ADRs en [`.agents/decisions/`](../../.agents/decisions/) — estos documentos referencian esas decisiones sin repetirlas.

## Contenido

| Documento | Contenido |
|---|---|
| [`OPT_Propuesta_Arquitectura.docx`](OPT_Propuesta_Arquitectura.docx) | Documento entregable: análisis del sistema legado, análisis de la base de datos, arquitectura propuesta, comparación Angular vs. Blazor, propuesta de mejoras de base de datos, plan de migración por fases y preguntas abiertas. |
| [`Manual_Tecnico_Backend_OPT.docx`](Manual_Tecnico_Backend_OPT.docx) | Manual técnico de Base de Datos y Backend — esquema, arquitectura de capas, patrones de código, convenciones, para desarrolladores y DBA. Incluye la API del módulo Comercial y el flujo de estados de la OT (§3.2.3, §4.3, §6.2, §7.5-7.7), el origen Sucursal/Operativo de una OT derivado en su DTO (§4.3/§6.2 — HU-OT-02/03, sesión 2026-09-22) y el módulo Operativo completo (§14, incluido el submenú Recepción y el Reporte de Cristales — sesión 2026-09-22), ver [`.agents/context/modulo-operativo.md`](../../.agents/context/modulo-operativo.md). |
| [`Manual_Tecnico_Frontend_OPT.docx`](Manual_Tecnico_Frontend_OPT.docx) | Manual técnico del Frontend — arquitectura Angular, estructura de carpetas, módulos, seguridad, convenciones. §5 cubre el módulo Clínico y §6 el módulo Comercial completo (ficha "Ver Orden" con bloqueo de edición, listado diferido, asistente de alta por pasos con cabecera/receta reciente/tres modalidades de pago, ticket imprimible, §6.9 el módulo Operativo con el submenú Recepción y el Reporte de Cristales — sesión 2026-09-22 —, y §6.10 la columna "Origen"/filtro "Solo Sucursal" del listado de OT y el enlace al Operativo desde su ficha — HU-OT-02/03, misma sesión). |
| [`Diccionario_Datos_OPT.docx`](Diccionario_Datos_OPT.docx) | Diccionario de datos dedicado — las 27 tablas aplicadas en `dbOPT_NET` (incluidas las 4 del módulo Operativo, script `009`, y sus columnas agregadas por `010`/`011`/`012`), columna por columna, con índices, FKs y valores de catálogo sembrados. Fuente de verdad del esquema aplicado. |
| [`Manual_Tecnico_UX_OPT.docx`](Manual_Tecnico_UX_OPT.docx) | Propuesta de branding e identidad visual (v1.0, no validada aún con stakeholders): logotipo, paleta, tipografía, mockups de pantalla, accesibilidad WCAG, hoja de ruta de implementación. |
| [`Manual_Tecnico_Seguridad_OPT.docx`](Manual_Tecnico_Seguridad_OPT.docx) | Checklist de 18 controles de seguridad de API con estado real verificado en código (implementado/parcial/faltante), detalle con evidencia archivo:línea, y el plan de implementación (ahora / antes de producción / backlog). Resumen operativo para IA en [`.agents/context/seguridad-apis.md`](../../.agents/context/seguridad-apis.md). |

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
- Convención de nombre para manuales técnicos: `Manual_Tecnico_<Area>_OPT.docx` (Backend, Frontend, UX, Seguridad). El diccionario de datos y la propuesta de arquitectura son excepciones deliberadas a ese patrón — son un tipo de documento distinto, no un manual por área.
- Al regenerar cualquiera de estos `.docx` (contenido desactualizado, nueva sección), mantener el mismo nombre de archivo para no romper las referencias desde `CLAUDE.md`, `AGENTS.md`, `src/frontend/CLAUDE.md`/`README.md` y `.agents/context/branding-ux-ui.md`.
