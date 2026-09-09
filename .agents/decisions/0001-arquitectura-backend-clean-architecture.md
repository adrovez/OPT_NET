# 0001 — Arquitectura de backend en capas (.NET)

**Estado:** Aceptada
**Fecha:** 2026-08-19
**Implementada:** 2026-08-19 (Fase 0 — scaffold inicial)

## Contexto

El backend legacy (`old/Fuente/OPT.Web` + `OPT.Dato` + `OPT.Entidad`) es un monolito ASP.NET MVC 5 sobre .NET Framework 4.8, con lógica de negocio embebida en los controllers, acceso a datos vía Entity Framework 6 Database First envuelto en clases `*DAL` por entidad, manejo de errores por `catch/throw ex` disperso, autenticación por `FormsAuthentication` + sesión en servidor (`InProc`), y sin proyectos de prueba. Esto dificulta el testeo, el escalamiento horizontal y la evolución independiente de las reglas de negocio respecto de la infraestructura (base de datos, correo, integraciones externas).

## Alternativas consideradas

- **Mantener MVC monolítico pero actualizado a .NET moderno**: descartado — no resuelve el acoplamiento entre lógica de negocio, acceso a datos y presentación; solo actualiza la versión del framework.
- **Arquitectura en capas / Clean Architecture (Domain, Application, Infrastructure, API)**: separa las reglas de negocio de los detalles técnicos (base de datos, framework web), permite testear la lógica de negocio de forma aislada, y hace explícitas las dependencias (siempre hacia el dominio).
- **CQRS con un mediador de comandos/consultas** (patrón complementario, no excluyente): facilita mantener los controllers delgados y las reglas de negocio organizadas por caso de uso en vez de por controller.

## Decisión

Backend en **.NET 8** (LTS vigente al iniciar el desarrollo), expuesto como **API REST**, organizado en capas: Domain → Application → Infrastructure → API, con dependencias apuntando siempre hacia el dominio. Entity Framework Core con Migrations reemplaza el modelo Database First del legacy. Autenticación basada en tokens (JWT) reemplaza `FormsAuthentication` + sesión en servidor.

### Decisiones técnicas derivadas (tomadas en el scaffold — Fase 0)

Estas opciones se fijaron al implementar el scaffold y son parte de esta decisión:

| Aspecto | Elección | Motivo |
|---------|----------|--------|
| Mediador CQRS | **MediatR 12** | Estándar de facto en el ecosistema .NET, cero dependencias de infraestructura en los handlers |
| Validación de entrada | **FluentValidation 11** | Expresivo, testeable de forma aislada, integración limpia con el pipeline de MediatR |
| Hashing de contraseñas | **BCrypt.Net-Next** (work factor 12) | Función diseñada para contraseñas (lenta por diseño), corrección directa del hallazgo crítico del legacy |
| Formato de errores HTTP | **ProblemDetails** (RFC 7807) | Respuesta consistente y estándar para todos los errores — traducida por `ExceptionHandlingMiddleware` |
| Documentación de la API | **Swagger / OpenAPI** | Habilitado en Development; incluye soporte JWT para pruebas manuales |

## Consecuencias

- Los controllers quedan delgados (sin lógica de negocio, sin try/catch) — el manejo de excepciones se centraliza en `ExceptionHandlingMiddleware`.
- La lógica de negocio (Application) se puede testear sin levantar base de datos ni servidor web.
- Cambiar de proveedor de correo, de integración contable, o incluso de motor de base de datos, no debería requerir tocar las reglas de negocio.
- Requiere más código de "andamiaje" (interfaces, DTOs, mapeos) que un CRUD directo sobre EF — se acepta ese costo a cambio de mantenibilidad a largo plazo.
- El equipo debe familiarizarse con el patrón si no lo ha usado antes (curva de aprendizaje inicial).

## Estado de implementación

El scaffold de `src/backend/` fue generado en la Fase 0 (2026-08-19) e incluye:

- `OPT.Domain` — `AuditableEntity`, `DomainException`, entidades de los 4 módulos iniciales (Organización, Clínico, Comercial, Inventario), interfaces de repositorio.
- `OPT.Application` — `IUnitOfWork`, `IPasswordService`, `ITokenService`, `ICurrentUserService`, `ValidationBehaviour`, feature `Login` completa.
- `OPT.Infrastructure` — `AppDbContext`, `AuditInterceptor`, `RepositorioBase<T>` con filtro de borrado lógico, `PasswordService` (BCrypt), `TokenService` (JWT), repositorios concretos.
- `OPT.API` — `ExceptionHandlingMiddleware`, `AuthController`, stubs de controllers para módulos pendientes, `Program.cs`.

El scaffold **no incluye aún**: Migrations de EF Core (pendiente de configurar cadena de conexión via `user-secrets`), proyecto de pruebas, ni los casos de uso de Clientes, OT e Inventario (se agregan módulo a módulo en Fase 1 en adelante).

## Qué pasa con `src/` actual

La iteración previa (`.NET 10, multi-tenant, GUID como PK`) fue dejada en `src/` sin modificar. El scaffold nuevo coexiste en `src/backend/`. La decisión de archivar o eliminar la iteración previa sigue pendiente del equipo — ver `AGENTS.md`, sección "Decisiones pendientes".
