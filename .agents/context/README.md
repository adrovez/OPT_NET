# Contexto de dominio — OPT

Esta carpeta es la "memoria de negocio" del proyecto: conocimiento extraído del análisis del sistema legado que un agente IA debe leer antes de diseñar o generar código para un módulo nuevo, para no reinventar ni contradecir reglas de negocio ya validadas por años de uso real.

No es documentación de arquitectura (eso vive en `.agents/decisions/`) ni un manual técnico o de usuario (eso vive en `src/documentos/` — única carpeta de documentos del proyecto). Es específicamente: **qué significa cada término del dominio** y **qué reglas de negocio observa el sistema actual**, para que la generación de código nuevo parta de una base correcta.

## Contenido

- [`glosario-dominio.md`](glosario-dominio.md) — términos de negocio del sistema OPT y qué representan.
- [`reglas-negocio-legado.md`](reglas-negocio-legado.md) — comportamientos observados en `old/Fuente/`, marcados como **preservar**, **mejorar** o **reconsiderar**.
- [`branding-ux-ui.md`](branding-ux-ui.md) — contexto accionable de marca/UX-UI: paleta, tipografía, mapeo de color a los catálogos reales (`OPT_EstadoOT`, `OPT_FormaPago`), reglas de accesibilidad, voz y tono. Resumen en texto de `src/documentos/Manual_Tecnico_UX_OPT.docx`.
- [`migracion-datos-legacy.md`](migracion-datos-legacy.md) — estado fase por fase de la migración de datos (`OPT.Migracion`), gotchas de datos reales ya encontrados (ids de catálogo que no coinciden, RUTs inconsistentes, flags únicos duplicados) y el patrón "usuario bootstrap" para auditoría — leer antes de escribir cualquier migrador nuevo.
- [`seguridad-apis.md`](seguridad-apis.md) — snapshot de qué controles de seguridad de API están implementados vs. faltantes, la decisión de secuencia (qué se resuelve ahora vs. antes de producción) y las reglas de forma de trabajo para un agente IA al tocar autenticación/autorización. Resumen operativo de `src/documentos/Manual_Tecnico_Seguridad_OPT.docx` — leer antes de agregar un controller o endpoint nuevo.
- [`modulo-operativo.md`](modulo-operativo.md) — requerimiento del Módulo Operativo (Operativos Oftalmológicos en terreno): qué problema resuelve, tabla de traducción de convenciones (el documento original se escribió para otro proyecto — multi-tenant, PK `UNIQUEIDENTIFIER`, etc.), decisiones ya cerradas con el usuario, y el estado real: esquema + Domain/Application/API + frontend completos y **aplicados a `dbOPT_NET`** (scripts `009` a `012`), incluidas las Épicas A (contacto del Operativo) y B (submenú Recepción + Reporte de Cristales) del HU `02_HU_Modulo_Operativo.html` (§6a) — quedan pendientes las Épicas C (Cobranza: vínculo laboral/desvinculación/pérdida, con preguntas abiertas sin resolver) y D (Gastos: categoría/fecha), ver §6 "Próximos pasos". Ver también ADR `0011` y `src/frontend/CLAUDE.md` § "Módulo Operativo". Leer antes de continuar cualquier etapa de este módulo — evita repetir preguntas ya resueltas.

## Cómo mantener esta carpeta

A medida que se migra cada módulo (según el plan de fases de la propuesta de arquitectura), se debe:

1. Leer el código legacy correspondiente en `old/Fuente/` antes de escribir el nuevo.
2. Si aparece una regla de negocio no documentada aquí, agregarla a `reglas-negocio-legado.md` con su clasificación (preservar/mejorar/reconsiderar) y una nota de por qué.
3. Si aparece un término de negocio nuevo, agregarlo a `glosario-dominio.md`.

Esta carpeta crece de forma incremental — no se espera que esté completa desde el día uno, sólo que nunca quede desactualizada respecto de lo que ya se migró.
