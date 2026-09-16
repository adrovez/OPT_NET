# Seguridad de APIs — contexto para IA

Este archivo es memoria operativa para un agente IA (Claude Code u otro) que va a diseñar o generar código en `src/backend/OPT.API`, `OPT.Application` o `OPT.Infrastructure`. No repite el detalle del [`Manual_Tecnico_Seguridad_OPT.docx`](../../src/documentos/Manual_Tecnico_Seguridad_OPT.docx) (eso vive en `src/documentos/`) — acá va el **estado resumido, la decisión de secuencia (ahora / al final) y cómo mantener este archivo al día**, para que cada sesión nueva no tenga que re-derivar el análisis desde cero.

## Snapshot de estado (2026-09-15, actualizado en la sesión de autorización por rol/sucursal)

Basado en un análisis de código real (no solo de lo que dice `CLAUDE.md`) sobre `src/backend`. Antes de confiar en esta tabla para una sesión nueva, verificar que sigue siendo cierta — es fácil que un endpoint nuevo se agregue sin el atributo de rol correspondiente, o que alguien resuelva un punto de la lista "falta" sin actualizar este archivo.

**Implementado y verificado en código:**
- HTTPS forzado (`UseHttpsRedirection`), JWT HMAC-SHA256 (480 min, clave vía `user-secrets`/config, nunca hardcodeada), BCrypt work factor 12, FluentValidation + `ValidationBehaviour` en el pipeline de MediatR, `ExceptionHandlingMiddleware` sin fuga de stack trace, CORS con orígenes acotados sin `AllowCredentials`, EF Core 100% parametrizado (sin `FromSqlRaw`/`ExecuteSqlRaw`), Commands de MediatR como DTOs propios (sin mass assignment sobre entidades), `AuditInterceptor` rellena `CreadoEn/CreadoPor/ModificadoEn/ModificadoPor` automáticamente, `appsettings.json`/`appsettings.Development.json` sin credenciales reales.
- **Autorización por rol** (sesión 2026-09-15): atributo propio `AutorizarRolesAttribute` (`OPT.API/Authorization/`) que lee el claim numérico `rolId` del JWT — se eligió sobre `[Authorize(Roles=...)]` porque ese mecanismo compara contra `ClaimTypes.Role`, que el token no emite. Constantes y grupos de roles en `OPT.Domain/Common/RolesOPT.cs` (los 8 roles reales de `OPT_Rol`: Administrador=1, Supervisor=2, Operador=3, Jefe Sucursal=4, Vendedor=5, Técnico Médico=6, Control Calidad=7, Externo=8). El legacy **no tenía ningún control de rol a nivel de servidor** (solo ocultaba ítems de menú) — el mapeo rol→acción es diseño nuevo de esta sesión, no una migración de una regla existente; ver el detalle de qué rol puede hacer qué en `Manual_Tecnico_Seguridad_OPT.docx` sección 5 (o el propio `RolesOPT.cs`, que lo documenta grupo por grupo). Aplicado a: `UsuariosController` (Administrador exclusivo), `SucursalesController`/`EmpresasController` (escrituras acotadas a Administrador/Supervisor), `Clientes`/`Anamnesis`/`RecetaCristalesController` (personal clínico/comercial, sin Control Calidad ni Externo), `OrdenesDeTrabajoController` (por acción — Anular/AnularCuota más restringidos que el resto, CambiarEstado suma Control Calidad), `CobranzaController` (supervisión). Los catálogos de solo lectura (Roles, Regiones, Comunas, EstadosOT, FormasPago, EstadosCuota, Productos) quedaron sin restricción de rol — no son datos sensibles y los necesita cualquier pantalla.
- **Autorización por sucursal / BOLA** (sesión 2026-09-15): helper `AutorizacionSucursal.ValidarAcceso(currentUser, sucursalId)` (`OPT.Application/Common/Security/`), invocado desde los handlers de `OrdenesDeTrabajo` que cargan o crean una OT (Crear, Actualizar, CambiarEstado, Anular, RegistrarAbono, RegistrarPago, GenerarPlanCuotas, PagarCuota, AnularCuota, ObtenerPorId) — lanza `ForbiddenAccessException` (nuevo, mapeada a HTTP 403 en `ExceptionHandlingMiddleware`) si la OT no es de la sucursal activa del usuario (claim `sucursalId`) ni de ninguna de sus sucursales asignadas (claim nuevo `sucursales`, CSV de `UsuarioSucursal`, agregado en `TokenService`). Administrador tiene alcance nacional (`RolesOPT.AccesoTotalSucursales`) y no pasa por esta validación. El listado (`ObtenerOrdenesDeTrabajoQuery`) aplica la misma regla: si el usuario no tiene alcance nacional y no filtra por sucursal, se restringe de oficio a su sucursal activa en vez de devolver todas. Otras entidades con `SucursalId` propio (`UsuarioSucursal`, `EmpresaSucursal`, `ProductoSucursal`) no se tocaron: son tablas de relación sin API de listado directo, ya cubiertas por la restricción de rol de su controller padre.

**Falta — decidido dejar para la pasada final, antes de producción:**
1. Rate limiting en `/api/auth/login` (built-in `AddRateLimiter` de .NET 8).
2. Security headers (`UseHsts`, `X-Content-Type-Options`, CSP básica).
3. Logging de eventos de seguridad (login fallido, cambio de rol/permiso) — hoy `LoginCommandHandler` no registra intentos fallidos.
4. Refresh tokens / revocación de JWT (mitigable mientras tanto acortando `Jwt:ExpiresMinutes` si preocupa la ventana de 480 min).

**Falta — backlog sin urgencia de seguridad real:**
- Health checks, versionamiento de API, límite de tamaño de request. Son robustez operacional, no controles de seguridad crítica para este dominio.

**Pendiente relacionado, no resuelto en esta sesión:**
- No se corrió `dotnet test`/pruebas end-to-end contra `dbOPT_NET` con sesión autenticada de cada rol — solo `dotnet build OPT.sln` (compila limpio). Verificar en una sesión con acceso a la base real antes de dar por cerrado el punto.
- El mapeo rol→acción es una decisión de diseño nueva (no legacy) — si el negocio la corrige (p. ej. Control Calidad necesita crear OT, o Técnico Médico necesita verla), ajustar `RolesOPT.cs` y los atributos de los controllers afectados, no un mecanismo nuevo.

## Por qué esta secuencia (ahora vs. al final)

Decisión tomada con el usuario el 2026-09-15: lo que toca **diseño de endpoints/dominio** (roles, BOLA por sucursal) se resuelve ahora, porque cada módulo nuevo construido sobre `[Authorize]` genérico aumenta el costo de retrofit después. Lo que es **middleware transversal puro** (rate limiting, headers, health checks) se agrega una vez al final sin tocar lógica de negocio — no hay penalidad por posponerlo.

Fecha límite real detrás de esto: Ley 21.719 obliga desde **01-12-2026** y el sistema maneja datos de salud (recetas, anamnesis) — ver ADR `0004` para el detalle de cumplimiento pendiente (retención/purga, consentimiento explícito, DPO, notificación de brechas, portabilidad).

## Forma de trabajo esperada de la IA en este tema

- Antes de agregar un controller o endpoint nuevo: confirmar si el recurso es "propiedad" de una sucursal/cliente y si corresponde agregar el chequeo de autorización correspondiente — no asumir que `[Authorize]` genérico alcanza.
- No introducir dependencias de seguridad nuevas (ASP.NET Core Identity completo, IdentityServer, etc.) sin que el usuario lo pida — la solución actual (JWT propio + BCrypt) es deliberada y liviana; ampliar significa roles/policies sobre lo que ya existe, no reemplazarlo.
- No implementar los puntos de "backlog sin urgencia" salvo pedido explícito — no son controles de seguridad, son robustez operacional, y agregarlos no pedido es over-engineering para el estado actual del proyecto.
- Cualquier control de seguridad nuevo que se implemente debe reflejarse en `Manual_Tecnico_Seguridad_APIs.docx` (`src/documentos/`) y en este archivo (mover el ítem de "falta" a "implementado").

## Cómo mantener este archivo

Cuando se resuelva un punto de la lista de "falta" (roles, autorización por sucursal, rate limiting, etc.):
1. Mover el ítem a "Implementado y verificado en código", citando el mecanismo usado (no hace falta archivo:línea acá — eso va en el manual `.docx`).
2. Actualizar la sección correspondiente de `Manual_Tecnico_Seguridad_OPT.docx`.
3. Si el cambio afecta el estado descrito en `CLAUDE.md` (sección de pendientes), actualizarlo también ahí.

Este archivo no reemplaza una auditoría de seguridad real — es contexto para que un agente IA no repita el mismo análisis ni pierda de vista lo ya decidido entre sesiones.
