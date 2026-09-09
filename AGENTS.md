# OPT — Project Agent Instructions

> **Estado:** Fases 0 a 2 en marcha. **Base de datos:** 23 tablas aplicadas en `dbOPT_NET` (scripts `001`-`006` en `src/basedatos/`; `003`, `005` y `006` solo agregan columnas). **Migración de datos:** Organización y Clínico con `OPT.Migracion` (ADR `0005`); Comercial con SQL directo (`M004`, ADR `0006`) — 12.578 OT, 20.573 líneas de detalle, 31.964 bitácoras, 3.277 abonos, 3.736 pagos, 34.110 cuotas y 4.018 productos; backfill `M006` (2026-08-28) con 12.574 vínculos receta↔OT y 11.168 comentarios de línea. Pendiente: `ProductoSucursal` (13.776) y los 659 productos que ningún detalle referencia. **Backend:** CRUD/API completo para Organización (Sucursal, Empresa, Usuario, Rol), catálogos de solo lectura (Región, Comuna, EstadoOT, FormaPago, EstadoCuota), Clínico (Cliente, Anamnesis, RecetaCristales) y Comercial (agregado OT con abonos, pagos, cuotas, flujo de estados y anulación — ADR `0007`), más `/api/productos` y `/api/cobranza/deudores`. El resto de Inventario sigue stub. **Frontend:** Angular con tema de marca aplicado y pantallas reales contra el backend para Organización, Clínico y Comercial; la ficha de la OT replica la vista "Ver Orden" del legacy, su listado no consulta hasta que hay un criterio (ADR `0008`), el alta es un asistente por pasos que permite crear cliente y receta sin salir de la orden y cierra con ticket imprimible (ADR `0009`), y la propia ficha/alta recibió una segunda pasada de mejoras (0010): bloqueo real de edición cuando la OT está `Entregado`/anulada, cabecera completa en el paso Cliente, combo de receta reciente + historial colapsable, y rediseño del paso Pago con tres modalidades explícitas y previsualización de cuotas. **Verificación pendiente:** no hay credenciales usables en `dbOPT_NET` para probar la API con sesión autenticada (los usuarios migrados conservan la clave de 4 caracteres del legacy y el validador de login exige 6).
> **Última actualización:** 2026-09-08

## Sobre este documento

Este archivo reemplaza la versión anterior de `AGENTS.md`, que describía una implementación (.NET 10 + Angular 21, multi-tenant) construida antes de que se realizara el análisis formal del sistema legado y la propuesta de arquitectura (ver `src/documentos/OPT_Propuesta_Arquitectura.docx`). Esa implementación previa sigue físicamente en `src/`, pero **ya no es la referencia de arquitectura vigente**: las decisiones de este documento parten de cero, tal como fue solicitado. Reconciliar o archivar el código existente en `src/` es una decisión pendiente del equipo, no asumida aquí — ver `.agents/decisions/0001-arquitectura-backend-clean-architecture.md`, sección "Qué pasa con `src/` actual".

## Sobre este proyecto

OPT es una aplicación legacy en migración hacia una arquitectura moderna. El proyecto contiene código legacy (`old/`) y código nuevo (`src/`).

**Contexto de negocio:** sistema de gestión de ópticas — clientes, ficha clínica (anamnesis, receta de cristales), órdenes de trabajo, pagos, inventario, sucursales y usuarios. El modelo de negocio (mono-óptica vs. múltiples ópticas como tenants aislados) es una pregunta abierta — ver sección "Decisiones pendientes".

---

## Estructura de directorios

```
OPT/
├── src/
│   ├── backend/                      # Scaffold Fase 0 (.NET 8 / Clean Architecture) ← REFERENCIA VIGENTE
│   │   ├── OPT.sln
│   │   ├── OPT.Domain/
│   │   ├── OPT.Application/
│   │   ├── OPT.Infrastructure/
│   │   ├── OPT.API/
│   │   └── OPT.Migracion/                # Consola standalone de migración de datos legacy — fuera de la arquitectura en capas (ADR 0005), no es parte del sistema en producción
│   ├── frontend/                     # Scaffold generado (Angular 21, standalone, zoneless — ADR 0002)
│   ├── basedatos/                    # Scripts SQL Server versionados: esquema 001-006 (23 tablas aplicadas en dbOPT_NET) + migracion/M00N_*.sql (carga de datos legacy) — ver src/basedatos/README.md
│   └── documentos/                   # TODOS los documentos/manuales del proyecto (única carpeta — no existe docs/ en la raíz)
│       ├── OPT_Propuesta_Arquitectura.docx
│       ├── Manual_Tecnico_Backend_OPT.docx
│       ├── Manual_Tecnico_Frontend_OPT.docx
│       ├── Diccionario_Datos_OPT.docx
│       ├── Manual_Tecnico_UX_OPT.docx
│       └── README.md                 # Índice de todos los documentos y su relación con los ADRs
│
├── old/                              # CÓDIGO LEGACY — solo referencia (SOLO LECTURA, nunca modificar)
│   ├── Fuente/                       # Código fuente legacy (.NET Framework 4.8 / MVC 5)
│   └── BD/                          # Scripts de la base de datos legada (db_a25cfd_opt2)
│
├── .agents/
│   ├── progress.md                   # Log de sesiones y estado actual del proyecto
│   ├── decisions/                    # ADRs — el "por qué" de cada decisión
│   ├── context/                      # Conocimiento de dominio extraído del legacy
│   └── skills/                      # Skills reutilizables para generación de código
│
└── AGENTS.md / CLAUDE.md             # Este archivo y su equivalente técnico para Claude Code
```

---

## Stack tecnológico (ver ADRs para detalle y alternativas evaluadas)

### Backend — **implementado en Fase 0**

| Componente | Tecnología | ADR |
|-----------|-----------|-----|
| Framework | .NET 8 (LTS), Clean Architecture (Domain / Application / Infrastructure / API) | `0001` ✅ Aceptada |
| Mediador CQRS | MediatR 12 + FluentValidation 11 | `0001` |
| ORM | Entity Framework Core 8, Code First — esquema versionado como script SQL (no Migrations aplicadas en runtime) | `0001` |
| Auth | JWT Bearer + BCrypt (work factor 12) para hashing de contraseñas | `0001` |
| API | REST, documentada con OpenAPI/Swagger (soporte JWT en UI) | `0001` |
| Errores HTTP | ProblemDetails (RFC 7807), centralizado en `ExceptionHandlingMiddleware` | `0001` |

### Frontend — **scaffold generado (Fase 0)**

| Componente | Tecnología | ADR |
|-----------|-----------|-----|
| Framework | Angular 21, standalone, zoneless (signals) | `0002` ✅ Aceptada |
| UI | Angular Material 21 (tema custom) | `0002` |
| Consumo de API | `HttpClient` + interceptores funcionales (JWT, manejo de errores/401), nunca acceso directo desde componentes | `0002` |
| Estado | Signals + servicios `providedIn: 'root'` (sin store externo) | `0002` |

Detalle de estructura y convenciones: `src/frontend/README.md`.

### Base de datos

| Componente | Tecnología | ADR |
|-----------|-----------|-----|
| Motor | SQL Server | `0003` |
| Esquema | Nuevo (no es copia del legacy); modelado en EF Core, versionado como script SQL en `src/basedatos/` | `0003` |
| Migración de datos | Proceso explícito y auditable desde `db_a25cfd_opt2` — Fase 6. Proyecto de consola `OPT.Migracion` construido, ADO.NET directo, standalone (`0005`). Organización y Clínico (Cliente/Anamnesis/RecetaCristales) ya migrados; Producto/OrdenDeTrabajo pendientes — ver `.agents/context/migracion-datos-legacy.md` | `0003`, `0005` |

---

## Reglas críticas (no negociables)

1. **NUNCA modificar archivos en `old/`** — es código legacy de referencia, solo lectura.
2. **CONSULTAR `old/Fuente/` y `.agents/context/`** cuando se migra lógica, para entender el comportamiento y las reglas de negocio existentes antes de reescribirlas.
3. **NUNCA contraseñas ni credenciales en texto plano** — ni en base de datos, ni en archivos de configuración versionados. Usar `dotnet user-secrets` en desarrollo y variables de entorno en producción.
4. **NUNCA hacer DELETE físico** en tablas de negocio — usar borrado lógico (campo `Eliminado` en `AuditableEntity`).
5. **NUNCA lógica de negocio en controllers** — los controllers delegan a MediatR, que enruta al handler correspondiente.
6. **NUNCA acceder al `DbContext` directamente desde la capa de Aplicación** — usar interfaces de repositorio implementadas en Infraestructura.
7. **Toda tabla transaccional hereda de `AuditableEntity`** — los campos de auditoría los rellena `AuditInterceptor` automáticamente.
8. **Todo cambio de esquema se modela primero en EF Core y se versiona como script SQL en `src/basedatos/`** (procedimiento en `CLAUDE.md`) — nunca un cambio manual directo en el motor, y nunca `dotnet ef database update` contra un ambiente real.
9. **SIEMPRE actualizar `.agents/progress.md`** al finalizar sesiones significativas de trabajo con un agente IA.
10. **Toda decisión de arquitectura relevante se registra como ADR** en `.agents/decisions/`, incluyendo las alternativas descartadas y por qué.
11. **Nombres de tabla con prefijo `OPT_` + singular** (`OPT_Cliente`, `OPT_OrdenDeTrabajo`) — ADR `0004`.
12. **`Cliente`, `Empresa`, `Usuario`, `Anamnesis`, `RecetaCristales` y `OrdenDeTrabajo` se exponen en API/URLs por `PublicId` (Guid), nunca por el `Id` interno** — medida de seguridad contra enumeración de datos personales/sensibles bajo la Ley N° 21.719 (ADR `0004`). Sus **subrecursos** (`DetalleOT`, `Abono`, `Pago`, `Cuota`, `BitacoraOT`) sí usan el Id interno: solo se alcanzan anidados bajo la OT, que ya está protegida (ADR `0007`).
13. **`OPT.Migracion` nunca escribe en el legacy** (`db_a25cfd_opt2`), solo lee. Nunca reutilizar `OPT.Domain`/`OPT.Application`/`OPT.Infrastructure` desde ahí (ADR `0005`) ni cambiar su default de dry-run — toda ejecución real requiere `--execute` explícito, y perfilar los datos reales con `sqlcmd` antes de escribir un migrador nuevo (ver `.agents/context/migracion-datos-legacy.md`).

---

## Decisiones pendientes (bloquean diseño de detalle)

| Decisión | Impacto si no se resuelve |
|----------|--------------------------|
| ¿Mono-óptica o multi-tenant? | Cambia el diseño de aislamiento de datos en cada tabla del esquema nuevo |
| ¿Convivencia legado/nuevo o corte por módulo? | Define el plan de despliegue de cada fase |
| ¿Qué pasa con la iteración previa en `src/`? | Evitar confusión entre el scaffold nuevo y el código obsoleto |

**Resuelto (2026-08-21):** volumen real de datos a migrar — perfilado en vivo contra `db_a25cfd_opt2`: 11.881 Clientes, 12.578 OrdenDeTrabajo, 13.183 RecetaCristales, 31.964 BitacoraOT, 4.677 Producto, 491 Empresa, 3.277 Abono, 1.261 Anamnesis, 13 Usuario. Plan de migración aprobado — ver `.agents/progress.md`.

**Resuelto (2026-08-24):** herramienta de migración construida y validada con una ejecución real — Sucursal, Empresa, Usuario, UsuarioSucursal y EmpresaSucursal ya están en `dbOPT_NET`.

**Resuelto (2026-08-26):** grupo Clínico migrado — Cliente (11.881), Anamnesis (1.261), RecetaCristales (13.183). Requirió extender el esquema primero (`003_extras_cliente_receta.sql`: `Cliente.FechaNacimiento`/`TipoPrevision`, `RecetaCristales.DpLejos`/`DpCerca`/`AddLejos`) tras perfilar que esos campos legacy sin columna equivalente se usaban en 59-91% de las filas. `OPT_Atencion` del legacy quedó confirmado fuera de alcance — no tiene tabla equivalente en el esquema nuevo. Ver `.agents/context/migracion-datos-legacy.md` para el estado fase por fase y los próximos pasos (Producto → OrdenDeTrabajo/DetalleOT/Abono/BitacoraOT).

**Resuelto (2026-08-26):** Fase 1 de negocio arrancada — CRUD/API completo para Cliente, Anamnesis y RecetaCristales (`OPT.Application`/`OPT.API`), con `PublicId` en rutas/DTOs (ADR `0004`) y `ObtenerClientesQuery` paginado (justificado por el volumen real, ~11.881 filas). Se confirmó con el usuario **no** crear un módulo/endpoint "Atención" — consistente con la decisión ya tomada al migrar los datos. Detalle completo, incluidas las decisiones de diseño (RUT inmutable tras creación, validación de rango de graduación óptica que el legacy no tenía) en `.agents/progress.md`, entrada 2026-08-26 "Fase 1: APIs de Clientes, Anamnesis y RecetaCristales".

**Resuelto (2026-08-27):** Fase 2 — módulo Comercial completo de punta a punta. Base de datos (`004`, `005`) y datos migrados por SQL directo (`M004`, ADR `0006`), API del agregado OT con flujo de estados, anulación por catálogo, precio derivado del detalle e imputación automática de cuotas (ADR `0007`), y frontend con listado, alta/edición, ficha de la OT y las pantallas de Abonos/Pagos/Cuotas/Cobranza. Se resolvió también la ambigüedad Abono vs. Pago con los datos reales: son dos flujos distintos, y el `Saldo` del legacy estaba inflado en 3.472 OT porque nunca descontaba `OPT_Pago`.

**Resuelto (2026-08-28):** vista "Ver Orden" y listado diferido (ADR `0008`). Se recuperó el vínculo receta↔OT que el legacy tenía (`OPT_RecetaCristales.idOT` → `OrdenDeTrabajoId`) y el comentario por línea del detalle, ambos perdidos en el rediseño del esquema; la ficha de la OT adoptó las pestañas del legacy (Cliente / Receta / Detalle / Abonos, más Pagos / Cuotas / Bitácora) y el listado dejó de consultar al entrar. Regla de contexto que quedó registrada: antes de dar por perdida una columna del legacy, verificar si el esquema nuevo la descartó **a propósito** o solo por no tener destino.

**Resuelto (2026-08-28, 2ª sesión):** alta de OT como asistente por pasos (ADR `0009`). Se recuperaron cuatro capacidades del legacy que el formulario nuevo había perdido —crear el cliente dentro del ingreso, tomar la receta ahí mismo, cerrar con confirmación + ticket imprimible, y no dejar un saldo sin plan de cuotas por descuido— reusando los diálogos y las APIs ya existentes, sin tocar backend ni esquema. Regla de contexto que quedó registrada: **el contrato de la API no es el flujo de trabajo**; antes de dar por terminada una pantalla migrada, recorrer el controller del legacy completo y preguntarse qué podía hacer el operador ahí que ahora ya no puede.

**Resuelto (2026-09-08):** seis observaciones puntuales sobre la ficha/alta de OT, propuestas y luego implementadas en la misma sesión (ADR `0010`). La más relevante como regla de contexto: **una regla de UI y su regla de validación en el backend son la misma regla — no se resuelve solo un lado.** La primera pasada solo tocó el frontend (quitó el `Validators.required` que dejaba el botón "Guardar" de Nueva Receta deshabilitado) y habría dejado la API rechazando con 422 igual; se detectó al documentar y se corrigió `CrearRecetaCristalesCommandValidator`/`ActualizarRecetaCristalesCommandValidator` en la misma sesión. Las cuotas siguen siendo mensuales (`AddMonths`), no de 30 días fijos — confirmado explícitamente por el usuario.

---

## Workflow de trabajo con agentes IA

1. Leer `.agents/progress.md` para conocer el estado actual antes de empezar.
2. Leer `.agents/context/` para entender el dominio de negocio y las reglas heredadas del legacy.
3. Si la tarea toca una decisión de arquitectura no cubierta por un ADR existente, proponerla y registrarla en `.agents/decisions/` antes de implementar.
4. Consultar `old/Fuente/` únicamente como referencia de comportamiento — nunca copiar sus patrones de codificación (controllers gordos, catch/throw ex, credenciales en texto plano) al código nuevo.
5. Implementar en `src/backend/` (scaffold Fase 0) siguiendo las reglas críticas de este documento y las convenciones de `CLAUDE.md`.
6. Actualizar `.agents/progress.md` con un resumen de la sesión al terminar.

---

## Skills disponibles

| Skill | Ubicación | Uso |
|-------|-----------|-----|
| `dotnet-best-practices` | `.agents/skills/dotnet-best-practices/` | Buenas prácticas de C#/.NET para el backend. |
| `ui-ux-pro-max` | `.agents/skills/ui-ux-pro-max/` | Guías de diseño de UI/UX, sistema de diseño, branding. |
| `angular-developer` | Asistente (no en repo) | Convenciones Angular — usar cuando arranque el frontend. |
| `angular-new-app` | Asistente (no en repo) | Scaffold de aplicación Angular nueva. |

---

## Documentación de referencia

| Documento | Ruta | Contenido |
|-----------|------|-----------|
| Propuesta de arquitectura | `src/documentos/OPT_Propuesta_Arquitectura.docx` | Análisis del legacy, comparación Angular vs. Blazor, mejoras de BD, plan de migración por fases. |
| Manual técnico — Backend | `src/documentos/Manual_Tecnico_Backend_OPT.docx` | Base de datos y backend: esquema (23 tablas), arquitectura de capas, patrones de código, convenciones, estado de `OPT.Migracion` y de los endpoints implementados — para desarrolladores y DBA. |
| Diccionario de datos | `src/documentos/Diccionario_Datos_OPT.docx` | Referencia exhaustiva de las 23 tablas de `dbOPT_NET`: columnas, tipos, índices, FKs, catálogos sembrados. Fuente de verdad del esquema aplicado. |
| Manual técnico — Frontend | `src/documentos/Manual_Tecnico_Frontend_OPT.docx` | Arquitectura Angular, estructura de carpetas, módulos, seguridad, convenciones — para desarrolladores. |
| Manual técnico — UX | `src/documentos/Manual_Tecnico_UX_OPT.docx` | Propuesta de branding e identidad visual: logotipo, paleta, tipografía, mockups de pantalla, accesibilidad WCAG. |
| Guía técnica para IA — Frontend | `src/frontend/CLAUDE.md` | Convenciones y patrones prescriptivos para generar código Angular con IA. |
| ADRs | `.agents/decisions/` | Decisiones de arquitectura con contexto, alternativas y consecuencias (`0001`-`0010`). |
| Migración de datos legacy | `.agents/context/migracion-datos-legacy.md` | Estado fase por fase, gotchas de datos reales ya encontrados y el patrón "usuario bootstrap" — leer antes de escribir un migrador nuevo. |
| Scripts de base de datos | `src/basedatos/README.md` | Diferencia entre esquema (`00N`) y carga de datos (`migracion/M00N`), estado de cada script y qué NO hacer al tocar la base. |
| Glosario de dominio | `.agents/context/glosario-dominio.md` | Términos de negocio del sistema OPT. |
| Reglas de negocio heredadas | `.agents/context/reglas-negocio-legado.md` | Comportamiento del legacy a preservar, mejorar o reconsiderar. |
| Branding y contexto UX/UI | `.agents/context/branding-ux-ui.md` | Paleta, tipografía, mapeo de color a `OPT_EstadoOT`/`OPT_FormaPago`, accesibilidad WCAG, voz y tono — resumen accionable de `Manual_Tecnico_UX_OPT.docx`, ya aplicado en `src/frontend` (sección "Implementación en código"). |
| Migración de datos legacy | `.agents/context/migracion-datos-legacy.md` | Estado fase por fase de `OPT.Migracion`, gotchas de datos reales y el patrón "usuario bootstrap" para auditoría. |
| Progress log | `.agents/progress.md` | Historial de sesiones y próximos pasos. |
| Guía técnica para IA | `CLAUDE.md` | Comandos de build, árbol del scaffold, convenciones de código. |
