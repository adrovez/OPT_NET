# Progress Log — OPT

Historial de sesiones de trabajo con agentes IA. Cada entrada se agrega al final (orden cronológico ascendente), nunca se reescribe una entrada pasada.

Formato de cada entrada: fecha, resumen de lo hecho, decisiones tomadas, próximos pasos sugeridos.

---

## 2026-08-19 — Análisis del legacy y arquitectura de documentación IA

**Resumen:**
- Se analizó el código fuente legacy en `old/Fuente/` (proyectos OPT.Web, OPT.Dato, OPT.Entidad — ASP.NET MVC 5 / .NET Framework 4.8 / EF6 Database First) y los scripts de base de datos en `old/BD/` (27 tablas, 18 procedimientos almacenados de la base `db_a25cfd_opt2`). No se modificó ningún archivo de `old/`.
- Se identificaron hallazgos críticos de seguridad: contraseñas de usuario en texto plano (columna `OPT_Usuario.Clave`, comparación directa en el login, reenvío de la clave original por correo en "Recuperar contraseña"), credenciales de base de datos y de la integración Defontana en texto plano en `Web.config` y en la tabla `OPT_Defontana`.
- Se identificaron hallazgos de diseño de base de datos: claves primarias sobre RUT (dato de negocio), generación de IDs fuera de la base para `OPT_OrdenDeTrabajo`, ausencia de auditoría y borrado lógico, campos calculados sin control transaccional, catálogos denormalizados, tablas temporales en el esquema de producción (`OPT_CuotaTEMP`, `OPT_ProductoTemp`), falta de índices explícitos sobre columnas FK.
- Se produjo un documento formal (`docs/architecture/OPT_Propuesta_Arquitectura.docx`) con: análisis del legacy, arquitectura propuesta (Domain/Application/Infrastructure/API + SPA), comparación detallada Angular vs. Blazor (recomendación: Angular), propuesta de mejoras de base de datos, plan de migración en 6 fases, y preguntas abiertas.
- Se creó la arquitectura de documentación IA (este archivo, `AGENTS.md`, `CLAUDE.md`, `.agents/decisions/`, `.agents/context/`), reemplazando la versión anterior de `AGENTS.md`/`CLAUDE.md` que reflejaba una iteración de desarrollo previa en `src/` (.NET 10 + Angular 21, multi-tenant) construida antes de este análisis formal.

**Decisiones tomadas (ver ADRs para el detalle):**
- `0001` — Backend en .NET 8+ con arquitectura en capas (Clean Architecture), reemplazando el monolito MVC.
- `0002` — Frontend recomendado: Angular (sobre Blazor), pendiente de confirmación final con el equipo.
- `0003` — Mejoras de base de datos: IDs sintéticos, auditoría, borrado lógico, credenciales seguras, catálogos normalizados, índices explícitos.

**Sin resolver / pendiente para el equipo:**
- Qué hacer con el código ya existente en `src/` (iteración previa): ¿se archiva, se reconcilia con las nuevas decisiones, o se descarta?
- Modelo mono-óptica vs. multi-tenant.
- Angular vs. Blazor — confirmación final según el perfil real del equipo de desarrollo.
- Convivencia legado/nuevo durante la migración, o corte único por módulo.
- Volumen real de datos a migrar.

**Próximos pasos sugeridos:**
1. Resolver las decisiones pendientes listadas arriba (bloquean el diseño de detalle de la Fase 0).
2. Definir el scaffold inicial de `src/` (Fase 0 del plan de migración): estructura de carpetas del backend y frontend, pipeline de build/pruebas, esqueleto de autenticación.
3. Completar `.agents/context/` con más detalle de reglas de negocio a medida que se migran módulos específicos.

---

## 2026-08-19 — Scaffold del backend (Fase 0)

**Resumen:**
- Se generó el scaffold completo del backend en `.NET 8 / Clean Architecture` para `src/backend/`, siguiendo los ADRs `0001` y `0003` y las reglas de `CLAUDE.md`. El scaffold se entregó como `src/backend/opt_backend_scaffold.zip` (55 archivos, 4 proyectos).
- **OPT.Domain**: `AuditableEntity` (base con auditoría + borrado lógico), `DomainException`, 12 entidades de negocio organizadas en 4 módulos (Organización, Clínico, Comercial, Inventario) con sus reglas de negocio invariantes embebidas, interfaces de repositorio genéricas y específicas.
- **OPT.Application**: `IUnitOfWork`, `IPasswordService`, `ITokenService`, `ICurrentUserService`, `ValidationBehaviour` (pipeline MediatR + FluentValidation), feature `Login` completa (Command + Handler + Validator) con las reglas preservadas del legacy (login por RUT, sucursal activa asignada post-login).
- **OPT.Infrastructure**: `AppDbContext`, `AuditInterceptor` (rellena campos de auditoría automáticamente en `SaveChanges`), `RepositorioBase<T>` con filtro global de borrado lógico, `PasswordService` (BCrypt work factor 12 — corrección directa del hallazgo crítico), `TokenService` (JWT — clave desde `IConfiguration`), `CurrentUserService`, `DependencyInjection.cs` con registro de todas las dependencias.
- **OPT.API**: `ExceptionHandlingMiddleware` (traduce todas las excepciones a `ProblemDetails`), `AuthController` (POST /api/auth/login), stubs de controllers para Clientes, OT e Inventario, `Program.cs` con pipeline completo y Swagger+JWT.
- Se actualizaron todos los archivos `.md` relevantes: ADR `0001` marcado como **Aceptada**, `CLAUDE.md` con comandos de build reales y árbol de la estructura, `src/AGENTS.md` reescrito para reflejar el scaffold, `AGENTS.md` raíz con estado actualizado.

**Decisiones tomadas:**
- ADR `0001` pasa de estado **Propuesta** a **Aceptada** — el scaffold implementa la decisión.
- Librería de hashing: **BCrypt.Net-Next** (work factor 12). No se creó ADR separado — se documenta como decisión derivada dentro de `0001`.
- Mediador CQRS: **MediatR 12** + **FluentValidation 11**. Idem anterior.
- Formato de errores: **ProblemDetails** (RFC 7807), centralizado en `ExceptionHandlingMiddleware`.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Qué hacer con la iteración previa en `src/` (`.NET 10, multi-tenant`): ¿archivar o eliminar?
- Modelo mono-óptica vs. multi-tenant (bloquea diseño final del esquema).
- Angular vs. Blazor — scaffold del frontend pendiente de esta decisión.
- Configurar `user-secrets` locales para primera ejecución (cadena de conexión + clave JWT).
- Definir `IEntityTypeConfiguration<T>` para cada entidad (carpeta `OPT.Infrastructure/Persistence/Configurations/` está vacía).
- Crear primera Migration de EF Core y scripts de datos iniciales (`src/basedatos/`).

**Próximos pasos sugeridos:**
1. Extraer `opt_backend_scaffold.zip` en `src/backend/`.
2. Configurar `dotnet user-secrets` (cadena de conexión real + clave JWT ≥32 chars).
3. Completar las `IEntityTypeConfiguration<T>` para cada entidad (convenciones de nombres de tabla, índices, unicidad de RUT).
4. Ejecutar `dotnet ef migrations add Initial` y levantar la API.
5. Implementar los primeros casos de uso de Clientes (Fase 1): `CrearCliente`, `ObtenerCliente`, `ActualizarCliente`, `EliminarCliente`.
6. Resolver Angular vs. Blazor para iniciar el scaffold del frontend.

---

## 2026-08-19 — Configuraciones EF Core y seed data (catálogos base)

**Resumen:**
- Se analizaron en detalle los scripts del legacy (`old/BD/Create_Table.sql`) para validar el diseño de las entidades de catálogo y detectar diferencias con el scaffold.
- **Nuevas entidades en OPT.Domain**: `CatalogEntity` (clase base sin auditoría para datos de referencia estáticos), `Region` y `Comuna` — que no estaban en el scaffold inicial.
- **Corrección en Empresa.cs**: se agregó el campo `Contacto` (nombre de la persona de contacto), presente en `OPT_Empresa.Contacto varchar(50)` del legacy pero omitido en el scaffold.
- **AppDbContext** actualizado: se agregaron `DbSet<Region>` y `DbSet<Comuna>`.
- **AppDbContextFactory**: nueva clase `IDesignTimeDbContextFactory<AppDbContext>` con `DesignTimeCurrentUserService` interno, necesaria para que `dotnet ef migrations add` funcione sin user-secrets activos.
- **EntityTypeConfigurations** (8 archivos, carpeta `OPT.Infrastructure/Persistence/Configurations/Organizacion/`):
  - `RegionConfiguration` — 16 regiones oficiales de Chile (fuente INE), `ValueGeneratedNever`, índice único en `CodigoOficial`.
  - `ComunaConfiguration` — 346 comunas completas de Chile agrupadas por región (fuente INE), `ValueGeneratedNever`.
  - `RolConfiguration` — 3 roles seed: Administrador, Supervisor, Operador.
  - `EmpresaConfiguration` — RUT como atributo único (no PK), campos de auditoría explícitos, campo Contacto incluido.
  - `SucursalConfiguration` — índice de unicidad filtrado para garantizar una sola sucursal Matriz activa.
  - `UsuarioConfiguration` — RUT único (no PK), FK a Rol y SucursalActiva, `ClaveHash` mapeado (nunca texto plano).
  - `UsuarioSucursalConfiguration` — FK por `int UsuarioId` (corrección del legacy que usaba `varchar RutUsuario` como FK).
  - `EmpresaSucursalConfiguration` — relación muchos-a-muchos Empresa-Sucursal con índice de unicidad compuesto.
- Todos los archivos escritos directamente en `src/backend/` del repositorio vía device bridge.

**Decisiones tomadas:**
- `CatalogEntity` no hereda de `AuditableEntity` — los catálogos geográficos (Region, Comuna) son datos estáticos que no requieren auditoría ni borrado lógico.
- IDs de Region y Comuna: `ValueGeneratedNever` (IDs fijos INE, no IDENTITY) — facilita seed declarativo y referencias cruzadas predecibles.
- No se hace seed de usuario administrador — el equipo lo decidió así; se creará manualmente en el primer setup.
- No se hace seed de Empresa ni Sucursal — datos de configuración inicial que ingresa el administrador.

**Sin resolver / pendiente para el equipo (heredado):**
- Qué hacer con la iteración previa en `src/` (`.NET 10, multi-tenant`): ¿archivar o eliminar?
- Modelo mono-óptica vs. multi-tenant.
- Angular vs. Blazor — scaffold del frontend pendiente.

**Próximos pasos sugeridos:**
1. Configurar `dotnet user-secrets` con la cadena de conexión a `dbOPT_NET` y la clave JWT:
   ```
   cd src/backend/OPT.API
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=dbOPT_NET;Trusted_Connection=True;TrustServerCertificate=True;"
   dotnet user-secrets set "Jwt:Key" "<clave-aleatoria-minimo-32-chars>"
   ```
2. Generar la primera Migration con el seed completo:
   ```
   cd src/backend
   dotnet ef migrations add Fase0_Catalogs --project OPT.Infrastructure --startup-project OPT.API
   dotnet ef database update --project OPT.Infrastructure --startup-project OPT.API
   ```
3. Verificar en SQL Server Management Studio que `dbOPT_NET` tiene las tablas `Regiones` (16 filas), `Comunas` (346 filas) y `Roles` (3 filas).
4. Continuar con Fase 1: casos de uso de Clientes (`CrearCliente`, `ObtenerCliente`, `ActualizarCliente`, `EliminarCliente`) incluyendo FK a `ComunaId`.
5. Resolver Angular vs. Blazor para iniciar el scaffold del frontend.

---

## 2026-08-19 — Esquema completo como script SQL (reemplazo de EF Migrations) + corrección de bugs de compilación

**Resumen:**
- Decisión del equipo: el esquema de base de datos se gestiona como **script SQL versionado en `src/basedatos/`**, no como Migrations de EF Core aplicadas en runtime (`dotnet ef database update` queda descartado para todos los ambientes). El modelo de EF Core sigue siendo la fuente de verdad del diseño; el script se regenera con `dotnet ef migrations script --idempotent` y la migration C# temporal se descarta (`dotnet ef migrations remove`) — no se versiona la carpeta `Migrations/`.
- **Completadas las `IEntityTypeConfiguration<T>` faltantes** (9 archivos nuevos) para los módulos Clínico (`Cliente`, `Anamnesis`, `RecetaCristales`), Comercial (`OrdenDeTrabajo`, `DetalleOT`, `Abono`, `BitacoraOT`) e Inventario (`Producto`, `ProductoSucursal`), siguiendo el patrón ya usado en Organización y contrastando tipos/longitudes contra `old/BD/Create_Table.sql`.
- **`AppDbContext`**: se agregó `modelBuilder.HasSequence<int>("SEQ_NumeroOT")` — `OrdenDeTrabajo.NumeroOT` ahora se genera en la BD vía `NEXT VALUE FOR`, nunca en la capa de aplicación (ADR 0003).
- **Bugs preexistentes descubiertos y corregidos** (bloqueaban la compilación del scaffold desde su generación inicial; no relacionados con las nuevas configuraciones):
  - Faltaba la entidad `EmpresaSucursal` (referenciada por `Empresa`, su configuración y `AppDbContext`, pero el archivo nunca se creó).
  - `UsuarioSucursal` no tenía propiedad `Id` pese a que `UsuarioSucursalConfiguration` ya la usaba.
  - `Usuario` no tenía `Activo` ni la navegación `SucursalActiva` pese a que `UsuarioConfiguration` ya las usaba.
  - `Rol` heredaba de `AuditableEntity` en vez de `CatalogEntity` — el seed de `RolConfiguration.HasData()` no provee `CreadoEn`/`CreadoPor`, requeridos por `AuditableEntity`; `CatalogEntity` ya documentaba a `Rol` como caso de uso previsto.
  - Faltaba el paquete `FluentValidation.DependencyInjectionExtensions` en `OPT.Application.csproj` (`AddValidatorsFromAssembly` no existía).
  - `AppDbContextFactory.DesignTimeCurrentUserService` no implementaba `ICurrentUserService` correctamente (tipos de retorno no coincidían: `int?`/`string?` vs. `int`/`string`, y faltaba `SucursalId`).
  - `UsuarioSucursalConfiguration` dejaba la navegación `Sucursal.Usuarios` sin vincular (`WithMany()` sin lambda), lo que generaba una FK sombra duplicada (`SucursalId1`) al generar la migration.
  - Faltaba el paquete `Microsoft.EntityFrameworkCore.Design` en `OPT.API.csproj` (requerido por las EF Core Tools sobre el proyecto de arranque).
- Se generó `src/basedatos/001_esquema_inicial.sql` (16 tablas, `SEQUENCE` de `NumeroOT`, seeds de Regiones/Comunas/Roles vía `HasData()`), validado con `dotnet build` limpio antes y después de remover la migration C# temporal.
- Documentación actualizada: `CLAUDE.md` (estado actual, comandos, regla de BD, gotchas), `src/AGENTS.md` (árbol, sección BD), `AGENTS.md` raíz (stack, regla crítica 8).

**Decisiones tomadas:**
- Esquema de BD versionado como script SQL manual en `src/basedatos/`, no como EF Migrations en runtime — decisión explícita del equipo, documentada en los tres archivos `.md` de instrucciones.
- Se instaló la herramienta global `dotnet-ef` (10.0.11) para poder generar el script; queda como prerrequisito documentado en `CLAUDE.md`.
- Los campos de graduación óptica en `RecetaCristales` (esfera/cilindro) usan `decimal(5,2)`; los montos monetarios usan `decimal(18,2)` en todo el módulo Comercial/Inventario.
- Las FK a catálogos aún no modelados como entidades propias (`EstadoOTId`, `FormaPagoId`, `CategoriaId`) quedan como columnas `int` simples sin constraint — los catálogos correspondientes (`EstadoOT`, `FormaPago`, `CategoriaProducto`) no existen todavía en `OPT.Domain` y su diseño no estaba en el alcance de esta sesión.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Diseñar y crear las entidades de catálogo `EstadoOT`, `FormaPago`, `CategoriaProducto` (o `TipoDocumento` si aplica) — hoy son solo `int` sin FK real.
- Qué hacer con la iteración previa en `src/` (`.NET 10, multi-tenant`): ¿archivar o eliminar?
- Modelo mono-óptica vs. multi-tenant.
- Angular vs. Blazor — scaffold del frontend pendiente.

**Próximos pasos sugeridos:**
1. Aplicar `001_esquema_inicial.sql` contra `dbOPT_NET` en desarrollo y verificar las 16 tablas + seeds.
2. Diseñar las entidades de catálogo pendientes (`EstadoOT`, `FormaPago`, `CategoriaProducto`) y regenerar el script (`002_catalogos_comerciales.sql`) siguiendo el procedimiento documentado en `CLAUDE.md`.
3. Continuar con Fase 1: casos de uso de Clientes.
4. Resolver Angular vs. Blazor para iniciar el scaffold del frontend.

---

## 2026-08-19 — ADR 0004 (Ley 21.719): naming OPT_ + PublicId no enumerable

**Resumen:**
- El usuario pidió tres cambios sobre `001_esquema_inicial.sql`: (1) prefijo `OPT_` en las tablas, (2) nombres en singular, (3) reemplazar el RUT como PK de `Cliente` por un GUID — señalando que tanto el RUT de `Cliente` como de `Empresa` son datos críticos — y pidió considerar la Ley N° 21.719 de Protección de Datos Personales. Se aclaró primero que el RUT ya no era la PK (esa corrección la hizo ADR 0003; la PK ya era `int IDENTITY`), y se investigó la ley vía web (publicada 13-12-2024, obligatoria desde 01-12-2026, datos de salud/biométricos clasificados como sensibles, derechos ARCO+, multas hasta 20.000 UTM) antes de proponer un enfoque.
- Se preguntó al usuario y se resolvió: (a) renombrar las 17 tablas a `OPT_` + singular, todas — incluidos catálogos; (b) no reemplazar el `int IDENTITY` por GUID como PK (evita cascada de cambios de tipo en todas las FK y fragmentación de índice clustered) — en su lugar, agregar una columna `PublicId` (`Guid`, `DEFAULT NEWID()`, único) que es la que se expone en API/URLs; (c) extender `PublicId` más allá de Cliente/Empresa a `Usuario`, `Anamnesis` y `RecetaCristales` por ser datos sensibles/credenciales; (d) antes de tocar el esquema, documentar un ADR de cumplimiento — el usuario explícitamente pidió "detener" la implementación hasta tenerlo.
- Se redactó `.agents/decisions/0004-cumplimiento-ley-21719.md` (estado: Aceptada) documentando la clasificación de datos sensibles (`Anamnesis`, `RecetaCristales`), la decisión técnica de `PublicId`, la convención de naming, y — explícitamente fuera de alcance — los puntos de cumplimiento que requieren decisión de negocio/legal antes de producción: derecho de cancelación real vs. borrado lógico actual, consentimiento explícito para datos de salud, necesidad de DPO, proceso de notificación de brechas en 72h, portabilidad de datos. El ADR se presentó al usuario para aprobación antes de tocar código.
- Una vez aprobado, se implementó: las 17 `IEntityTypeConfiguration<T>` actualizaron su `.ToTable(...)` a `OPT_<Entidad>` singular; se agregó la propiedad `Guid PublicId` a `Cliente`, `Empresa`, `Usuario`, `Anamnesis` y `RecetaCristales` (dominio) con su mapeo (`DEFAULT NEWID()`, índice único) en la configuración correspondiente. Se regeneró `001_esquema_inicial.sql` con el mismo procedimiento de la sesión anterior (`dotnet ef migrations add` → `migrations script --idempotent` → `migrations remove`), validado con `dotnet build` limpio.

**Decisiones tomadas:**
- Ver el detalle completo en `.agents/decisions/0004-cumplimiento-ley-21719.md`. En síntesis: naming `OPT_` + singular en las 17 tablas; `PublicId` (Guid, no enumerable) como identificador de exposición externa en 5 tablas con datos personales/sensibles, sin reemplazar el `int IDENTITY` interno.
- Los nombres de índices/constraints (`UQ_Clientes_Rut`, `FK_Usuarios_Roles`, etc.) **no** se renombraron a singular — quedan con la convención plural preexistente por no ser parte de lo pedido explícitamente; es una inconsistencia menor de estilo que puede normalizarse en una sesión futura si se decide.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Todos los puntos listados en la sección "Pendiente" del ADR 0004: política de retención/purga real (derecho de cancelación), consentimiento explícito para datos de salud, evaluación de necesidad de DPO, proceso de notificación de brechas, portabilidad de datos. Son bloqueantes para producción antes del 01-12-2026, no para continuar Fase 1.
- Diseñar las entidades de catálogo pendientes (`EstadoOT`, `FormaPago`, `CategoriaProducto`) — siguen sin FK real.
- Qué hacer con la iteración previa en `src/` (`.NET 10, multi-tenant`), modelo mono-óptica vs. multi-tenant, Angular vs. Blazor.

**Próximos pasos sugeridos:**
1. Aplicar el `001_esquema_inicial.sql` actualizado contra `dbOPT_NET` (reemplaza cualquier aplicación previa del script anterior con nombres en plural — no hay datos productivos aún, así que no hay migración de datos que resolver).
2. Al implementar los controllers/DTOs de Fase 1, usar `PublicId` en las rutas de API de `Cliente`, no el `Id` interno — el ADR 0004 lo deja como regla explícita.
3. Definir con el equipo (con asesoría legal si corresponde) los puntos pendientes de la Ley 21.719 antes de diciembre 2026.
4. Continuar con Fase 1: casos de uso de Clientes.

---

## 2026-08-19 — Refresco de documentación IA + Manual Técnico (.docx)

**Resumen:**
- Se actualizaron `CLAUDE.md`, `AGENTS.md` (raíz) y `src/AGENTS.md` para reflejar el estado real del código tras las dos sesiones anteriores: 17 tablas (no 16) con prefijo `OPT_` + singular, columna `PublicId` en `Cliente`/`Empresa`/`Usuario`/`Anamnesis`/`RecetaCristales` documentada como regla no negociable (con referencia a ADR `0004`), `CatalogEntity` como base de catálogos estáticos, `EmpresaSucursal` agregada al árbol de entidades, y referencia al nuevo Manual Técnico en las tablas de "Documentación de referencia" de los tres archivos.
- Se generó `src/documentos/Manual_Tecnico_OPT.docx` (documento nuevo, ~52 KB, 12 secciones: introducción, arquitectura general, OPT.Domain, OPT.Application, OPT.Infrastructure, OPT.API, Base de Datos con referencia completa de las 17 tablas generada a partir de `001_esquema_inicial.sql`, Seguridad y Cumplimiento (Ley 21.719), Convenciones de código, Comandos, Gotchas, Registro de ADRs). Generado con `python-docx` (se instaló como dependencia de esta sesión, `pip install python-docx`) vía un script en el scratchpad — no quedó ningún script ni dependencia Python agregada al repositorio del proyecto.

**Decisiones tomadas:**
- El Manual Técnico cubre explícitamente Base de Datos y Backend, tal como se pidió — no incluye frontend (inexistente todavía) ni el legacy (ya documentado aparte).
- La referencia de tablas del manual (sección 7.7) se construyó a mano a partir de una lectura completa de `001_esquema_inicial.sql`, no con un parser genérico — para poder anotar contexto (qué campos son datos sensibles, qué FKs faltan) que un parser no tendría.

**Sin resolver / pendiente para el equipo (heredado):**
- Los mismos puntos listados en la entrada anterior (Ley 21.719, catálogos `EstadoOT`/`FormaPago`/`CategoriaProducto`, multi-tenant, Angular vs. Blazor).

**Próximos pasos sugeridos:**
1. Revisar el Manual Técnico generado y ajustar si algo no refleja la intención real del equipo (es un documento vivo, no definitivo).
2. Aplicar `001_esquema_inicial.sql` contra `dbOPT_NET`.
3. Continuar con Fase 1: casos de uso de Clientes.

---

## 2026-08-21 — Scaffold del frontend (Angular)

**Resumen:**
- Se generó el scaffold base del frontend en `src/frontend/` (antes vacío). Se confirmó con el usuario, antes de generar código: (a) Angular sobre Blazor — ADR `0002` pasa de Propuesta a **Aceptada**; (b) Angular Material como librería de UI sobre PrimeNG/Tailwind; (c) generar un proyecto real compilable (Angular CLI + `node_modules`) en vez de solo una plantilla de carpetas.
- Stack generado: **Angular 21.0.5**, standalone, **zoneless** (sin `zone.js`, `provideZonelessChangeDetection()`), Angular Material 21 (Material 3, tema custom), Vitest como test runner, ESLint (`angular-eslint`) + Prettier. Node instalado (v24.11.1) no soportaba Angular CLI 22 (exige un patch de Node más nuevo); se usó CLI 21 sin problema.
- Estructura: `core/` (Auth, TokenStorage, authGuard, authInterceptor/errorInterceptor funcionales, modelos de auth), `shared/` (Toast sobre MatSnackBar — reemplaza SweetAlert; ApiProblemDetails), `layout/` (Shell — toolbar+sidenav para la zona autenticada; AuthLayout — contenedor para login), `features/auth` (login funcional), `features/clientes` (modelo+servicio+listado, contra los campos reales de `Cliente.cs`), `features/ordenes-de-trabajo` e `features/inventario` (placeholders — sus controllers en el backend son stubs vacíos). Rutas raíz componen `/login` público vs. shell con children lazy-loaded protegidos por `authGuard`.
- El feature Auth se construyó contra el contrato **exacto** ya implementado en el backend (`AuthController`, `LoginCommand`/`LoginResult`, `TokenService`): login por RUT+clave (no email/username), respuesta `{ token, usuarioId, nombreCompleto, sucursalActivaId }`, claims del JWT (`sub`, `rut`, `nombre`, `rolId`, `sucursalId`). El feature Clientes se construyó contra los campos reales de `OPT.Domain/Entities/Clinico/Cliente.cs` (el controller es stub, sin endpoints todavía).
- Se corrigieron todos los `*.spec.ts` generados por el CLI que quedaron rotos por las nuevas dependencias (Router/HttpClient vía DI) — build, lint y los 14 tests unitarios pasan limpio; build de producción dentro de budgets (440 kB inicial, bajo el warning de 500 kB).
- Se instaló `@angular/animations` (faltaba para `provideAnimationsAsync`, requerido por Angular Material) y `prettier` como dev dependency (el `package.json` del CLI trae la config de Prettier pero no el paquete).
- `angular.json` configurado con `fileReplacements` para `environment.ts`/`environment.development.ts` (build production/development respectivamente).
- Se escribió `src/frontend/README.md` documentando stack, estructura, convención para agregar features nuevos, y las decisiones específicas del frontend (sesión en `sessionStorage`, reconstrucción de sesión decodificando el JWT por no existir todavía `/me`, `PublicId` en rutas, sin NgRx).
- Se actualizaron `AGENTS.md` (raíz) y `CLAUDE.md`: estado del frontend, comandos reales (`npm start`/`build`/`test`/`lint`/`format`), tabla de stack, se removió "¿Angular o Blazor?" de "Decisiones pendientes".

**Decisiones tomadas:**
- ADR `0002` — **Aceptada**: Angular confirmado por el usuario (2026-08-21).
- Angular Material como librería de componentes (sobre PrimeNG y Tailwind+custom) — decisión del usuario vía pregunta directa.
- Zoneless en vez de zone.js — alineado con la recomendación de la propia CLI 21 y con signals como mecanismo de reactividad ya mencionado en el ADR `0002`.
- JWT en `sessionStorage`, no `localStorage` — se pierde al cerrar la pestaña, acota la ventana de exposición para un sistema con datos clínicos sensibles (Ley 21.719). Decisión tomada sin preguntar, por ser consistente con el espíritu de AGENTS.md regla 12 (no persistir más que el token) y con el enfoque conservador que ya tomó el equipo en ADR `0004`.
- Sin NgRx ni librería de estado — signals + servicios `providedIn: 'root'` alcanzan para el volumen de este sistema ("pequeño, personalizado" — instrucción explícita del usuario).
- `features/ordenes-de-trabajo` e `features/inventario` quedaron como placeholders intencionales (sin servicio/modelo, solo página+ruta) en vez de scaffolding especulativo de CRUD completo, porque sus controllers en el backend son stubs vacíos sin contrato que seguir todavía — evita inventar una forma de datos que probablemente no coincida con lo que se implemente en Fase 1+.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Qué hacer con la iteración previa en `src/` (`.NET 10, multi-tenant`) — sigue sin resolver, y ahora también aplica a si existía algo reutilizable en su lado Angular (no se revisó en esta sesión).
- Modelo mono-óptica vs. multi-tenant.
- Los mismos puntos pendientes de la Ley 21.719 (ADR `0004`).
- El feature Clientes del frontend quedará desalineado si el DTO que exponga `ClientesController` en Fase 1 no coincide 1:1 con los campos actuales de la entidad de dominio (p. ej. si se agrega paginación) — revisar `Clientes` (servicio) y `Cliente` (modelo) al implementar los endpoints reales.

**Próximos pasos sugeridos:**
1. Aplicar `001_esquema_inicial.sql` contra `dbOPT_NET` y configurar `user-secrets` (sigue pendiente de sesiones anteriores).
2. Implementar los casos de uso de Clientes en el backend (Fase 1) siguiendo el patrón de `Auth/Commands/Login/`; ajustar `src/frontend/src/app/features/clientes/` si el contrato final difiere del modelo actual.
3. Al implementar Órdenes de Trabajo e Inventario en el backend, completar el patrón de `features/clientes/` en el frontend (hoy son placeholders).
4. Probar el flujo de login end-to-end (`ng serve` + `dotnet run`) una vez que la base de datos esté aplicada y haya al menos un usuario con clave hasheada.

---

## 2026-08-21 — Manual Técnico del Frontend + CLAUDE.md propio de `src/frontend/`

**Resumen:**
- Se preguntó al usuario el alcance antes de generar nada: (a) manual nuevo **solo Frontend**, independiente del `Manual_Tecnico_OPT.docx` existente (Backend/BD, que sigue vigente sin tocarlo); (b) los estándares de código para IA van en un **`src/frontend/CLAUDE.md` propio** (Claude Code lo carga automáticamente al trabajar en esa carpeta), no en el `README.md`.
- Se generó `docs/technical-manual/Manual_Tecnico_Frontend_OPT.docx` (nueva carpeta `docs/technical-manual/`, referenciada en el árbol de `AGENTS.md` pero hasta ahora vacía) con `python-docx`, replicando el estilo visual del manual de Backend/BD existente (se inspeccionaron sus estilos/colores/tablas antes de generar, para mantener consistencia: Heading 1/2/3 en Calibri `#1F3A5F`, tablas `Table Grid` con header blanco en negrita sobre fill `#1F3A5F`). 12 secciones: introducción, arquitectura general (standalone/zoneless/signals/sin NgRx), estructura de carpetas completa, módulo Auth (contrato exacto con el backend), módulo Clientes (parcial), placeholders de Órdenes de Trabajo/Inventario, seguridad (sessionStorage, PublicId, ProblemDetails), convenciones de código (resumen — remite a `CLAUDE.md` como fuente de verdad), comandos, gotchas, ADRs relacionados, próximos pasos.
- Se creó `src/frontend/CLAUDE.md`: guía prescriptiva para generación de código IA, distinta en propósito del `README.md` (que documenta cómo correr el proyecto). Contenido más relevante: checklist obligatorio antes de escribir un feature nuevo (leer el Controller del backend primero — no inventar contratos; si es stub, no construir CRUD; si no hay Controller, basarse en la entidad real de `OPT.Domain`), advertencia explícita sobre no nombrar servicios igual a APIs globales del navegador (documenta el caso real `Notification` → `Toast` de la sesión anterior), convenciones de componentes/servicios/testing con la receta exacta de providers para specs (`provideRouter([])`, `provideHttpClient()`+`provideHttpClientTesting()`) ya resuelta en el código existente.
- Se actualizaron las tablas de "Documentación de referencia" en `AGENTS.md` y `CLAUDE.md` (raíz) y el encabezado de `src/frontend/README.md` para enlazar ambos documentos nuevos.

**Decisiones tomadas:**
- Manual Técnico del Frontend como documento **independiente** en `docs/technical-manual/`, no una fusión con `src/documentos/Manual_Tecnico_OPT.docx` — decisión explícita del usuario vía pregunta directa, evita el riesgo de que un documento grande y regenerado quede desalineado con el otro.
- `src/frontend/CLAUDE.md` en vez de ampliar `README.md` — separa audiencia (agente IA generando código vs. desarrollador humano arrancando el proyecto) y aprovecha la carga automática jerárquica de `CLAUDE.md` de Claude Code.
- El manual técnico declara explícitamente que ante discrepancia con `src/frontend/CLAUDE.md`, este último es la fuente de verdad — el `.md` vive junto al código y se espera que se actualice con cada cambio de convención; el `.docx` es una fotografía más costosa de regenerar.

**Sin resolver / pendiente para el equipo (heredado):**
- Los mismos puntos de sesiones anteriores (Ley 21.719, catálogos `EstadoOT`/`FormaPago`/`CategoriaProducto`, mono-óptica vs. multi-tenant, qué hacer con la iteración previa en `src/`).

**Próximos pasos sugeridos:**
1. Mantener `src/frontend/CLAUDE.md` actualizado cada vez que se agregue o cambie una convención de código del frontend — es el documento que efectivamente va a guiar la generación de código futura, más que el `.docx`.
2. Regenerar `docs/technical-manual/Manual_Tecnico_Frontend_OPT.docx` cuando el módulo Clientes deje de ser parcial (backend implementado) o cuando Órdenes de Trabajo/Inventario dejen de ser placeholders.
3. Continuar con Fase 1: casos de uso de Clientes en el backend.

---

## 2026-08-21 — Catálogos EstadoOT/FormaPago/CategoriaProducto + plan de migración de datos legacy

**Resumen:**
- El usuario pidió iniciar la migración de datos real desde `db_a25cfd_opt2` (legacy) hacia `dbOPT_NET`, con string de conexión a `localhost` (Windows Auth). Antes de escribir nada se inspeccionó el esquema legacy completo (`old/BD/Create_Table.sql`, 32 tablas) y se perfiló la data real en vivo vía `sqlcmd`: **11.881 Clientes, 12.578 OrdenDeTrabajo, 13.183 RecetaCristales, 31.964 BitacoraOT, 4.677 Producto, 491 Empresa, 3.277 Abono, 1.261 Anamnesis, 13 Usuario**. Se confirmó que ambas bases (`db_a25cfd_opt2` y `dbOPT_NET`) están `ONLINE` en el mismo servidor.
- Se detectaron 3 bloqueos estructurales que impedían migrar directamente y se resolvieron con el usuario (`AskUserQuestion`):
  1. **Catálogos faltantes**: `OPT_EstadoOT`, `OPT_FormaPago`, `OPT_CategoriaProducto` no existían en el esquema nuevo pese a que `OrdenDeTrabajo.EstadoOTId`, `Abono.FormaPagoId` y `Producto.CategoriaId` son `NOT NULL` → decisión: **crearlas ahora** (no diferir).
  2. **Empresa sin RUT**: el legacy no tiene `Rut`/`RazonSocial`/`Giro`/`Email` (491 registros) pero el esquema nuevo los exige `NOT NULL` (`Rut` además `UNIQUE`) → decisión: **placeholder único por registro** (`Rut='SIN-RUT-{idEmpresa}'`, `RazonSocial`=nombre legacy).
  3. **RUT de Cliente con formato inconsistente**, probablemente personas duplicadas bajo distinto string de RUT (ej. `'0423117720'` / `'04231772-0'` / `'042317720'` para "IVAN PALACIO(S)") → decisión: **migrar tal cual, generar reporte de sospechosos** para revisión manual posterior (sin fusión automática).
  4. **Nombre/Apellido**: el legacy solo tiene un campo `Nombre` (Cliente y Usuario); el nuevo exige ambos `NOT NULL` → decisión: **split automático** (primera palabra → Nombre, resto → Apellido; sin resto, Apellido = Nombre).
- Se diseñó (`EnterPlanMode` + agente `Plan`) y el usuario aprobó un plan completo de migración con arquitectura de herramienta standalone: **nuevo proyecto de consola `OPT.Migracion`**, ADO.NET directo (`Microsoft.Data.SqlClient` + `Dapper`), **sin** pasar por `OPT.Domain`/`OPT.Application`/`OPT.Infrastructure` ni `AppDbContext`. Razón: `AuditableEntity.SetCreacion/SetModificacion` fuerzan `DateTimeOffset.UtcNow` (no hay forma de preservar fechas históricas del legacy vía la API pública de Domain), `Abono.Crear`/`BitacoraOT.Registrar` son `internal` con invariantes de negocio no garantizadas en data histórica, y `NumeroOT` no es asignable explícitamente vía `OrdenDeTrabajo.Crear` (se necesita preservar el `idOT` legacy como `NumeroOT`, el número visible en tickets al cliente). El plan completo (~18 fases, con derivación de `EstadoOTId`/`CreadoEn` desde `BitacoraOT`, mapeo de `Comuna` por nombre, parseo de `RecetaCristales`, etc.) queda guardado como referencia de diseño; su construcción se pospuso a pedido del usuario ("pausar acá por ahora").
- **Se ejecutó el Paso 1 del plan** (prerrequisito de esquema, con aprobación del usuario para seguir "todo de una vez" en ese paso):
  - Nuevas entidades `CatalogEntity`: `OPT.Domain/Entities/Comercial/EstadoOT.cs`, `FormaPago.cs`, `OPT.Domain/Entities/Inventario/CategoriaProducto.cs` — mismo patrón que `Rol`.
  - Nuevas `IEntityTypeConfiguration<T>`: `EstadoOTConfiguration`, `FormaPagoConfiguration`, `CategoriaProductoConfiguration` — seeds con los mismos ids/nombres del legacy (`EstadoOT`: 0-6; `FormaPago`: 0-4; `CategoriaProducto`: solo `1=General`, el legacy no tiene ese concepto).
  - `RolConfiguration.HasData()` extendido de 3 a 8 filas — se conservan los 6 roles reales del legacy (`Jefe Sucursal`, `Vendedor`, `Tecnico Medico`, `Control Calidad`, `Externo` + `Administrador` ya existente) como filas **distintas** de los 3 genéricos ya sembrados (`Administrador`/`Supervisor`/`Operador`), sin colapsarlos.
  - 3 FKs nuevas agregadas en las configs existentes: `FK_OrdenesDeTrabajo_EstadosOT`, `FK_Abonos_FormasPago`, `FK_Productos_CategoriasProducto`.
  - `AppDbContext` con los 3 `DbSet` nuevos.
  - **Gotcha real descubierto y resuelto**: `dotnet ef migrations script --idempotent` sin migration history previa en el proyecto (la migration `Initial` de `001` ya se había eliminado, como indica el procedimiento de `CLAUDE.md`) generó un script que recreaba **las 20 tablas desde cero** en vez de solo el diff — habría roto `dbOPT_NET` al reintentar crear tablas ya existentes. Se corrigió regenerando con una migration `Initial` temporal (archivos nuevos apartados con `mv` + ediciones revertidas temporalmente) que refleja exactamente el esquema ya aplicado, luego la migration real `AgregaCatalogos...` encima, y `dotnet ef migrations script Initial --idempotent` para obtener solo el diff real (188 líneas, no ~1300). **Este gotcha aplica a cualquier futuro `00N_*.sql`** — ver nota agregada abajo.
  - `002_catalogos.sql` generado y **aplicado exitosamente contra `dbOPT_NET`** (verificado vía `sqlcmd`: 3 tablas nuevas, 8 roles, 7 estados, 5 formas de pago, 1 categoría, 3 FKs activas, tablas transaccionales siguen en 0 filas). `dotnet build OPT.sln` limpio antes y después. Sin carpeta `Migrations/` residual (no se versiona).
  - Se actualizaron `CLAUDE.md` y `AGENTS.md` (estado actual, tabla de documentación, decisiones pendientes) y se generó `docs/technical-manual/Diccionario_Datos_OPT.docx` (diccionario de datos dedicado, 20 tablas, mismo estilo visual que `Manual_Tecnico_OPT.docx`).

**Decisiones tomadas:**
- Las 4 decisiones de la migración de datos listadas arriba (catálogos ahora, RUT placeholder en Empresa, RUT de Cliente as-is + reporte, split automático Nombre/Apellido).
- Arquitectura de `OPT.Migracion`: consola standalone con ADO.NET directo, sin pasar por Domain/Application/Infrastructure — ver razones arriba. Reutiliza `BCrypt.Net-Next` directamente (mismo work factor 12 que `PasswordService`) para el hashing de contraseñas, sin necesitar DI.
- `EstadoOT`/`FormaPago`/`CategoriaProducto` como `CatalogEntity` (sin auditoría, `ValueGeneratedNever`, ids fijos) — mismo patrón que `Rol`/`Region`/`Comuna`, consistente con ADR `0003`.
- **Gotcha de proceso descubierto y documentado**: cuando se agrega un `00N_*.sql` incremental y el proyecto no conserva ninguna migration de EF Core entre sesiones (por diseño, ver regla crítica 8 de `AGENTS.md`), `dotnet ef migrations add` **no tiene con qué diffear** y el `add` captura el modelo completo. Procedimiento correcto: (1) apartar temporalmente los archivos/ediciones nuevos, (2) `dotnet ef migrations add Initial` para fijar una migration base que refleje el esquema ya aplicado, (3) restaurar los archivos/ediciones nuevos, (4) `dotnet ef migrations add <NombreReal>`, (5) `dotnet ef migrations script Initial --idempotent --output ...` (con `Initial` como punto de partida explícito, no el comando sin argumentos), (6) `dotnet ef migrations remove` dos veces. Agregado a la tabla "Gotchas conocidos" de `CLAUDE.md` en esta misma sesión.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Construir el proyecto `OPT.Migracion` (Paso 2 del plan aprobado) y ejecutar `--dry-run` antes de tocar datos reales — el usuario pidió pausar acá.
- Los mismos puntos de sesiones anteriores: Ley 21.719 (puntos de ADR `0004`), mono-óptica vs. multi-tenant, qué hacer con la iteración previa en `src/`.

**Próximos pasos sugeridos:**
1. Retomar y construir `OPT.Migracion` según el plan aprobado (fases 00-18: preflight, catálogos en memoria, Sucursal/Empresa/Usuario, Cliente/Anamnesis/RecetaCristales, Producto, OrdenDeTrabajo/DetalleOT/Abono/BitacoraOT, restart de `SEQ_NumeroOT`, reporte final).
2. Correr `--dry-run` primero y revisar el reporte (duplicados de RUT, valores de receta no parseables, comunas sin match) antes de cualquier escritura real.
3. Tomar backup de `dbOPT_NET` antes de la corrida real (`BACKUP DATABASE ... TO DISK=...`) o confirmar explícitamente que es desechable.
4. Continuar con Fase 1: casos de uso de Clientes en el backend (independiente de la migración de datos).

---

## 2026-08-24 — Propuesta de branding, aplicación al frontend, y reorganización de documentos

**Resumen:**
- **Propuesta de branding**: a pedido del usuario ("como experto UX/UI"), se generó `src/documentos/Manual_Tecnico_UX_OPT.docx` (entonces `branding.docx`) con `python-docx` + `Pillow` (assets generados en el scratchpad, sin dependencias agregadas al repo): concepto de marca (isotipo = anillo/lente con glint), paleta con roles (`primary` #0F6B76 "Iris", `tertiary` #F2A93B "Ámbar Lente", neutros), tipografía, mapeo de color a los catálogos reales `OPT_EstadoOT` (7 estados) y `OPT_FormaPago` (5 medios), mockups de login/dashboard, y una sección de accesibilidad con ratios de contraste WCAG **calculados numéricamente** (no estimados) — el cálculo detectó y corrigió un error real del primer borrador (texto blanco sobre los chips "MONTAJE"/"LABORATORIO" no pasaba ni AA-large). Se creó `.agents/context/branding-ux-ui.md` como resumen accionable en texto para IA, siguiendo la convención de `.agents/context/` (no `docs/brand-guidelines.md`, que es la ruta que espera un skill de terceros instalado en `.agents/skills/ui-ux-pro-max/brand/` pero que el usuario decidió no usar, prefiriendo la convención propia del proyecto).
- **Branding aplicado al frontend real** (`src/frontend/`): tema Angular Material 3 regenerado con la herramienta oficial `ng generate @angular/material:theme-color --primary-color="#0F6B76" --tertiary-color="#F2A93B"` (no a mano — deja que la ciencia de color HCT de Material derive los 13 tonos por rol) → `src/frontend/src/theme-colors.scss` (nuevo), enchufado en `src/frontend/src/styles.scss` reemplazando `mat.$azure-palette`/`mat.$blue-palette`. Como el código existente ya usaba tokens de sistema (`var(--mat-sys-*)`, `color="primary"`) sin ningún hex hardcodeado, el cambio se propaga solo a toda la app. Además: favicon reemplazado por el isotipo de marca (`public/favicon.ico`, 16/32/48px, generado con Pillow) y título de pestaña (`OptFrontend` → `OPT · Gestión de Ópticas`). Verificado: `npm run build`/`lint`/`test` en verde (14/14 tests), y captura de pantalla del login vía Chrome headless + verificación de `--mat-sys-primary`/`--mat-sys-tertiary` en el CSS compilado contra los valores exactos de `theme-colors.scss`. Deliberadamente **no** se tocó el layout del login (el mockup split-panel de la propuesta) — es cambio de feature, no de tema, y el `CLAUDE.md` del frontend marca las pantallas reales para cuando el backend tenga los endpoints.
- **Reorganización de documentos**: a pedido del usuario, se consolidaron todos los documentos del proyecto en `src/documentos/` (antes repartidos entre `docs/architecture/`, `docs/technical-manual/` y `docs/` raíz) y se eliminó la carpeta `docs/` por completo. Se renombraron los dos manuales sin el patrón `Manual_Tecnico_<Area>_OPT.docx`: `Manual_Tecnico_OPT.docx` → `Manual_Tecnico_Backend_OPT.docx`, `branding.docx` → `Manual_Tecnico_UX_OPT.docx`. `docs/architecture/README.md` se movió a `src/documentos/README.md` y se reescribió como índice general de los 5 documentos de la carpeta (antes solo indexaba la propuesta de arquitectura). Se actualizaron todas las referencias cruzadas vivas en `CLAUDE.md`, `AGENTS.md`, `src/AGENTS.md`, `src/frontend/CLAUDE.md`, `src/frontend/README.md`, `.agents/decisions/0002-*.md`, `.agents/decisions/0003-*.md`, `.agents/decisions/README.md`, `.agents/context/branding-ux-ui.md` y `.agents/context/README.md` (a este último también se le agregó el ítem `branding-ux-ui.md` en su tabla de contenido, que había quedado sin listar desde la sesión anterior). Las entradas históricas de este mismo archivo (`progress.md`) **no se reescribieron** — describen rutas válidas en su momento; este archivo es un log cronológico, no vive actualizado.

**Decisiones tomadas:**
- Contexto de marca para IA vive en `.agents/context/branding-ux-ui.md` (convención propia del proyecto), no en `docs/brand-guidelines.md` (convención del skill de terceros `ui-ux-pro-max`) — confirmado explícitamente con el usuario vía `AskUserQuestion`.
- Los 4 manuales técnicos (Backend, Frontend, Diccionario de Datos, UX) y la propuesta de arquitectura viven, sin excepción, en `src/documentos/` — no vuelve a existir una carpeta `docs/` en la raíz.
- Convención de nombre confirmada: `Manual_Tecnico_<Area>_OPT.docx` para los manuales por área; el diccionario de datos y la propuesta de arquitectura quedan como excepciones deliberadas (son otro tipo de documento).

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), mono-óptica vs. multi-tenant, `OPT.Migracion`, qué hacer con la iteración previa en `src/`.
- Validación de la propuesta de branding con stakeholders reales (fase 6 de su hoja de ruta) — sigue siendo v1.0 no confirmada.
- Vectores finales del logotipo (SVG) — hoy solo existe como descripción + mockups PNG dentro de `Manual_Tecnico_UX_OPT.docx` y como favicon rasterizado.
- Rediseño del layout de login/pantallas reales según los mockups de la propuesta — pendiente de que el backend tenga los endpoints correspondientes.

**Próximos pasos sugeridos:**
1. Validar la propuesta de branding con stakeholders (dueños de óptica, equipo de atención) antes de darla por definitiva.
2. Si se aprueba, encargar/generar los vectores SVG finales del logotipo.
3. Retomar Fase 1 de negocio (Clientes) o `OPT.Migracion`, según prioridad del equipo — sin relación con el trabajo de esta sesión.

---

## 2026-08-24 — `OPT.Migracion`: construcción y ejecución real para entidades simples/bajo riesgo

**Resumen:**
- El usuario pidió retomar y ejecutar la migración de datos legacy → `dbOPT_NET` para las entidades "simples y de bajo riesgo": Región/Comuna, Sucursal, Empresa, Rol, Usuario (dejando fuera deliberadamente Cliente/Anamnesis/RecetaCristales/OrdenDeTrabajo/Producto, de mayor riesgo). Se construyó el proyecto de consola `OPT.Migracion` (`src/backend/OPT.Migracion/`, agregado a `OPT.sln`) siguiendo la arquitectura ya aprobada en la sesión 2026-08-21: ADO.NET directo vía `Microsoft.Data.SqlClient` + `Dapper`, sin dependencia de `OPT.Domain`/`OPT.Application`/`OPT.Infrastructure`, reutilizando `BCrypt.Net-Next` (work factor 12) directamente para hashear las contraseñas legacy en texto plano.
- **Perfilado de datos en vivo antes de escribir código** (vía `sqlcmd`) que reveló 3 hallazgos no anticipados por el plan original:
  1. **Región/Comuna**: el legacy usa sus propios ids (`idRegion` 0-15 con un centinela `0="SIN REGION"`; `idComuna` con códigos INE de 5 dígitos + centinela `0="SIN COMUNA"`), distintos de los ids secuenciales del catálogo ya sembrado en `dbOPT_NET`. Como **ninguna** de las 5 entidades en alcance (Sucursal/Empresa/Usuario/UsuarioSucursal/EmpresaSucursal) referencia Región/Comuna, se determinó que no hace falta migrar filas — solo se agregó un verificador de cobertura por nombre (`CatalogoVerificador`, solo lectura) que confirmó 13/15 regiones y 343/346 comunas cubiertas (las faltantes son variantes ortográficas menores: "Gral." vs "General", "Aisén" vs "Aisén" — sin impacto).
  2. **Rol**: el `idRol` legacy (1-6) **no coincide** con el `RolId` nuevo de las mismas filas (quedaron como 4-8 tras la sesión 2026-08-21, tras los 3 roles genéricos 1-3) — se construyó `RolMapper` para mapear por nombre exacto en vez de asumir igualdad de id.
  3. **Sucursal Matriz**: el legacy tiene 2 filas con `Matriz=1` (`Casa Matriz` y `AGENCIA DE ADUANAS CARLOS ROSSI`), pero el esquema nuevo exige una única matriz activa (índice único filtrado) — bloqueaba el insert. Se preguntó al usuario (`AskUserQuestion`): decidió que solo `Casa Matriz` (491 empresas / 12 usuarios vinculados) conserva `EsMatriz=true`; la otra (1 empresa / 0 usuarios vinculados, dato de carga erróneo) se fuerza a `false`.
- **Auditoría (`CreadoPor`)**: como el legacy no registra quién creó cada fila y `Sucursal`/`Empresa`/`Usuario` exigen `CreadoPor int NOT NULL`, se preguntó al usuario cómo resolverlo. Eligió usar el primer Usuario Administrador migrado como dueño de auditoría de todo el lote (en vez de un valor centinela `0`). Se implementó como: insertar primero al admin "bootstrap" (Pablo Brito Luck, RUT `13713218-4` — el admin con `FechaIngreso` real más antigua, excluyendo una cuenta con fecha centinela `1900-01-01`) con `CreadoPor` temporal, luego un `UPDATE` que autoreferencia su propio `Id` nuevo; el resto de Usuario/Sucursal/Empresa usa ese mismo `Id` como `CreadoPor`. Se verificó primero que `CreadoPor` no tiene FK real a nivel de base de datos en ninguna `IEntityTypeConfiguration<T>` (es un `int` simple), por lo que el valor temporal antes del `UPDATE` no viola ninguna constraint.
- **Fechas históricas preservadas**: `CreadoEn` usa `FechaRegistro`/`FechaIngreso` del legacy (no `DateTimeOffset.UtcNow`) para Sucursal y Usuario — es la razón original por la que el plan de 2026-08-21 descartó pasar por `AuditableEntity.SetCreacion`. Empresa no tiene fecha en el legacy, así que usa la fecha de ejecución de la migración.
- **Otras normalizaciones aplicadas** (replican exactamente `Usuario.Crear()` de `OPT.Domain` para no generar datos que un alta manual por la API no pudiera producir): RUT `Trim().ToUpperInvariant()`, Email `Trim().ToLowerInvariant()`, Nombre completo dividido en Nombre/Apellido (primera palabra / resto, sin resto → Apellido = Nombre — regla ya aprobada en 2026-08-21). Empresa usa el RUT placeholder `SIN-RUT-{idEmpresa}` ya aprobado (se verificó que el `idEmpresa` legacy máximo es 5484, por lo que el placeholder siempre cabe en `Rut nvarchar(12)`).
- **Ejecución**: se corrió primero en modo dry-run (por defecto, sin flags) contra los datos reales — reportó exactamente los números y decisiones esperadas. Con aprobación explícita del usuario se corrió `-- --execute`, todo dentro de una única transacción SQL (rollback automático ante cualquier error). Resultado verificado post-commit vía `sqlcmd`: 13 Usuario, 3 Sucursal (1 sola con `EsMatriz=1`), 491 Empresa (sin RUTs duplicados), 19 UsuarioSucursal, 883 EmpresaSucursal — coincide exactamente con lo migrado y con el mapeo de roles esperado.
- La herramienta incluye una guardia de re-ejecución (aborta si las tablas destino ya tienen filas, salvo `--force`) para no duplicar datos ante un reintento accidental.

**Decisiones tomadas:**
- Solo `Casa Matriz` (idSucursal=1 del legacy) migra con `EsMatriz=true`; la otra sucursal con `Matriz=1` en el legacy se trata como error de carga y migra en `false` — decisión del usuario vía `AskUserQuestion`.
- `CreadoPor` de todas las filas migradas (Sucursal/Empresa/Usuario) apunta al primer Usuario Administrador migrado (autoreferenciado), no a un valor centinela — decisión del usuario vía `AskUserQuestion`.
- Región/Comuna/Rol no requieren `INSERT` — ya están sembrados; Rol se mapea por nombre (no por id, que no coincide entre legacy y nuevo).
- `OPT.Migracion` por defecto corre en dry-run; requiere `--execute` explícito para escribir, y `--force` explícito para re-ejecutar sobre tablas no vacías.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Migrar las entidades de mayor riesgo (Cliente, Anamnesis, RecetaCristales, OrdenDeTrabajo/DetalleOT/Abono/BitacoraOT, Producto/ProductoSucursal) — sigue pendiente el plan de ~18 fases de la sesión 2026-08-21 (RUTs de Cliente con formato inconsistente, mapeo de Comuna por nombre ya que los ids no coinciden entre legacy y nuevo — mismo hallazgo que esta sesión con Región/Comuna, derivación de `EstadoOTId` desde `BitacoraOT`, parseo de `RecetaCristales`, preservación de `NumeroOT`).
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), mono-óptica vs. multi-tenant, qué hacer con la iteración previa en `src/`.
- Validación de la propuesta de branding con stakeholders (heredado, sin relación con esta sesión).

**Próximos pasos sugeridos:**
1. Migrar Producto/ProductoSucursal (bajo-medio riesgo, sin datos personales) como siguiente paso natural antes de Cliente/OT.
2. Migrar Cliente/Anamnesis/RecetaCristales (datos sensibles Ley 21.719) — requiere resolver el mapeo Comuna por nombre (mismo patrón que Región/Comuna en esta sesión) y el reporte de RUTs sospechosos ya acordado en 2026-08-21.
3. Migrar OrdenDeTrabajo/DetalleOT/Abono/BitacoraOT — el paso más complejo (derivación de estado, preservación de `NumeroOT`, reinicio de `SEQ_NumeroOT` tras la carga).
4. Continuar Fase 1 de negocio (casos de uso de Clientes en el backend) en paralelo, si el equipo lo prioriza.

---

## 2026-08-24 — CRUD/API de Sucursal, Empresa, Usuario y Rol

**Resumen:**
- El usuario pidió "migrar en el backend" Sucursal/Empresa/Rol/Usuario; se aclaró vía `AskUserQuestion` que los datos de estas 5 entidades ya estaban migrados (sesión anterior del mismo día) y que lo pedido era construir el CRUD/API real, inexistente hasta ahora (solo `AuthController` y 3 controllers stub vacíos). Se diseñó el plan en modo plan (3 agentes Explore en paralelo sobre Domain/Application/Infrastructure+API) y se ejecutó tras aprobación.
- **Dominio** (`OPT.Domain/Entities/Organizacion/Usuario.cs`): se agregaron los mutadores `Actualizar(rut, nombre, apellido, email, rolId, usuarioId)`, `AsignarSucursal(sucursalId)` y `QuitarSucursal(sucursalId)` (gestionan la colección privada `_sucursales`) — no existían. `Sucursal` y `Empresa` no requirieron cambios (sus `Crear`/`Actualizar` ya alcanzaban).
- **Repositorios nuevos**: `ISucursalRepositorio`/`SucursalRepositorio` (sin miembros propios — el genérico `IRepositorioBase<T>` alcanza), `IEmpresaRepositorio`/`EmpresaRepositorio` (+ `ObtenerPorPublicIdAsync`, `ExisteRutAsync`), `IRolRepositorio`/`RolRepositorio` (interfaz propia sin heredar de `IRepositorioBase<T>` — `Rol` es `CatalogEntity`, no `AuditableEntity`, no encaja en el genérico). `IUsuarioRepositorio` (ya existía) sumó `ObtenerPorPublicIdAsync` (con `.Include(Sucursales)` + `.Include(Rol)`) y `UsuarioRepositorio` sobrescribió `ObtenerTodosAsync` para incluir `Rol` (necesario para `RolNombre` en los DTOs de listado). Los 3 repos nuevos registrados en `OPT.Infrastructure/DependencyInjection.cs`.
- **Application** (`Features/Sucursales|Empresas|Usuarios|Roles/`): CQRS completo siguiendo el patrón de `Auth/Commands/Login/` (Command/Query + Handler + Validator por verbo, DTO compartido por entidad). Sucursal y Empresa: Crear/Actualizar/Eliminar (soft delete) + ObtenerPorId/ObtenerTodos — Sucursal por `Id` interno (no está en la lista de ADR 0004), Empresa por `PublicId` (Guid, obligatorio por ADR 0004). Usuario: además de Crear/Actualizar/Eliminar/ObtenerPorId/ObtenerTodos, se agregó `CambiarClave` (verifica clave actual, rehashea con `IPasswordService`), `Activar`/`Desactivar` (ya existían como mutadores de dominio) y `AsignarSucursal`/`QuitarSucursal` — estas últimas se agregaron porque sin al menos una sucursal asignada un usuario nuevo no puede iniciar sesión (`LoginCommandHandler` lo exige). Rol: solo `ObtenerTodos` (catálogo sembrado de 8 filas, sin mutadores en el dominio).
- **Fuera de alcance deliberado**: gestión de `EmpresaSucursal` (a diferencia de `UsuarioSucursal`, no bloquea ninguna funcionalidad ya implementada) y paginación de `ObtenerEmpresasQuery`/`ObtenerUsuariosQuery` (491 empresas es manejable sin paginar por ahora).
- **Controllers nuevos** (`OPT.API/Controllers/`): `SucursalesController`, `EmpresasController`, `UsuariosController`, `RolesController` — todos `[Authorize]`, patrón delgado sin try/catch idéntico a `AuthController`.
- **Verificación end-to-end real**: se inicializó `user-secrets` en `OPT.API` (no estaba configurado — pendiente conocido) con la cadena de conexión a `dbOPT_NET` (Windows Auth) y una clave JWT de desarrollo generada localmente (no committeada). Se detectó y corrigió un problema de arranque: `dotnet run` sin `ASPNETCORE_ENVIRONMENT=Development` corre en `Production` por defecto, donde `AddUserSecrets` no se activa — la API arrancaba pero fallaba en cualquier request con `IDX10703` (clave JWT de longitud cero) porque leía `Jwt:Key=""` de `appsettings.json` en vez del secreto. Con `ASPNETCORE_ENVIRONMENT=Development` explícito, la API levantó correcto. Se generó un hash bcrypt de prueba (mini proyecto descartable en el scratchpad, usando el mismo `BCrypt.Net-Next` work factor 12 que `PasswordService`) para poder autenticarse contra un usuario real ya migrado (no se conocen las claves originales en texto plano de los 13 usuarios migrados) — se capturó el hash original de `UsuarioId=1` antes de sobrescribirlo temporalmente, y se restauró exactamente al terminar (verificado por `sqlcmd` que el hash quedó idéntico al capturado). Con ese login se probaron contra datos reales: `GET /api/roles` (8), `GET /api/sucursales` (3), `GET /api/empresas` (491), `GET /api/usuarios` (13, con `rolNombre` resuelto correctamente) — más un ciclo completo `POST`→`GET`→`PUT`→`DELETE` sobre Sucursal confirmando el soft delete (la fila no reaparece en la lista tras `DELETE`), validaciones de negocio (`POST /api/sucursales` con segunda Matriz → 400; `POST /api/empresas` con RUT duplicado → 400), y el flujo completo de Usuario (`POST` crear → `POST .../sucursales/{id}` asignar → `POST .../desactivar` → `GET` refleja `activo:false`). Se limpiaron los datos de prueba creados (soft delete de la sucursal y el usuario de prueba) al terminar; los conteos de filas activas quedaron exactamente en la línea base (13/3/491).

**Decisiones tomadas:**
- Alcance de "migrar el backend" confirmado con el usuario: CRUD/API real, no re-verificación de la migración de datos (ya hecha).
- `AsignarSucursal`/`QuitarSucursal` de Usuario incluidos en este alcance por ser un requisito funcional directo (login los exige); `EmpresaSucursal` queda fuera por no bloquear nada ya implementado.
- Rol expuesto solo de lectura — el dominio no tiene factory ni mutadores para crearlo/editarlo vía API, es catálogo sembrado fijo.
- Los handlers de mutación no llaman `repo.Actualizar(entidad)` explícitamente tras mutar — se apoyan en el change tracking de EF Core sobre la entidad ya cargada (mismo patrón que `LoginCommandHandler`), solo `Crear` llama `repo.Agregar(...)`.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Gestión de `EmpresaSucursal` (asignar/quitar sucursales a una empresa) — seguimiento sugerido, no bloqueante.
- Frontend para estas 4 entidades — paso 2 del pedido original del usuario, sesión separada.
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), Cliente/Anamnesis/RecetaCristales/OrdenDeTrabajo/Producto pendientes de migración de datos, qué hacer con la iteración previa en `src/`.

**Próximos pasos sugeridos:**
1. Frontend (Angular) para Sucursales/Empresas/Usuarios/Roles contra los endpoints nuevos — pedido explícito del usuario como paso 2, pendiente de sesión propia.
2. Si se prioriza, agregar gestión de `EmpresaSucursal` siguiendo el mismo patrón ya usado para `UsuarioSucursal` (`AsignarSucursal`/`QuitarSucursal`).
3. Continuar con Producto/ProductoSucursal o Cliente/Anamnesis/RecetaCristales en `OPT.Migracion`, según prioridad del equipo (independiente de este trabajo).

---

## 2026-08-25 — API de solo lectura para Región/Comuna

**Resumen:**
- El usuario pidió "generar módulos" de Región/Comuna, Sucursal, Empresa, Rol y Usuario. Antes de escribir código se verificó el estado real (no solo el `.md`) con un agente Explore sobre las 4 capas de los 6 conceptos: **Sucursal, Empresa, Rol y Usuario ya tenían CRUD/API completo** (sesión 2026-08-24, sin stubs ni TODOs); **Región y Comuna eran el único hueco real** — solo existían como `CatalogEntity` + `IEntityTypeConfiguration` + seed, sin Application ni Controller. Se confirmó el alcance con el usuario vía `AskUserQuestion`: solo construir Región/Comuna, dejar los otros 4 módulos intactos.
- Se revisó el legacy (`OPT_RegionDAL.Lista()`, `OPT_ComunaDAL.Lista(idRegion)`) para confirmar el comportamiento a preservar: listado completo de regiones y listado de comunas **filtrado por región** (uso típico: combo cascada en formularios de dirección) — sin mutadores en ningún caso.
- Implementación siguiendo exactamente el patrón ya usado por `Rol` (mismo tipo de entidad `CatalogEntity`, no encaja en `IRepositorioBase<T>` genérico):
  - `OPT.Domain/Interfaces/Repositories/IRegionRepositorio.cs` (`ObtenerTodosAsync`) e `IComunaRepositorio.cs` (`ObtenerPorRegionAsync(regionId)`).
  - `OPT.Infrastructure/Persistence/Repositories/RegionRepositorio.cs`/`ComunaRepositorio.cs`, registrados en `DependencyInjection.cs`.
  - `OPT.Application/Features/Regiones/` (`RegionDto`, `Queries/ObtenerTodos`) y `Features/Comunas/` (`ComunaDto`, `Queries/ObtenerPorRegion`) — sin Commands, catálogos sembrados de solo lectura.
  - `OPT.API/Controllers/RegionesController.cs` (`GET /api/regiones`) y `ComunasController.cs` (`GET /api/comunas?regionId={id}`), `[Authorize]`, mismo estilo delgado sin try/catch.
  - Usan `Id` interno en la API, no `PublicId` — no están en el alcance de datos personales/sensibles del ADR `0004`.
- **Verificación**: `dotnet build OPT.sln` limpio. Se evitó mintear un JWT firmado con la clave de desarrollo para probar los endpoints vía HTTP (el clasificador de permisos de la sesión lo bloqueó por ser una acción sensible de seguridad, incluso en contexto de prueba — decisión correcta, no se intentó eludir). En su lugar se verificó la lógica real contra `dbOPT_NET` con un proyecto de consola descartable en el scratchpad que reutiliza `AppDbContextFactory` (la misma factory que ya usan las EF Core Tools) para ejecutar las mismas consultas LINQ directamente, sin pasar por HTTP/auth: confirmó 16 regiones (orden alfabético correcto) y 52 comunas para "Región Metropolitana de Santiago" (Id=13), y 0 comunas para un `RegionId` inexistente.
- Se actualizó `CLAUDE.md` raíz (estado actual + árbol de `OPT.Application`/`OPT.API`).

**Decisiones tomadas:**
- Alcance confirmado con el usuario: solo Región/Comuna en esta sesión — no auditoría ni cambios en Sucursal/Empresa/Rol/Usuario, no gestión de `EmpresaSucursal` (quedan como próximos pasos opcionales, sin tomarlos ahora).
- `GET /api/comunas` filtra por `regionId` como query param **requerido** (sin validador de FluentValidation — el binding no-nullable de ASP.NET Core ya devuelve 400 si falta, mismo criterio que `ObtenerSucursalPorIdQuery`/`ObtenerEmpresaPorPublicIdQuery`, que tampoco llevan validador para su id de ruta). Un `regionId` que no corresponde a ninguna región devuelve lista vacía, no error — replica el comportamiento del legacy (sin validación de existencia) y evita un round-trip extra a la base de datos.
- No se agregó `IComunaRepositorio.ExisteAsync`/lógica de validación de FK pensando en el futuro módulo de Clientes — se dejó fuera por ser trabajo especulativo fuera del alcance pedido (YAGNI); se retomará cuando se implemente esa validación real.

**Sin resolver / pendiente para el equipo (heredado):**
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), Cliente/Anamnesis/RecetaCristales/OrdenDeTrabajo/Producto pendientes de migración de datos, gestión de `EmpresaSucursal`, frontend para Sucursales/Empresas/Usuarios/Roles, qué hacer con la iteración previa en `src/`.

**Próximos pasos sugeridos:**
1. Frontend (Angular) para Región/Comuna como combo cascada, reutilizable en el futuro formulario de dirección de `Cliente` (Fase 1).
2. Cuando se implemente Cliente, usar `ComunaId` como FK real y validar su existencia contra `IComunaRepositorio` (agregar `ExisteAsync` en ese momento, no antes).
3. Continuar con Fase 1 de negocio (Clientes) o con `OPT.Migracion` (Producto/Cliente), según prioridad del equipo.

---

## 2026-08-25 — Formularios de Frontend para Sucursales/Empresas/Roles/Usuarios

**Resumen:**
- El usuario pidió crear formularios de frontend para las APIs ya disponibles: Región/Comuna, Sucursales, Empresas, Roles, Usuarios. Se confirmó el alcance con `AskUserQuestion` antes de escribir código: (a) Usuario incluye el 100% de la API (CRUD + cambiar clave + activar/desactivar + asignar/quitar sucursal), no solo CRUD básico; (b) Región/Comuna queda **fuera** de esta sesión — es de solo lectura y hoy ningún formulario lo necesita como FK (ni Empresa ni Sucursal tienen dirección estructurada con Comuna todavía), se retoma cuando el módulo Cliente lo necesite.
- Se releyeron los contratos reales del backend (Commands/Queries/DTOs/Validators de `Sucursales`, `Empresas`, `Roles`, `Usuarios` en `OPT.Application`) antes de escribir el modelo TypeScript de cada uno — siguiendo la regla ya existente de `src/frontend/CLAUDE.md` de nunca inventar un contrato. Hallazgo relevante: `UsuarioDto` solo expone `sucursalActivaId` (no la lista completa de sucursales asignadas) y esa misma propiedad solo la fija el login (`Usuario.SetSucursalActiva` no está expuesto por ningún Command/Controller) — el diálogo de asignar/quitar sucursal se construyó reconociendo esa limitación real de la API en vez de inventar un endpoint que no existe.
- **Patrón nuevo establecido**: CRUD simple = página de listado (única ruta del feature) + diálogo `MatDialog` de alta/edición, en vez de rutas `/nuevo`/`/editar/:id` — más liviano para formularios chicos y ahora documentado en `src/frontend/CLAUDE.md` como convención a seguir. Se creó `shared/components/confirm-dialog/` (diálogo de confirmación genérico) reutilizado por las 3 eliminaciones y por activar/desactivar Usuario, en vez de un diálogo de confirmación por feature.
- **Implementado** (todo generado con `ng generate` para heredar convención de nombres, luego completado a mano):
  - `features/sucursales/` — CRUD completo (`Id` interno en la API, `EsMatriz` solo editable al crear — el formulario lo oculta en modo edición).
  - `features/empresas/` — CRUD completo (`PublicId`, mismo shape de campos para crear/actualizar).
  - `features/roles/` — solo lista de solo lectura, sin diálogo de alta/edición (coincide con el backend, sin mutadores).
  - `features/usuarios/` — CRUD + 3 diálogos adicionales: `usuario-clave-dialog` (cambiar clave — se documentó explícitamente en el diálogo que `CambiarClaveUsuarioCommand` verifica `claveActual` contra el hash del usuario objetivo, no del administrador, para no inducir a error de uso), `usuario-sucursal-dialog` (asignar/quitar, sin poder mostrar el estado actual por la limitación de API ya mencionada), y activar/desactivar como acción directa con confirmación.
  - Rutas nuevas registradas en `app.routes.ts` (`/sucursales`, `/empresas`, `/usuarios`, `/roles`) y agregadas al `itemsNav` de `Shell`.
- **Verificación**: `npm run build`/`lint`/`test` en verde (28/28 tests, antes 14/14 — se completaron los `.spec.ts` generados por el CLI con los providers de `MatDialogRef`/`MAT_DIALOG_DATA`/`HttpClient` necesarios, siguiendo la tabla de recetas ya existente en `src/frontend/CLAUDE.md`, ahora ampliada con la fila de diálogos). No se pudo verificar visualmente en navegador (sin `chromium-cli`/Playwright en el entorno, mismo límite que la sesión de branding); se confía en que Angular compila las plantillas con chequeo de tipos estricto (detecta bindings inválidos, imports de componente faltantes, etc. en `ng build`).
- Se actualizaron `CLAUDE.md` raíz, `src/frontend/README.md` (árbol de `features/`, límite de la API de Usuario, patrón de diálogo) y `src/frontend/CLAUDE.md` (sección nueva "CRUD simple: lista + diálogo", fila de testing para diálogos).

**Decisiones tomadas:**
- Las 2 decisiones de alcance confirmadas por el usuario arriba (Usuario completo, Región/Comuna fuera).
- Alta/edición vía `MatDialog` en vez de rutas propias — decisión de diseño tomada sin preguntar (no había patrón previo en el repo para esto), documentada como convención para que el próximo CRUD simple no reinvente el enfoque.
- No se agregó búsqueda/filtro client-side en la lista de Empresas (491 filas sin paginar en el backend) — fuera del alcance pedido ("formularios", no "mejoras de listado"); posible mejora futura de bajo costo con `MatTableDataSource`.
- El diálogo de cambiar clave se dejó disponible desde la lista de administración de Usuarios tal como lo permite la API, pero con una nota visible de que requiere la clave actual del usuario objetivo — no se intentó ocultar ni maquillar esa limitación real del backend.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Verificación visual en navegador real de los formularios nuevos (bloqueada por falta de herramienta de navegador headless en el entorno).
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), Cliente/Anamnesis/RecetaCristales/OrdenDeTrabajo/Producto pendientes de migración de datos, gestión de `EmpresaSucursal`, auditoría de los 4 módulos de backend existentes (declinada por el usuario en la sesión anterior), Región/Comuna en el frontend (pendiente hasta que Cliente lo necesite), qué hacer con la iteración previa en `src/`.

**Próximos pasos sugeridos:**
1. Si se prioriza, agregar búsqueda/filtro client-side a la lista de Empresas.
2. Cuando se implemente el módulo Cliente (Fase 1), construir el selector cascada de Región/Comuna reutilizando `features/roles` (solo lectura) como plantilla más cercana.
3. Validar visualmente en un navegador real cuando haya herramienta disponible o el usuario lo haga manualmente con `dotnet run` + `ng serve`.

---

## 2026-08-25 — Refresco de documentación IA y manuales técnicos

**Resumen:**
- El usuario pidió actualizar los `.md` de contexto para IA y los manuales técnicos (`.docx`) para que reflejen el trabajo de las últimas sesiones (catálogos `EstadoOT`/`FormaPago`/`CategoriaProducto` de 2026-08-21, CRUD de Organización de 2026-08-24, Región/Comuna + formularios de frontend de 2026-08-25). Antes de escribir, se auditaron todos los `.md` de IA (`AGENTS.md` raíz, `src/AGENTS.md`, `.agents/decisions/README.md`, `.agents/context/README.md`) y ambos manuales técnicos (`Manual_Tecnico_Backend_OPT.docx`, `Manual_Tecnico_Frontend_OPT.docx`) contra el estado real del código — no solo se agregó lo nuevo, se encontraron y corrigieron varias inconsistencias **preexistentes**, de sesiones anteriores:
  - `.agents/decisions/README.md`: el índice de ADRs listaba `0001` como "Propuesta", pero el ADR ya dice "Aceptada" en su propio archivo desde el 2026-08-19 (el scaffold lo implementa) — desincronización de una sesión previa, corregida.
  - `Manual_Tecnico_Backend_OPT.docx`: la sección 7.6 "Deuda técnica conocida" seguía afirmando que `EstadoOTId`/`FormaPagoId`/`CategoriaId` no tenían FK real — quedó resuelto en `002_catalogos.sql` (2026-08-21) pero nunca se actualizó esa sección ni las 3 celdas de columna correspondientes en 7.7 (`OrdenDeTrabajo.EstadoOTId`, `Abono.FormaPagoId`, `Producto.CategoriaId` seguían diciendo "sin FK real"). También faltaban las 3 tablas nuevas de `002_catalogos.sql` (`OPT_EstadoOT`, `OPT_FormaPago`, `OPT_CategoriaProducto`) completas en la sección 7.7, la sección seguía hablando de "17 tablas" en dos lugares, y la descripción de seed de `OPT_Rol` seguía diciendo 3 roles en vez de 8. Se verificó explícitamente que `OPT_BitacoraOT.EstadoAnteriorId/EstadoNuevoId` **sí** sigue sin FK real (no estaba en el alcance de esa migración) antes de tocar esa celda — no se sobre-corrigió.
  - `Manual_Tecnico_Frontend_OPT.docx`: todavía decía que el backend de Organización "no lo consume" el frontend (frase ya falsa tras esta sesión), referenciaba el manual de backend por su nombre viejo (`Manual_Tecnico_OPT.docx`, renombrado a `Manual_Tecnico_Backend_OPT.docx` en la sesión 2026-08-24) en 2 lugares, y documentaba `apiUrl` apuntando al puerto `7000` (ya corregido en código a `63595` en una sesión anterior de este mismo día, pero no en el manual). No tenía ninguna sección sobre el tema de marca/branding aplicado (2026-08-24).
  - `CLAUDE.md` raíz: quedaba una referencia más al puerto `7000` sin corregir (`src/frontend/README.md` ya se había corregido en una sesión previa).
- **Manuales técnicos actualizados con python-docx** (ya instalado de sesiones previas), con un enfoque de **edición quirúrgica vía XML** en vez de regeneración completa desde cero: se localizan párrafos/tablas por texto exacto (nunca por índice numérico, para no depender de la posición) y se inserta/edita solo lo necesario, clonando tablas existentes (`Columna|Tipo|Notas`) para preservar el formato exacto (fondo azul `#4472A8`, texto blanco en negrita del header) sin reconstruirlo a mano. Cada script se probó primero contra una copia en el scratchpad y se verificó el resultado completo (estructura, texto, formato de celdas) antes de aplicarlo al archivo real — se encontraron y corrigieron 2 bugs del propio script de edición durante esa verificación (texto duplicado en un párrafo con múltiples runs; texto duplicado en celdas de encabezado clonadas por escribir en el run vacío en vez del run con el texto real).
  - Backend: agregadas las tablas `OPT_EstadoOT`/`OPT_FormaPago` (Módulo Comercial) y `OPT_CategoriaProducto` (Módulo Inventario) a la sección 7.7; corregidas las 3 celdas de FK y el texto de 7.6; corregido "17→20 tablas" (2 lugares) y el seed de `OPT_Rol`; agregada la subsección "Región y Comuna (2026-08-25)" en 4.3; agregadas 2 filas a la tabla de Controllers (6.2); actualizado el texto de DI (5.3).
  - Frontend: agregado el capítulo completo "7. Módulo Organización — Sucursales, Empresas, Roles y Usuarios (implementado)" (4 subsecciones: patrón lista+diálogo, Sucursales/Empresas, Roles, Usuarios) — lo que obligó a renumerar los capítulos 7-12 a 8-13 (con sus subsecciones); agregada la sección "2.5 Tema visual y branding"; corregidas las 2 referencias al nombre viejo del manual de backend y el puerto de `apiUrl`; agregadas 4 filas a la tabla de `features/`; actualizado el diagrama de rutas de 2.3; reescrito el bullet ya obsoleto de "Próximos Pasos" (13, antes 12) sobre Organización, agregando los pendientes reales (Región/Comuna sin consumidor, `EmpresaSucursal`, búsqueda en Empresas).
- **`.md` de IA actualizados**: `AGENTS.md` raíz (header de estado, referencia faltante a `branding-ux-ui.md` en la tabla de documentación), `src/AGENTS.md` (header, árbol de `Features`/`Controllers` con Región/Comuna, línea de frontend, estado de BD), `CLAUDE.md` raíz (última referencia a puerto `7000`).

**Decisiones tomadas:**
- Editar los `.docx` existentes por XML (python-docx, `addnext`/`addprevious`/clonado de tablas) en vez de regenerarlos completos desde cero como en sesiones anteriores — más seguro para un documento largo porque no arriesga fidelidad en el ~90% de contenido que no cambia, y más verificable (diff conceptual claro de qué se tocó). No se registra como ADR por ser una decisión de herramienta/proceso de documentación, no de arquitectura del sistema.
- Verificar cada localización por texto exacto y contra una copia de prueba antes de tocar el archivo real — encontró los 2 bugs de duplicación antes de que llegaran al documento entregado.
- No se regeneró `Diccionario_Datos_OPT.docx` (sin cambios de esquema esta sesión) ni `Manual_Tecnico_UX_OPT.docx` (sin cambios de diseño, solo de implementación — ya cubierto en `.agents/context/branding-ux-ui.md`) ni `OPT_Propuesta_Arquitectura.docx` (documento histórico de análisis, no de estado de implementación).

**Sin resolver / pendiente para el equipo (heredado):**
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), Cliente/Anamnesis/RecetaCristales/OrdenDeTrabajo/Producto pendientes de migración de datos, gestión de `EmpresaSucursal`, Región/Comuna sin consumidor en el frontend, auditoría de los 4 módulos de backend existentes (declinada anteriormente), qué hacer con la iteración previa en `src/`.

**Próximos pasos sugeridos:**
1. Abrir ambos manuales en Word y ejecutar "Actualizar campo" sobre el Índice (es un TOC de Word, no texto estático) para que refleje los capítulos nuevos/renumerados.
2. Continuar con Fase 1 de negocio (Clientes) o con `OPT.Migracion` (Producto/Cliente), según prioridad del equipo.

---

## 2026-08-26 — `OPT.Migracion`: Cliente, Anamnesis y RecetaCristales (datos sensibles Ley 21.719)

**Resumen:**
- El usuario pidió migrar Cliente, Anamnesis, "Atención" y RecetaCristales desde `db_a25cfd_opt2` hacia `dbOPT_NET`, con la misma cadena de conexión local usada en sesiones anteriores. Antes de escribir código se perfiló en vivo (vía `sqlcmd`) el legacy real para estas 4 entidades, lo que reveló 3 brechas estructurales entre el legacy y el esquema Fase 0 que no estaban documentadas — se resolvieron con el usuario vía `AskUserQuestion` antes de tocar nada:
  1. **`OPT_RecetaCristales` nuevo no tenía columnas para Distancia Pupilar (DP) ni Adición (ADD)**, pese a que el legacy las usa en el 91% de sus 13.183 filas → decisión: extender el esquema ahora (no descartar el dato).
  2. **`OPT_Cliente` nuevo no tenía `FechaNacimiento` (66% de uso en legacy) ni `TipoPrevision` (59%)** → decisión: extender el esquema para ambos. `idEmpresa` (100% de uso, pero redundante — `OrdenDeTrabajo` ya tiene su propio `EmpresaId` a nivel de orden) e `idTipoDocumento` (99.8% un único valor constante) se descartaron sin extender el esquema.
  3. **`OPT_Atencion` (legacy, 1.184 filas) no tiene tabla equivalente en el esquema nuevo** — `Anamnesis`/`RecetaCristales` ya se vinculan directo a `Cliente` sin un evento "Atención" intermedio, y solo 1.184/13.183 filas de `RecetaCristales` pasan por esa tabla igual → decisión: queda fuera de alcance, no se crea tabla nueva.
  4. **`Anamnesis.CreadoPor`**: a diferencia de Sucursal/Empresa/Usuario (sin info de creador en el legacy), `Anamnesis` sí tiene `RutUsuario` real → se preguntó igual, y el usuario eligió mantener el criterio uniforme ya usado (Usuario bootstrap), no mapear el RutUsuario real.
- **Esquema extendido** (`003_extras_cliente_receta.sql`, procedimiento de regeneración de migration documentado en `CLAUDE.md`): `OPT_Cliente.FechaNacimiento` (`date`), `OPT_Cliente.TipoPrevision` (`nvarchar(50)`), `OPT_RecetaCristales.DpLejos`/`DpCerca`/`AddLejos` (`nvarchar(20)` — texto libre, no decimal, porque el legacy registra DP/ADD en formatos compuestos como `"58-56"` o `"+150/+200"` que no caben en una columna numérica). Aplicado y verificado contra `dbOPT_NET`.
- **`OPT.Migracion` extendido**: nuevos `LegacyCliente`/`LegacyAnamnesis`/`LegacyRecetaCristales` + queries de solo lectura, `NuevoCliente`/`NuevaAnamnesis`/`NuevaRecetaCristales` + inserts, `ComunaMapper` (mismo patrón que `RolMapper` — mapeo por nombre, nunca por id legacy), `RecetaCristalesParser` (parseo de esfera/cilindro/eje). `Program.cs` se restructuró para migrar por **grupos independientes** (Organización / Clínico) en vez de un solo guard global — necesario porque Organización ya estaba migrada de la sesión 2026-08-24 y debía omitirse (reutilizando el Usuario bootstrap existente, buscado por RUT) mientras que Clínico sí se insertaba.
- **Parseo de `RecetaCristales`**: perfilando las excepciones de un primer dry-run real (226 de 158.196 campos) se encontró que casi todas eran formato válido mal manejado, no basura — 193 usaban coma como separador decimal (formato chileno, `"-0,50"`) y 20 eran el término clínico real `"NEUTRO"`/`"NEUTROS"` (sin corrección). Se corrigió el parser para manejar ambos casos explícitamente; el recuento final de excepciones genuinas bajó a 14 (typos irrecuperables como `"-O50"`, `"NaN"`) — se guardan `NULL` y quedan en el reporte de dry-run, nunca se adivina el valor.
- **2 bugs reales encontrados y corregidos durante la ejecución** (no en el dry-run, que no toca la base): (1) una lectura auxiliar con Dapper (`ObtenerUsuarioIdPorRutAsync`) no recibía el `SqlTransaction` de la conexión de escritura activa → `InvalidOperationException` en runtime; (2) Dapper no soporta `System.DateOnly` como parámetro directo (`Cliente.FechaNacimiento`) → `NotSupportedException` en runtime, se corrigió convirtiendo a `DateTime?` antes de pasarlo. Ambos fallos fueron capturados limpiamente por el `try/catch` + `ROLLBACK` de la transacción única — cero filas quedaron a medio escribir en ninguno de los 2 intentos fallidos, confirmando que el patrón de transacción del diseño de `OPT.Migracion` funciona como se esperaba.
- **Ejecución real** (tras dry-run limpio + confirmación explícita del usuario vía `AskUserQuestion`, sin backup adicional — la transacción única ya protege contra un fallo a mitad de camino): `--execute` corrió en una única transacción, COMMIT exitoso. Verificado post-commit vía `sqlcmd`: 11.881 Cliente (RUTs todos únicos, incluidas las 16 variantes de formato migradas tal cual sin fusionar, decisión ya aprobada en 2026-08-21), 1.261 Anamnesis, 13.183 RecetaCristales — coincide exactamente con lo previsto en el dry-run. 9.238/11.881 Clientes con `ComunaId` mapeado (el resto `NULL`, comunas sin match de nombre o sin comuna en el legacy). Todo `CreadoPor` apunta al mismo Usuario bootstrap (Id=1) ya migrado en la sesión 2026-08-24.

**Decisiones tomadas:**
- Las 4 decisiones de brecha de esquema/alcance listadas arriba (extender `RecetaCristales` con DP/ADD como texto libre, extender `Cliente` con FechaNacimiento/TipoPrevision, descartar `idEmpresa`/`idTipoDocumento` de Cliente, dejar `OPT_Atencion` fuera de alcance, `Anamnesis.CreadoPor` = bootstrap uniforme) — todas confirmadas con el usuario vía `AskUserQuestion` antes de escribir código.
- Migrar por grupos independientes (Organización / Clínico) en `OPT.Migracion`, reutilizando el Usuario bootstrap ya migrado por búsqueda de RUT cuando Organización ya tiene datos — patrón a reutilizar para Producto/OrdenDeTrabajo.
- Ejecutar sin backup adicional de `dbOPT_NET` — la transacción SQL única ya cubre el escenario de fallo a mitad de camino (y de hecho lo hizo, 2 veces, durante esta misma sesión).

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Migrar Producto/ProductoSucursal y OrdenDeTrabajo/DetalleOT/Abono/BitacoraOT (el más complejo del plan original de ~18 fases) — sigue pendiente.
- 16 grupos de RUT de Cliente con variantes de formato (mismo cliente probablemente duplicado bajo distinto string) quedan en `dbOPT_NET` tal cual, sin fusionar — pendiente de revisión manual/limpieza de datos si el negocio lo requiere (decisión ya tomada 2026-08-21: no fusionar automáticamente).
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), mono-óptica vs. multi-tenant, qué hacer con la iteración previa en `src/`.

**Próximos pasos sugeridos:**
1. Migrar Producto/ProductoSucursal (bajo-medio riesgo, sin datos personales) como siguiente paso natural antes de OrdenDeTrabajo.
2. Continuar con Fase 1 de negocio (casos de uso de Clientes en `OPT.Application`/`OPT.API`) — ahora hay datos reales en `dbOPT_NET` para probar contra ellos.

---

## 2026-08-26 — Refresco de documentación IA y manuales técnicos (post-migración Clínico)

**Resumen:**
- El usuario pidió actualizar los `.md` de contexto para IA y los manuales técnicos (`.docx`) para reflejar la migración de datos Clínico (Cliente/Anamnesis/RecetaCristales) y la extensión de esquema (`003_extras_cliente_receta.sql`) de la sesión anterior, mismo día.
- **`.md` de IA actualizados**: `AGENTS.md` raíz (header de estado, estructura de directorios con `003_extras_cliente_receta.sql`, tabla de stack BD, sección "Decisiones pendientes" con entrada resuelta 2026-08-26), `src/AGENTS.md` (header, árbol de `basedatos/`, sección "Base de datos — contexto actual" con los conteos reales de Cliente/Anamnesis/RecetaCristales, "Próximo script esperado" ahora apunta a `004_`), `.agents/context/glosario-dominio.md` (entrada "Atención" anotada: sin tabla equivalente en el esquema nuevo, confirmado fuera de alcance). `CLAUDE.md`, `.agents/progress.md` y `.agents/context/migracion-datos-legacy.md` ya se habían actualizado en la sesión anterior (mismo día) como parte del trabajo de migración — esta sesión corrigió además 2 referencias cruzadas que habían quedado con la nota "pendiente actualizar Diccionario/Manual" una vez que ese trabajo efectivamente se completó (ver abajo).
- **`Diccionario_Datos_OPT.docx` actualizado** (edición quirúrgica vía XML con `python-docx`, mismo enfoque de la sesión 2026-08-25 — clonar filas/párrafos existentes en vez de reconstruir, para preservar formato exacto): agregadas las 5 columnas nuevas a las tablas `Columna|Tipo|Nulo|Default|Notas` de `OPT_Cliente` (`FechaNacimiento`, `TipoPrevision`) y `OPT_RecetaCristales` (`DpLejos`, `DpCerca`, `AddLejos`); nueva fila en «6. Historial de scripts» para `003_extras_cliente_receta.sql`; fila «Migración de datos reales del legacy» de «Pendientes» actualizada con el estado real (Organización + Clínico migrados); párrafo de «1.2 Alcance» actualizado de "dos scripts" a "tres scripts". **Bug evitado por verificación previa**: las celdas de esta tabla resultaron tener un solo `run` de texto por celda (patrón simple) — se verificó la estructura de runs antes de clonar filas, replicando la lección de la sesión 2026-08-25 (nunca asumir la estructura de runs sin inspeccionarla primero).
- **`Manual_Tecnico_Backend_OPT.docx` actualizado**: la tabla «13.3 Estado de la migración» (antes con una sola fila «Cliente, Anamnesis, RecetaCristales — Pendiente») se dividió en 3 filas «Migrada» con sus conteos reales; el heading de esa sección pasó de «(2026-08-24)» a «(2026-08-26)»; se insertó un párrafo explicativo sobre la extensión de esquema y `OPT_Atencion` fuera de alcance; se agregaron 3 bullets nuevos a «13.4 Reglas y patrones aplicados» (perfilar volumen de uso antes de descartar un campo, coma decimal + terminología clínica "NEUTRO" en el parseo, migración por grupos independientes con reutilización del Usuario bootstrap). **Bug real evitado**: a diferencia del Diccionario, las celdas de la tabla `Entidad|Estado|Filas migradas` de este manual tenían **dos runs por celda** (uno vacío + uno con el texto real) — el mismo patrón que causó el bug de duplicación de la sesión 2026-08-25. Se detectó inspeccionando la estructura antes de editar y se usó una función que localiza el run con más texto (no "el primero") como el run real a preservar, en vez de asumir que el primer run es el correcto.
- No se tocaron `Manual_Tecnico_Frontend_OPT.docx`, `Manual_Tecnico_UX_OPT.docx` ni `OPT_Propuesta_Arquitectura.docx` — sin cambios que les correspondan esta sesión.

**Decisiones tomadas:**
- Mismo enfoque de edición quirúrgica por XML (clonar filas/párrafos existentes, nunca regenerar el documento completo) que la sesión 2026-08-25 — y misma disciplina de verificar la estructura de runs de cada tabla antes de escribir, en vez de asumir que todas las tablas del mismo documento (o de documentos distintos) comparten el mismo patrón de runs.

**Sin resolver / pendiente para el equipo (heredado):**
- Migrar Producto/ProductoSucursal y OrdenDeTrabajo/DetalleOT/Abono/BitacoraOT — sigue pendiente.
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), mono-óptica vs. multi-tenant, qué hacer con la iteración previa en `src/`, validación de la propuesta de branding con stakeholders.

**Próximos pasos sugeridos:**
1. Migrar Producto/ProductoSucursal como siguiente paso natural antes de OrdenDeTrabajo.
2. Continuar con Fase 1 de negocio (casos de uso de Clientes en `OPT.Application`/`OPT.API`).

---

## 2026-08-26 — Fase 1: APIs de Clientes, Anamnesis y RecetaCristales

**Resumen:**
- El usuario pidió crear las APIs de Clientes, Anamnesis, "Atención" y RecetaCristales, tomando el legacy como guía pero considerando los cambios de la migración. Antes de escribir código se preguntó por "Atención" vía `AskUserQuestion`, porque el pedido contradecía una decisión ya documentada (`OPT_Atencion` sin tabla equivalente en el esquema nuevo, confirmado con el usuario en la sesión de migración del mismo día): el usuario confirmó **no** crear un módulo Atención — solo Clientes/Anamnesis/RecetaCristales como recursos independientes, el frontend orquesta la creación conjunta llamando a los 3 endpoints.
- **Dominio**: se agregó `Anamnesis.Actualizar(...)` y `RecetaCristales.Actualizar(...)` (comprehensivo, reutiliza `SetOjoDerecho`/`SetOjoIzquierdo`/`SetDpAdd` + llama `SetModificacion`) — el scaffold original solo tenía `Crear` para estas dos entidades; el legacy (`OPT_AnamnesisDAL.Editar`/`OPT_RecetaCristalesDAL.Editar`) sí soporta edición.
- **Repositorios nuevos**: `IAnamnesisRepositorio`/`AnamnesisRepositorio` e `IRecetaCristalesRepositorio`/`RecetaCristalesRepositorio` (patrón `RepositorioBase<T>`, `ObtenerPorPublicIdAsync` con `Include(Cliente)` para resolver `ClientePublicId` en el DTO, `ObtenerPorClienteAsync` para el historial). `IClienteRepositorio` ganó `ObtenerPorPublicIdAsync` (faltaba — solo tenía `ObtenerPorRutAsync`) y `BuscarPaginadoAsync(rut, nombre, pagina, tamanioPagina)`. `IComunaRepositorio` ganó `ExisteAsync(id)` para validar la FK `Cliente.ComunaId` antes de insertar/actualizar (mismo patrón que la validación de `RolId` en `Usuarios`).
- **`ObtenerClientesQuery` es paginado**, a diferencia de `ObtenerEmpresasQuery`/`ObtenerUsuariosQuery` (listado completo) — decisión deliberada: Cliente tiene ~11.881 filas reales (vs. 491 Empresa), devolver todo sin paginar no escala. Nuevo `PagedResult<T>` en `OPT.Application/Common/Models/`. Rut/Nombre son filtros opcionales (equivalente al buscador `OPT_ClienteDAL.Lista` del legacy).
- **Anamnesis y RecetaCristales expuestos como recursos propios** (con su propio `PublicId`, ADR `0004`) en vez de anidados bajo `/api/clientes/{id}/...` — cada uno tiene su propio controller (`AnamnesisController`, `RecetaCristalesController`) con `GET /api/{recurso}?clientePublicId={guid}` para el historial de un cliente (reemplaza `ListaPorRut`/`Listar(pRut)` del legacy) y CRUD completo por `PublicId` propio.
- **Gotcha de namespace evitado**: el namespace `OPT.Application.Features.Anamnesis` coincide literalmente con el nombre de la entidad de dominio `Anamnesis` (a diferencia de `Empresas`/`Usuarios`, cuyo singular de entidad no coincide con el nombre del namespace en plural) — mismo problema para `RecetaCristales`. Se resolvió con alias de tipo (`using EntidadAnamnesis = OPT.Domain.Entities.Clinico.Anamnesis;`) en los handlers que instancian la entidad, en vez de renombrar el namespace de la feature (que rompería la convención `Features/<Modulo>/`). Verificado con `dotnet build OPT.sln` — compila limpio.
- **`RecetaCristalesMapper`** (clase estática interna en la raíz de `Features/RecetaCristales/`) centraliza el mapeo Entidad→DTO — evita duplicar los 19 campos en 4 handlers distintos (Crear/Actualizar/ObtenerPorId/ObtenerPorCliente).
- Se registraron los 2 repositorios nuevos en `OPT.Infrastructure/DependencyInjection.cs`. No se tocó `AppDbContext` (los `DbSet<Anamnesis>`/`DbSet<RecetaCristales>` ya existían del scaffold original).

**Decisiones tomadas:**
- No crear módulo "Atención" — confirmado con el usuario vía `AskUserQuestion`, consistente con la decisión ya tomada en la sesión de migración del mismo día.
- `ObtenerClientesQuery` paginado con filtro Rut/Nombre opcional, en vez de listado completo — justificado por volumen real de datos (~11.881 filas), no es sobre-ingeniería especulativa.
- RUT de Cliente es inmutable después de creado (`ActualizarClienteCommand` no lo incluye) — coherente con que es el identificador de negocio externo usado para buscar/fusionar duplicados, y el legacy tampoco lo permite editar en su vista de edición.
- Rango de validación de graduación óptica en `RecetaCristales`: -30.00 a +30.00 (mismo rango documentado en el comentario de `RecetaCristalesConfiguration`), eje 0-180 grados — no existía en el legacy (los campos eran `string` sin validación), es una mejora directa.

**Sin resolver / pendiente para el equipo (heredado):**
- Migrar Producto/ProductoSucursal y OrdenDeTrabajo/DetalleOT/Abono/BitacoraOT — sigue pendiente.
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), mono-óptica vs. multi-tenant, qué hacer con la iteración previa en `src/`, validación de la propuesta de branding con stakeholders.
- Frontend: no se tocó `src/frontend/` esta sesión — falta el consumo real de estos 3 endpoints nuevos (pantallas de Clientes/Anamnesis/RecetaCristales).
- No se agregaron tests automatizados (no existe aún el proyecto de pruebas en `OPT.sln`).

**Próximos pasos sugeridos:**
1. Verificar end-to-end contra `dbOPT_NET` real (los 11.881 clientes/1.261 anamnesis/13.183 recetas ya migrados sirven de datos de prueba) — la sesión no incluyó pruebas manuales contra la base, solo `dotnet build`.
2. Frontend: pantallas de Clientes (con el buscador paginado) y las fichas de Anamnesis/RecetaCristales dentro de la ficha de cliente.
3. Migrar Producto/ProductoSucursal como siguiente paso natural antes de OrdenDeTrabajo.
3. Abrir ambos manuales en Word y ejecutar "Actualizar campo" sobre el Índice para que refleje cualquier cambio de numeración de secciones (no se agregaron/quitaron secciones esta sesión, pero sigue siendo buena práctica tras cualquier edición).

---

## 2026-08-26 — Mejora de UX/UI del frontend (estados vacío/error, navegación responsive, login)

**Resumen:**
- El usuario pidió, como experto UX/UI, analizar el diseño del legacy (`old/Fuente`) y el del frontend nuevo (`src/frontend`) y mejorar este último para que fuera más intuitivo, amigable y con una visual profesional. Se delegó el análisis a dos agentes `Explore` en paralelo (uno por codebase) para no gastar contexto propio en lectura exhaustiva.
- **Hallazgos del legacy** (Bootstrap 4 + Font Awesome, sin sistema de diseño propio): navegación por navbar+dropdowns que no escala con roles, inconsistencia modal-vs-página-completa sin criterio entre módulos similares, sin feedback de carga visible, mensajes de error que filtran detalle técnico (`jqXHR.responseText`). A preservar: terminología de negocio en español ya consolidada (RUT, OT, Abono, Ficha Médica), el flujo de OT por tabs (Cliente→Receta→Lentes→Abonos), y el patrón "buscar → listar paginado → acciones por fila".
- **Hallazgos del frontend nuevo** (ya con tema M3 de marca y patrón lista+diálogo consistente, ver sesión 2026-08-25): **cero responsive design** (ni un `@media` en todo `src/app`), estados vacíos mínimos (un `<p>` gris sin ícono ni CTA), **sin manejo visible de error de carga** (un `catch` de HTTP solo apagaba el spinner, dejando la lista en el mismo estado visual que "sin datos" — un fallo de red quedaba indistinguible de una lista realmente vacía), `roles-list` sin filtro/paginador consistente con el resto (aunque esto resultó ser una decisión ya justificada por volumen real, no un bug), login sin identidad de marca fuerte en la primera impresión.
- **Componente nuevo**: `shared/components/empty-state/` (`app-empty-state`, generado con `ng generate` para heredar la convención de nombres) — ícono + título opcional + mensaje + acción proyectada (`<ng-content>`), con `tone` `'neutral'`/`'error'` para no confundir "sin datos" con "falló la carga". Aplicado a los 5 listados existentes (`sucursales-list`, `empresas-list`, `usuarios-list`, `roles-list`, `clientes-list`), a las 2 pestañas de sub-recurso de `cliente-ficha` (Anamnesis/RecetaCristales) y a los 2 placeholders de módulo pendiente (`ordenes-de-trabajo-list`, `inventario-list`, ahora con ícono "en construcción" en vez de solo texto).
- **Manejo de error real**: cada listado ganó un signal `error` (además de `cargando`); el método de carga pasó de `private` a `protected` para que el template lo vuelva a invocar desde el botón "Reintentar" del `app-empty-state` en modo error. `clientes-list` además distingue un tercer caso ("sin resultados" de un filtro activo, signal `hayFiltro`) del caso "sin datos" genérico.
- **Navegación lateral responsive** (`layout/shell/`): `BreakpointObserver` de `@angular/cdk/layout` (ya era dependencia transitiva de Angular Material, sin paquete nuevo) observa `(max-width: 959.98px)`; el `mat-sidenav` pasa de `mode="side"` fijo a `mode="over"` con un botón de menú hamburguesa en el `mat-toolbar` que aparece solo bajo ese punto de quiebre. `shell.scss` ganó `overflow-x: auto` en `.contenido`, dando scroll horizontal automático a cualquier tabla ancha sin tocar cada feature.
- **Login con panel de marca** (`layout/auth-layout/`): en viewports ≥900px se agrega un panel lateral con degradado `primary`→`primary-dark` y un acento sutil (`--opt-accent` radial, opacidad ≤0.35, ubicado en la esquina opuesta al texto para no perder contraste) con el lockup + tagline + una línea de propuesta de valor; por debajo de 900px el panel se oculta y el lockup/tagline reaparecen dentro de la card de login (`@media` espejado en ambos archivos para no duplicar el mensaje en pantallas anchas). Sucursal ganó además un chip visual para "Matriz" (antes texto plano "Sí/No").
- **Verificación real, no solo build**: se instaló Playwright + Chromium localmente (no estaba en el proyecto) para levantar el dev server (`ng serve`) y capturar screenshots reales del login (ancho/angosto) y, forjando un JWT de sesión válido en `sessionStorage` (mismo esquema de claims que `Auth.usuarioDesdeToken`, sin backend corriendo), del shell autenticado con el backend apagado — esto confirmó en vivo que el estado de error (`app-empty-state tone="error"` + botón Reintentar) se dispara correctamente ante un `ERR_CONNECTION_REFUSED` real, y que el sidenav colapsa/expande correctamente en un viewport de 420px. `npm run lint`, `npm test` (38/38, subieron de 36 por el spec nuevo de `empty-state`) y `npm run build` (sin warnings de budget) quedaron en verde, incluido tras un `npm run format` que Prettier aplicó sobre 5 archivos tocados.
- **Observación menor sin corregir**: el ícono `expand_more` de los grupos del menú lateral se renderizó como texto sin convertir a glifo en una de las capturas de Chromium headless — no se tocó ese código (preexistente) y es probablemente un artefacto del entorno de captura, no un bug real; queda anotado para confirmar en un navegador real.
- **Documentación actualizada en la misma sesión** (a pedido explícito del usuario): `.agents/context/branding-ux-ui.md` (nueva sección "Patrones de UI agregados" con la tabla de los 4 patrones, 2 bullets nuevos en "Componentes UI — lineamientos", referencia agregada en "Cómo aplicar este contexto"), `src/frontend/CLAUDE.md` (nueva sección "Estados de listado: vacío y error", 2 bullets nuevos en "Branding y sistema visual"), `CLAUDE.md` raíz (párrafo nuevo dentro del punto 3 de Pendiente), y `Manual_Tecnico_UX_OPT.docx` editado quirúrgicamente por XML (mismo enfoque de sesiones anteriores: `python-docx`, clonar filas/párrafos existentes, verificar el patrón de runs de cada tabla antes de escribir — esta tabla también tenía 2 runs por celda) para: nota de actualización en el encabezado, 2 bullets nuevos en sección 11, 2 capturas de pantalla reales insertadas en sección 12 (a continuación de los mockups conceptuales existentes, no reemplazándolos — con su propio subtítulo "Estado real implementado"), 2 filas nuevas + 2 celdas actualizadas en la tabla de sección 15 (Fases 1 y 5 marcadas "Hecho", Fases 7-8 nuevas), e intro de sección 16 corregido de "futura implementación" a implementación real vigente.

**Decisiones tomadas:**
- No tocar `roles-list` para agregarle paginador/filtro — el hallazgo inicial de "inconsistencia" resultó ser, tras revisar `CLAUDE.md`, una decisión ya justificada por volumen real (3 sucursales, 13 usuarios, catálogo de roles fijo); agregar paginación ahí habría sido sobre-ingeniería no solicitada.
- Estado de error y estado vacío nunca comparten el mismo componente visual sin distinguirse — es la corrección de UX más importante de la sesión (un error de red no debe parecer "no hay datos").
- Instalar Playwright + Chromium localmente para verificación visual real en vez de confiar solo en `lint`/`test`/`build` — consistente con la regla de `CLAUDE.md` raíz ("Para UI o frontend changes, start the dev server and use the feature in a browser before reporting the task as complete").
- Actualizar `.md` de IA y el `.docx` de diseño en la misma sesión del cambio de código (no diferirlo) — mismo criterio que las sesiones de migración/backend anteriores.

**Sin resolver / pendiente para el equipo (heredado):**
- Migrar Producto/ProductoSucursal y OrdenDeTrabajo/DetalleOT/Abono/BitacoraOT — sigue pendiente.
- Los mismos puntos de sesiones anteriores: Ley 21.719 (ADR `0004`), mono-óptica vs. multi-tenant, qué hacer con la iteración previa en `src/`, validación de la propuesta de branding con stakeholders, vector final del logotipo.
- Verificar en un navegador real (no headless) el glifo `expand_more` del menú lateral, por si el artefacto observado en Chromium headless también ocurre en uso real.

**Próximos pasos sugeridos:**
1. Validar visualmente en un navegador real (no solo headless) contra `dbOPT_NET` con sesión autenticada real, ahora que hay datos de Clientes/Anamnesis/RecetaCristales migrados para probar los listados con filas reales (no solo el estado de error/vacío).
2. Migrar Producto/ProductoSucursal como siguiente paso natural antes de OrdenDeTrabajo.
3. Aplicar el mismo criterio de estados vacío/error a las pantallas de OrdenesDeTrabajo/Inventario cuando el backend deje de ser un stub.

---

## 2026-08-27 — 2ª pasada UX/UI del frontend (revisión de acabado + modo oscuro)

**Contexto:** a pedido del usuario ("como experto en UX/UI analiza el diseño actual … realiza todas las mejoras incluyendo las brechas"), sobre la base v2.0 ya aplicada ese mismo día. No se tocó color, tipografía ni tono de v2.0 — solo estructura, accesibilidad y consistencia. Decisiones de alcance confirmadas por el usuario vía preguntas: formularios largos → agrupar dentro del `MatDialog` (no rutas); modo oscuro → implementarlo completo; acciones de fila → menú de overflow; guía de estilo → reconciliar estado + agregar secciones de sistema.

**Resumen:**
- **Modo oscuro completo.** Servicio nuevo `shared/services/tema.ts` (`Tema`, `providedIn: 'root'`): preferencia `light`/`dark`/`system` (default `system`), signal `oscuro` computed sobre `matchMedia` + preferencia, escribe `data-opt-theme` en `<html>`, persiste en `localStorage` (`opt.tema`) con try/catch. Toggle en la toolbar del `Shell` (menú Claro / Oscuro / Según el sistema). `styles.scss` emite `mat.theme` una vez para claro y otra para oscuro — **esta versión de Angular Material rechaza `theme-type: light dark` en una sola emisión** (`"Unknown theme-type provided: light dark"`), se probó y se descartó. El bloque oscuro aplica bajo `@media (prefers-color-scheme: dark)` y bajo `html[data-opt-theme='dark']`. `theme-tokens.scss` solo overridea en oscuro `--opt-accent-container` y las sombras; los chips de `OPT_EstadoOT` quedan idénticos en ambos temas a propósito (fondo + fg explícitos, isla de color, ya AA). Por el 2º set de tokens M3 en CSS, `angular.json` subió `initial maximumWarning` de 500 a 550 kB (documentado).
- **Escalas de sistema en `theme-tokens.scss`:** `--opt-space-*` (2–48px, base 4), `--opt-radius-*` (8/12/16/999), `--opt-elevation-1|2` (reforzadas en oscuro), `--opt-motion-*` + `--opt-easing-*`. Reset global de `prefers-reduced-motion` en `styles.scss`.
- **Tipografía global de encabezados** en `styles.scss`: `h1/h2/h3:not([class])` a la escala de la guía §4 (27/21/16.5, peso 600). Los `h2[mat-dialog-title]` quedan excluidos por `:not([class])`.
- **Componentes compartidos nuevos** (creados a mano, sin `ng generate` — E/S de la carpeta montada demasiado lenta, mismo motivo que sesiones previas; con su `.spec.ts`):
  - `shared/components/page-header/` (`app-page-header`): `title`/`subtitle?`/`backTo?`/`backLabel`, acción primaria por `<ng-content>`. Reemplaza el `<div class="encabezado">` reimplementado en 5 features. Aplicado a los 7 listados + los 2 stubs + `cliente-ficha`.
  - `shared/components/list-skeleton/` (`app-list-skeleton`): `rows`/`header`, shimmer con `role="status"` + texto "Cargando…", apaga animación bajo `prefers-reduced-motion`. Reemplaza el `<mat-spinner diameter="32">` suelto en todos los listados.
- **Acciones de fila → menú de overflow** en `clientes-list` (ver ficha inline; editar/eliminar en `mat-menu` con `<ng-template matMenuContent>` + `[matMenuTriggerData]`) y `usuarios-list` (editar inline; clave/sucursales/activar/eliminar en el menú — antes 5 icon-buttons por fila). `empresas-list`/`sucursales-list` (2 acciones) se dejan inline. Todo `mat-icon-button` con `[attr.aria-label]` descriptivo; la acción destructiva con la clase global nueva `.opt-accion-destructiva` (`--mat-sys-error`).
- **`.opt-mono` aplicado** a las celdas de RUT de Clientes/Usuarios/Empresas y a la tabla de graduación de la receta (era una brecha: la clase existía sin consumidores).
- **Formularios:** `<mat-error>` por tipo de error + `<mat-hint>` en todos los formularios (login, cliente, sucursal, empresa, usuario, cambio de clave, anamnesis, receta). Toggle mostrar/ocultar clave (`matSuffix`, signal `ocultarClave`, `aria-label` + `aria-pressed`) en login, alta de usuario y cambio de clave. `cliente-form` (11 campos) reorganizado con `class="opt-form-grid"` (2 columnas, colapsa a 1 <600px) + subtítulos `<p class="opt-form-section">` (Identificación / Contacto / Ubicación) + `.opt-col-2` para email/dirección — clases globales nuevas en `styles.scss`. Sigue siendo `MatDialog`, no ruta.
- **`cliente-ficha`:** `fechaNacimiento` con `DatePipe` (antes ISO crudo); badge clínico persistente en el `app-page-header` con ícono `lock` y texto unificado **"Información clínica"** (antes había "Salud" y "Dato de salud — Ley 21.719" mezclados — se unificaron los 3 formularios/pantallas clínicas); estado de error real (`app-empty-state`) cuando la carga del cliente falla (antes: pantalla en blanco); se eliminó el método `volver()`/inject de `Router` (lo cubre `backTo` del page-header).
- **Verificación:** `npm run lint` (limpio), `npm test` (**47/47**, 40 archivos — subió de 38 por los specs de `tema`, `page-header`, `list-skeleton`), `npm run build` (sin warnings de budget tras subir el límite). No verificado aún en navegador real ni contra `dbOPT_NET` con sesión autenticada.
- **Documentación actualizada en la misma sesión:** `.agents/context/branding-ux-ui.md` (nota de 2ª pasada en el estado + 6 secciones nuevas: Modo oscuro, Escalas de sistema, Componentes compartidos de layout, Menú de overflow, Formularios, Encabezados de página), `src/frontend/CLAUDE.md` (7 bullets nuevos en "Branding y sistema visual"), `src/Guia_de_estilo/index.html` (banner de estado reconciliado a "aplicada, pendiente validación" + secciones nuevas 12–17: Espaciado, Elevación y forma, Movimiento, Modo oscuro, Layout de formularios, Tablas y listados + entradas de nav). Los `.docx` de `src/documentos/` **no** se tocaron esta vez — quedan pendientes de sincronizar (Manual_Tecnico_Frontend_OPT.docx §5, Manual_Tecnico_UX_OPT.docx §§11-16).

**Decisiones tomadas:**
- Modo oscuro por doble emisión de `mat.theme` (no `theme-type: light dark`) — forzado por la versión de Angular Material instalada; anotado para simplificar cuando se actualice.
- Chips de `OPT_EstadoOT` no invierten con el tema — son semánticos, no decorativos.
- Subir el budget `initial` a 550 kB en vez de recortar en otro lado — el 2º set de tokens M3 es el costo legítimo del modo oscuro pedido.
- Formularios largos: agrupar dentro del diálogo (grid + secciones), no promover a ruta — se respeta la regla de `frontend/CLAUDE.md`.
- No tocar los `.docx` esta sesión para no expandir el alcance; anotados como pendientes explícitos.

**Sin resolver / pendiente para el equipo (heredado + nuevo):**
- Sincronizar `Manual_Tecnico_Frontend_OPT.docx` y `Manual_Tecnico_UX_OPT.docx` con esta 2ª pasada.
- Verificar en navegador real (claro y oscuro) contra `dbOPT_NET` con sesión autenticada — incluido el contraste de los tokens M3 oscuros generados (no se recalcularon a mano, se confía en el algoritmo M3).
- Los mismos puntos de siempre: Ley 21.719, migración Producto/OT, validación de branding con stakeholders, vector del isotipo.

**Próximos pasos sugeridos:**
1. Levantar el dev server y recorrer las pantallas en ambos temas y en viewport angosto antes de dar por cerrado.
2. Sincronizar los 2 `.docx`.
3. Al implementar OrdenesDeTrabajo/Inventario, usar `app-page-header` + `app-list-skeleton` + menú de overflow desde el inicio.

---

## 2026-08-27 — Propuesta v2.1: densidad y tipografía (3ª pasada UX/UI, misma fecha)

**Contexto:** el usuario pidió analizar el frontend con la skill `.agents/skills/ui-ux-pro-max`, señalando que "la fuente es muy grande, en especial los input", y validar si el branding transmite "amigable, sencillo pero robusto". Primero se generó una propuesta en HTML (`src/Guia_de_estilo/propuesta-densidad-tipografia.html`) sin tocar código; luego, tras confirmar alcance con el usuario, se aplicó.

**Alcance confirmado por el usuario (vía preguntas):**
- Densidad Material: **`-2` global** (no `-3`, no densidad extra en tablas) — es el nivel estable con `appearance="outline"` (el único appearance usado, 52 instancias).
- Aplicar el núcleo **+ todos los ajustes de marca §7** de la propuesta.
- Actualizar **ambos** `.docx`: UX y Frontend.

**Diagnóstico (causa raíz):** el tamaño percibido NO venía de `font-size` sueltos mal puestos, sino de dos decisiones globales en `src/styles.scss`: `density: 0` en `mat.theme()` (calibración táctil/móvil de M3 → form-field ~56px) + la **escala tipográfica M3 por defecto** (texto de input = rol `body-large` = 16px). Se corrige en el mismo lugar de origen, sin tocar ningún `.scss` de feature (salvo un margen en `clientes-list`).

**Cambios aplicados (7 archivos):**
- `src/styles.scss`:
  - `density: 0` → **`density: -2`** en `mat.theme()` (form-field ~56→~48px, filas ~52→~44px, botones ~40→~32px).
  - **Escala tipográfica compacta v2.1** — override de tokens de sistema M3 *después* de `mat.theme()` (familias y pesos siguen saliendo del tema; solo cambia tamaño/interlineado). Se redefine cada rol granular (`--mat-sys-<rol>-size` / `-line-height`) **y** su shorthand (`--mat-sys-<rol>`), porque `styles.scss` y algunos componentes usan `font: var(--mat-sys-body-medium)`. Valores: `body-large` 16→14, `body-medium` 14→13.5, `body-small` 12→11.5, `label-large` 14→13, `title-medium` 16→15, `title-large` 22→19.
  - **Excepción móvil:** `@media (max-width: 599.98px) { html { --mat-sys-body-large-size: 1rem } }` — el texto del input vuelve a 16px bajo 600px para no disparar el auto-zoom de iOS al enfocar un campo.
  - Encabezados "desnudos" §4: H1 27→24, H2 21→19, H3 16.5→15 (peso 600 sin cambio).
  - **Cabecera de `mat-table` liviana (§7):** de banda `primary-container` plena → fondo `surface` + borde inferior 2px `primary` + texto `primary`. El hover de fila conserva el tinte `primary-container`.
- `src/app/layout/shell/shell.scss`: `.contenido` padding 24→20 (móvil sigue 16).
- `src/app/shared/components/page-header/page-header.scss`: `margin-bottom` `--opt-space-lg`→`--opt-space-md` (24→16); `.page-header__titulo` 27→24.
- `src/app/shared/components/empty-state/empty-state.scss` (§7 "amigable"): padding 48→36; **`.empty-state__titulo` en Fraunces** (`var(--opt-font-titular)` tras el shorthand `font:`) — excepción deliberada a "nunca Fraunces en la interfaz de trabajo", acotada al título de un estado vacío/error ("momento de pausa"); ícono neutral de `--mat-sys-primary` → **`--opt-accent-dark`** (el lugar elegido para dejar respirar el terracota). El tono de error se mantiene en `--mat-sys-error`.
- `src/app/features/clientes/pages/clientes-list/clientes-list.scss`: `.filtro` `margin-bottom` 16→12.

**§7 evaluado y NO aplicado:** acento en "hint de campo obligatorio pendiente" (no hay un hook natural en el markup actual — los `mat-hint` existentes son informativos, no de obligatoriedad; forzarlo sería ruido fabricado). Acento en el ícono de la acción primaria de listado se descartó por contraste: el botón primario es relleno `primary` con ícono blanco; teñir el ícono de terracota sobre azul falla WCAG. El "copy con voz" de los estados vacíos **ya estaba resuelto** en la 2ª pasada ("Aún no hay clientes", "Registra el primero…") — no requirió cambios.

**Evaluación de branding "amigable, sencillo, robusto":**
- **Robusto:** logrado (añil profundo, IBM Plex, monoespaciado para RUT/folios, rampa de estado semántica, contrastes AA/AAA). Sin cambios.
- **Sencillo:** era parcial — no fallaba la paleta sino el tamaño/densidad. Lo corrige esta pasada + la cabecera de tabla liviana.
- **Amigable:** estaba subrepresentado (el terracota casi no se veía; Fraunces encerrado en el login). Los 2 toques del §7 en `empty-state` (Fraunces + terracota) lo suben sin saturar la interfaz de trabajo.

**Verificación:** `sass` aislado de `styles.scss` (confirmado que el override de shorthand gana en cascada al valor stock de `mat.theme`), luego **`npm run build`** (31s, sin warnings de budget — `styles.css` 21.6kB, prácticamente igual), **`npm run lint`** (limpio), **`npm test`** (**47/47**, 40 archivos). No verificado aún en navegador real ni contra `dbOPT_NET` con sesión autenticada.

**Decisiones tomadas:**
- Densidad `-2` (no `-3`): equilibrio compacto/cómodo estable con `appearance="outline"`; `-3` habría requerido QA visual del notch del outline (bug histórico de Material).
- Override de tokens M3 por CSS custom properties (no un mapa `typography` completo con `mat.define-typography-*`): menor superficie de cambio, mismo patrón ya usado en el proyecto para el halo de foco y `.opt-badge-clinico`. Anotado: si se actualiza Angular Material, revisar si conviene migrar al enfoque idiomático.
- Fraunces en `empty-state` es una excepción intencional y acotada, no una relajación de la regla general.

**Sin resolver / pendiente (heredado + nuevo):**
- Verificar en navegador real (claro y oscuro, 375px y escritorio, zoom 200%) que el outline no se recorta con density -2 y que el auto-zoom de iOS no ocurre en el input bajo 600px.
- Sincronizar la 2ª pasada UX/UI en los `.docx` seguía pendiente y se resuelve en esta misma sesión (UX §§ tipografía/densidad/tablas/empty-state; Frontend §5/branding) junto con la v2.1.
- Los de siempre: Ley 21.719, migración Producto/OT, validación de branding con stakeholders, vector del isotipo.

**Próximos pasos sugeridos:**
1. Levantar el dev server y recorrer listados + `cliente-form` (11 campos) en ambos temas y en viewport de 375px.
2. Al implementar OrdenesDeTrabajo/Inventario, la nueva densidad y escala ya aplican solas (tokens globales) — no re-estilar tamaños por feature.
3. Si el equipo valida v2.1, regenerar los mockups PNG del `Manual_Tecnico_UX_OPT.docx` con la densidad/escala nuevas.

---

## 2026-08-27 — Paginación server-side, búsqueda única y fix de formularios en modal (transversal)

**Contexto:** el usuario definió 3 requisitos para todos los módulos (actuales y futuros): (1) todo listado transaccional con paginación **del lado del servidor** (nada de traer todo a Angular ni paginar en memoria), con query params `pagina`, `tamanioPagina`, filtros opcionales, `ordenarPor`, `direccionOrden` y respuesta `PagedResult<T>` con metadatos; (2) búsqueda tipo Google (un solo input) en vez de N campos de filtro; (3) los formularios dentro de `MatDialog` se veían mal (scroll horizontal, título recortado, la grilla de 2 columnas no colapsaba). Decisiones confirmadas con el usuario: nomenclatura **en español**; búsqueda de Cliente contra RUT+Nombre+Apellido; aplicar **todo ahora** a los 4 módulos con listado (Clientes, Empresas, Usuarios, Sucursales); solo listados transaccionales (los catálogos siguen como lista completa).

**Backend — primitivas reutilizables:**
- `OPT.Domain/Common/ParametrosPaginacion.cs` (nuevo) — record base abstracto; `init` sanean `Pagina >= 1` y `TamanioPagina` acotado a `TamanioPaginaMaximo = 100` (fuera de rango -> 20). Expone `Busqueda`, `OrdenarPor`, `DireccionOrden`, calculados `OrdenDescendente` y `Saltar`. **Vive en Domain.Common** (no en Application) porque los contratos de repositorio la reciben como parámetro y Domain no referencia Application.
- `OPT.Domain/Common/PagedResult.cs` (movido desde `OPT.Application/Common/Models/` + enriquecido) — `record PagedResult<T>(Items, Pagina, TamanioPagina, Total)` + calculados `TotalPaginas`, `TienePaginaAnterior`, `TienePaginaSiguiente`. Incluye `PagedResultFactory.Crear<TEntidad,TDto>(items, total, parametros, map)`. **`OPT.Application/Common/Models/` quedó vacía.**
- `OPT.Infrastructure/Persistence/Extensions/QueryablePaginacionExtensions.cs` (nuevo) — `IQueryable<T>.AplicarOrden(ordenarPor, desc, columnasWhitelist, ordenPorDefecto)` (orden seguro por lista blanca explícita por entidad, sin `System.Linq.Dynamic`; siempre agrega `.ThenBy(e => e.Id)` como desempate estable) y `IQueryable<T>.PaginarAsync(parametros, ct)` -> `(Items, Total)` con `CountAsync` + `Skip/Take` + `ToListAsync`. Materializa `IQueryable` con EF -> vive en Infrastructure, Application no lo ve.

**Backend — por módulo (Clientes, Empresas, Usuarios, Sucursales):**
- `I{Entidad}Repositorio` + `{Entidad}Repositorio`: nuevo `BuscarPaginadoAsync(ParametrosPaginacion, ct)` con un `Dictionary<string, Expression<Func<T,object>>>` estático de columnas ordenables y el `Where` de búsqueda (`Contains` OR). Cliente: reemplaza el viejo `BuscarPaginadoAsync(string? rut, string? nombre, int, int)`. Usuario: `.Include(u => u.Rol)` en el query paginado (el DTO usa `Rol.Nombre`).
  - Búsqueda: Cliente = Rut/Nombre/Apellido · Empresa = Nombre/Rut/RazonSocial · Usuario = Nombre/Apellido/Rut/Email · Sucursal = Nombre/Direccion.
  - Orden permitido: Cliente `rut|nombre|apellido|email` · Empresa `nombre|rut|razonSocial` · Usuario `rut|nombre|apellido|email|activo` · Sucursal `nombre|direccion`.
- `Obtener{Modulo}Query` ahora `: ParametrosPaginacion, IRequest<PagedResult<{Entidad}Dto>>` (Empresas/Usuarios/Sucursales pasaron de `IReadOnlyList<Dto>` a `PagedResult<Dto>`).
- Handlers: `var (items, total) = await repo.BuscarPaginadoAsync(request, ct); return PagedResultFactory.Crear(...)`.
- Controllers: `ObtenerTodos([FromQuery] Obtener{Modulo}Query query, ...)` — el resto de acciones sin cambios.
- **Sin cambios de esquema** -> no hay script SQL nuevo en `src/basedatos/`. `dotnet build OPT.sln` limpio.

**Frontend — primitivas reutilizables:**
- `shared/models/paged-result.model.ts` — agrega `totalPaginas`, `tienePaginaAnterior`, `tienePaginaSiguiente`.
- `shared/models/parametros-consulta-paginada.model.ts` (nuevo) — `ParametrosConsultaPaginada`, `TAMANIO_PAGINA_DEFECTO = 20`, `OPCIONES_TAMANIO_PAGINA = [10,20,50]`.
- `core/utils/paginacion.util.ts` (nuevo) — `aHttpParams(p)`: arma el `params` de `HttpClient` omitiendo `busqueda`/`ordenarPor` vacíos.
- `shared/utils/estado-lista-paginada.ts` (nuevo) — `EstadoListaPaginada<T>`: signals + `cargar()/refrescar()/buscar()/cambiarPagina()/ordenar()`. Canal `switchMap` que cancela la petición anterior; `takeUntilDestroyed()` (instanciar en field initializer del componente).
- `shared/components/search-box/` (nuevo) — `<app-search-box>`: `mat-form-field` outline con ícono `search`, botón limpiar, `type="search"`. Emite `buscar` (string trimmeado) con `debounceTime` (350ms) + `distinctUntilChanged`; limpiar/Enter emiten de inmediato (canal único `Subject`).

**Frontend — por módulo:**
- Servicios: `buscar(p): Observable<PagedResult<T>>` en los 4. `Clientes.buscar` cambió de firma. `Sucursales.listar()` se conserva (lo usan `usuarios-list` y `UsuarioSucursalDialog`) y ahora hace `GET ?tamanioPagina=100` + `map(r => r.items)`.
- Listados: `<app-search-box>` reemplaza los filtros (Empresas/Usuarios/Sucursales no tenían ninguno), `<mat-paginator>` en los 4, `matSort` + `mat-sort-header` en columnas de la whitelist, estado vía `EstadoListaPaginada`. Vacío/error distinguen "sin resultados de búsqueda" de "aún no hay datos".

**Frontend — fix de formularios en modal (global, sin providers nuevos):**
- `src/styles.scss`: `.mat-mdc-dialog-panel { max-width: 94vw }`, `.mat-mdc-dialog-content { overflow-x: hidden }`, `.opt-form-grid` pasa a `repeat(auto-fit, minmax(min(100%, 15rem), 1fr))` (colapsa por ancho disponible, no por viewport) + `.opt-form-grid .mat-mdc-form-field { min-width: 0 }` (causa raíz del desborde). Se eliminó el `@media 599px` que forzaba 1 columna.
- `cliente-form.scss` `:host` 92vw->94vw; `empresa/usuario/sucursal-form.scss` `:host` `min-width: 360px` -> `width: min(480px, 94vw)`; `confirm-dialog.scss` (nuevo) `:host { max-width: 420px }`.
- **No se tocó `app.config.ts`** — `MAT_DIALOG_DEFAULT_OPTIONS` habría metido `@angular/material/dialog` en el bundle inicial (regla de `src/frontend/CLAUDE.md`). Todo el fix es CSS.

**Verificación:** backend `dotnet build OPT.sln` limpio. Frontend `npm run lint` limpio, `npm run build` sin warnings de budget, `npm test` **50/50** (47 previos + 3 de `search-box`; spec con `await setTimeout` real, no `fakeAsync` — proyecto zoneless sin `zone.js/testing`). **No verificado aún end-to-end contra `dbOPT_NET` ni en navegador real.**

**Rollout futuro:** OrdenesDeTrabajo / Inventario y todo listado nuevo siguen el mismo patrón — `Obtener{X}Query : ParametrosPaginacion`, `{X}Repositorio.BuscarPaginadoAsync` con whitelist `_orden`, handler con `PagedResultFactory.Crear`, controller `[FromQuery]`; frontend `servicio.buscar(p)` + `EstadoListaPaginada` + `<app-search-box>` + `<mat-paginator>` + `matSort`.

**Pendiente:** sincronizar `Manual_Tecnico_Backend_OPT.docx` y `Manual_Tecnico_Frontend_OPT.docx`; verificación end-to-end contra `dbOPT_NET`.

---

## 2026-08-27 — Fix modal Editar cliente + rediseño de la Ficha del cliente (continuación)

**Contexto:** tras la sesión de paginación, el usuario reportó que (1) el modal "Editar cliente" seguía viéndose mal — pidió compararlo con "Editar empresa", que sí se ve bien; y (2) la pantalla "Ver Ficha" del módulo Cliente no lo convencía — pidió revisar el legacy y proponer una visualización con UX/UI. Se levantó el stack real (backend en :63595, `ng serve` en :4200 con `--host localhost` — CORS solo permite `http://localhost:4200`, no `127.0.0.1`) y se validó todo en navegador contra `dbOPT_NET` (login RUT `1-9`, el usuario admin migrado).

### 1. Modal "Editar cliente" — causa raíz encontrada en vivo

Diagnóstico vía DevTools sobre el modal renderizado:
- `.opt-form-grid` **no era `display: grid`** — `getComputedStyle` daba `display: block`. Angular Material inyecta `.mat-mdc-dialog-content { display: block }` DESPUÉS de `styles.css`; misma especificidad (0,1,0) → gana Material. La grilla nunca funcionó como tal.
- `.mat-mdc-dialog-content { overflow-x: hidden }` (mío) tampoco aplicaba — Material define `overflow-x: auto` con la misma especificidad y va después.
- **El bug visible (scroll horizontal):** `cliente-form :host { width: min(680px, 94vw) }` ≈ 596px, pero Material limita el panel con `.cdk-overlay-pane.mat-mdc-dialog-panel { max-width: var(--mat-dialog-container-max-width, 560px) }` (especificidad 0,2,0). El formulario (596) > panel (560) → desborde. `empresa-form` se ve bien porque su `:host` es `min(480px, 94vw)` = 480 < 560.

**Solución (empresa como referencia = una sola columna):**
- `cliente-form.html`: se quitó `.opt-form-grid` y `.opt-col-2`. Formulario apilado + `<p class="opt-form-section">` por grupo (Identificación / Contacto / Ubicación). Para pares cortos (Nombre+Apellido, Fecha+Previsión, Región+Comuna) un `<div class="fila">` flex (`flex-wrap` + `.campo { flex: 1 1 200px }`) — colapsa solo en angosto.
- `cliente-form.scss`: `:host { width: min(520px, 94vw) }` (< 560, cabe sin desbordar).
- `src/styles.scss`: se **retiró** `.opt-form-grid` / `.opt-form-grid .mat-mdc-form-field` / `.opt-form-grid .opt-col-2` / `.mat-mdc-dialog-panel { max-width: 94vw }` (todos muertos por especificidad o ya innecesarios). Se dejó la red de seguridad `.mat-mdc-dialog-container .mat-mdc-dialog-content { overflow-x: hidden }` (2 clases → gana a Material). `.opt-form-section` se limpió (sin `grid-column`).
- Verificado en vivo: `pane.w = 520`, `content.overflowX = hidden`, sin scroll horizontal, título completo, `.fila` en 2-up.

### 2. Ficha del cliente (`cliente-ficha`) — rediseño "ficha clínica"

Legacy revisado: `old/.../Areas/Clientes/Views/FichaMedica/Details.cshtml` — panel "Datos del Paciente" fijo a la izquierda (tabla clave-valor + botón Editar) + card con pestañas Anamnesis / Recetas / OT a la derecha, cada una con "Nueva X" + tabla de historial. Es un buen patrón (ficha de paciente) que el Angular actual había perdido (tenía "Datos" como una pestaña más → identidad no persistente, mucho espacio muerto, receta renderizada como tabla inline frágil).

**Nuevo layout (`cliente-ficha.html/.scss/.ts`):**
- `app-page-header` (volver + nombre) full width.
- Grid `300px 1fr` (colapsa a 1 col bajo 900px):
  - **Izquierda — panel de identidad `position: sticky`**: `.opt-badge-clinico`, RUT en `.opt-mono`, `<dl>` con Edad (calculada de `fechaNacimiento`) + "· nac. dd-MM-yyyy", Previsión como chip, Teléfono/Email/Dirección, resumen "N anamnesis · M recetas", botón "Editar datos".
  - **Derecha — `mat-tab-group`**: pestañas Anamnesis / Receta de cristales con contador en la etiqueta. Cada panel: barra "N registros" + "Nueva X", luego tarjetas ordenadas por `fechaRegistro` desc.
- **Tarjeta de anamnesis:** fecha (mono) + editar/eliminar; 4 chips HTA/Diabetes/Alergias/"Usa lentes" con estado Sí/No (`.chip--si` resaltado en `tertiary-container` cuando presente); línea "Alergias: …" y observaciones si hay.
- **Tarjeta de receta:** fecha + chips Urgente (`.chip--alerta` = `mat-sys-error`) / Laboratorio; dos bloques **Lejos** / **Cerca** lado a lado (`flex-wrap`), cada uno una tabla mini OD/OI × Esf./Cil./Eje en `.opt-mono` alineada a la derecha, "—" para nulos; línea DP lejos / DP cerca / ADD; observaciones.
- Estados vacío/error por pestaña (`app-empty-state` + reintentar) — antes las cargas de sub-listas no tenían manejo de error.

**Cambio de backend (sin esquema):** `AnamnesisDto` y `RecetaCristalesDto` ganaron `DateTimeOffset FechaRegistro` desde `CreadoEn` (audit). La migración ya preservó la `FechaIngreso` legacy en `CreadoEn` (`Program.cs` líneas 362/388), así que las fechas de los registros históricos son reales. Tocados: los 2 DTOs, `RecetaCristalesMapper`, y los handlers de Anamnesis Crear/Actualizar/ObtenerPorId/ObtenerPorCliente. Modelos frontend `anamnesis.model.ts` / `receta-cristales.model.ts` + `RecetaCristalesFormulario` (nuevo `Omit` de `fechaRegistro`).

**Verificado en navegador (dbOPT_NET, cliente THOMAS MENZEL con 1 anamnesis + 2 recetas):** panel de identidad correcto (Edad 62, Previsión chip, resumen), pestañas con contador, tarjeta de anamnesis con chips Sí/No y fecha real `26-05-2025`, tarjetas de receta con Lejos/Cerca lado a lado y valores mono, sin scroll horizontal, sin errores de consola. Layout 2-col en desktop, 1-col + panel arriba en angosto.

**Verificación global:** backend `dotnet build OPT.sln --no-incremental` (0 errores, 2 warnings preexistentes de stubs). Frontend `npm run lint` limpio, `npm run build` sin warnings de budget, `npm test` **50/50**, `prettier --check` limpio.

**Pendiente:** verificación end-to-end de Empresas/Usuarios/Sucursales (paginación + guardado real); sincronizar `Manual_Tecnico_Backend_OPT.docx` (endpoints paginados + `PagedResult` + `FechaRegistro` en los DTOs clínicos) y `Manual_Tecnico_Frontend_OPT.docx` (`app-search-box`, `EstadoListaPaginada`, ficha clínica rediseñada, formularios de diálogo a una columna).

---

## 2026-08-27 — Migración de datos del módulo Comercial (OT, detalle, bitácora, abonos, pagos, cuotas)

**Alcance pedido por el usuario:** "migrar los datos de los módulos orden de trabajo, pagos, abonos, cuotas. **Solo base de datos**." Por eso esta fase **no** se hizo en `OPT.Migracion` (C#/Dapper) sino con dos scripts SQL. Es la primera fase de migración que no pasa por esa herramienta.

### Perfilado previo (todo verificado en vivo con `sqlcmd`)

Los datos legacy resultaron inusualmente limpios: 0 huérfanos en detalle/abono/pago/cuota/bitácora, 0 OT con RUT sin cliente, 0 OT sin sucursal/empresa/fecha, 0 `idOT` duplicados, `OPT_Producto.Codigo` único (4.677/4.677). Los catálogos `OPT_EstadoOT` (7) y `OPT_FormaPago` (5) tienen **ids idénticos** en ambas bases — el único caso hasta ahora donde no hay que remapear por nombre.

### 4 brechas de esquema resueltas con `AskUserQuestion`

| Brecha | Decisión |
|---|---|
| `OPT_Pago` (3.736) y `OPT_Cuota` (34.110) no tenían tabla destino | **Crear ambas** en el esquema nuevo, con forma `AuditableEntity` y `FormaPagoId` en vez del `TipoPago` texto libre |
| `OPT_Producto` vacío en destino, pero `DetalleOT.ProductoId` es FK `NOT NULL` | Migrar **solo los 4.018 productos usados** por algún detalle. Sin `ProductoSucursal` |
| `Saldo` legacy no descuenta `OPT_Pago` (3.472 OT infladas) | **Recalcular** `TotalAbonado = abonos + pagos`, `Saldo = Precio − TotalAbonado` |
| 7 campos de OT sin destino | Agregar `Beneficiario`, `FechaAtencion`, `HoraEntrega`, `NumeroCuotas`. Descartar `EstadoPago` y `FormaAbono` (derivables y sucios: 1.565 vacíos, 2.050 "SIN INFORMACION"). `Usuario` → `CreadoPor` |

### Artefactos

- **`src/basedatos/004_comercial_pagos_cuotas.sql`** (esquema, idempotente, aplicado): agrega `OPT_FormaPago (5, 'CHEQUE')`; crea el catálogo `OPT_EstadoCuota` (PENDIENTE/PAGADA/ANULADA); agrega las 4 columnas a `OPT_OrdenDeTrabajo`; crea `OPT_Pago` y `OPT_Cuota` con sus FKs e índices (`UQ_Cuotas_OrdenDeTrabajoId_Numero`). **Escrito a mano, no generado con `dotnet ef migrations script`** — el alcance era solo base de datos.
- **`src/basedatos/migracion/M004_datos_comercial.sql`** (datos, carpeta nueva): `INSERT ... SELECT` cross-database en **una sola transacción**, con guardas (base correcta, tablas de `004` presentes, `@Force = 0` aborta si ya hay OT) y una verificación dura que hace `ROLLBACK` si el mapeo ordinal de Empresa dejara de calzar por nombre.

### Resultado (ejecutado y verificado contra `dbOPT_NET`)

| Tabla | Filas | Esperado |
|---|---|---|
| `OPT_Producto` | 4.018 | 4.018 |
| `OPT_OrdenDeTrabajo` | 12.578 | 12.578 |
| `OPT_DetalleOT` | 20.573 | 20.573 |
| `OPT_BitacoraOT` | 31.964 | 31.964 |
| `OPT_Abono` | 3.277 | 3.277 |
| `OPT_Pago` | 3.736 | 3.736 |
| `OPT_Cuota` | 34.110 | 34.110 |

**Cuadratura de montos (idéntica al legacy):** Precio OT 1.431.504.714 · Abonos 246.687.201 · Pagos 293.944.093 · Cuotas 1.157.674.769 · Detalle (Cant×VU) 1.388.016.546.
**Por diseño NO cuadran:** `TotalAbonado` 540.631.294 (legacy 246.687.186) y `Saldo` 890.873.420 (legacy 1.185.383.027) — es el recálculo decidido arriba.

**Verificación fila a fila (todas dieron 0 discrepancias):** ClienteId ↔ RUT, SucursalId ↔ nombre, EmpresaId ↔ razón social, `EstadoOTId` ↔ última bitácora, producto del detalle ↔ código legacy, `FechaEntrega` = fecha + hora legacy, `FechaAtencion`/`HoraEntrega`/`NumeroCuotas`/`Beneficiario` idénticos, y match por tupla (OT, monto, fecha, forma de pago) para abonos, pagos, cuotas y bitácora.

### Deuda técnica que deja esta sesión

1. **`OPT.Domain` no tiene `Pago`, `Cuota` ni `EstadoCuota`**, y `OrdenDeTrabajo` no tiene las 4 propiedades nuevas → el modelo de EF Core y `dbOPT_NET` están **desalineados**. Hay que crear las entidades + `IEntityTypeConfiguration<T>` antes de regenerar cualquier script con `dotnet ef migrations script` (si no, EF va a querer volver a crear las tablas).
2. **`OPT.Migracion` no sabe** que el módulo Comercial ya está migrado.
3. **`ProductoSucursal` (13.776) y los 659 productos no usados** siguen pendientes.
4. `Diccionario_Datos_OPT.docx` ✅ actualizado en la misma sesión (23 tablas: nuevas secciones de `OPT_EstadoCuota`/`OPT_Pago`/`OPT_Cuota` en §4, las 4 columnas nuevas de OT, `CHEQUE` en §5.3, nueva §5.5, diagrama de §3, historial de scripts y pendientes de §7). `Manual_Tecnico_Backend_OPT.docx` ✅ también actualizado (v1.1): alcance, nota de entidades faltantes en 3.2.3, 7.1 (esquema vs. `migracion/`), 7.4 diagrama, 7.6 deuda de EF Core, 7.7 con `OPT_EstadoCuota`/`OPT_Pago`/`OPT_Cuota` y las 4 columnas de OT, 11 gotchas (+3), 12 ADRs (+`0006`) y 13.1/13.3 con el estado real y las cifras de verificación.

Gotchas de datos nuevos (collation distinta entre las dos bases, mapeo ordinal de Empresa, cuotas siempre PENDIENTE, saldo legacy inflado, `CHEQUE` fuera de catálogo, montos ≤ 0) documentados en `.agents/context/migracion-datos-legacy.md`.

### Documentación para agentes IA actualizada en esta sesión

- **`.agents/decisions/0006-modulo-comercial-pago-cuota-y-migracion-sql.md`** (nuevo) — las 4 decisiones, las alternativas descartadas y los 6 costos aceptados. Indexado en `.agents/decisions/README.md`.
- **`src/basedatos/README.md`** (nuevo) — estructura de la carpeta (esquema `00N_*.sql` vs. datos `migracion/M00N_*.sql`, tabla comparativa), estado de los 5 scripts y una lista explícita de **qué NO hacer** (no correr `migrations add` con el modelo desalineado, no re-ejecutar un `M00N`, no comparar texto cross-database sin `COLLATE`, no copiar ids de catálogo legacy).
- **`.agents/context/glosario-dominio.md`** — Abono/Pago/Cuota reescritos con lo que mostraron los datos reales; entradas nuevas de `EstadoCuota` y del valor `CHEQUE`; `Beneficiario` aclarado como texto libre.
- **`.agents/context/reglas-negocio-legado.md`** — el *[Reconsiderar]* de Abono vs. Pago quedó **resuelto**; el *[Mejorar]* del saldo pasó a confirmado con cifras; 2 reglas nuevas (bitácora sin estado anterior, `Usuario`/`Responsable` como texto libre).
- **`CLAUDE.md`** — 3 gotchas nuevos en la tabla, la regla de los dos grupos de scripts, y el índice de documentación al día.

---

## 2026-08-27 — Revisión de cobertura de `PublicId` (deuda técnica registrada)

**Origen:** el usuario preguntó por qué `OPT_OrdenDeTrabajo.OrdenDeTrabajoId` es `int` y no un GUID, si por seguridad no convendría exponer un identificador opaco. La respuesta corta es que el proyecto ya resolvió eso con el patrón `PublicId` del ADR `0004` (columna `Guid` al lado del `int IDENTITY`, que sigue siendo PK) — pero al validarlo contra `dbOPT_NET` apareció que **`OrdenDeTrabajo` no lo tiene**, y sí debería.

**Criterio aplicado** (hacen falta las dos condiciones): la entidad es un recurso direccionable de primer nivel en la API, **y** enumerar su id filtra datos personales, sensibles o financieros de una persona identificable.

**Revisión tabla por tabla de las 23 de `dbOPT_NET`:**

| Tabla | Veredicto |
|---|---|
| `Cliente`, `Empresa`, `Usuario`, `Anamnesis`, `RecetaCristales` | ✅ ya lo tienen |
| `OrdenDeTrabajo` (12.578) | ❌ **falta — prioridad alta** |
| `Abono` (3.277), `Pago` (3.736), `Cuota` (34.110) | ❌ falta — prioridad media |
| `DetalleOT` (20.573) | a evaluar al diseñar la API (si es agregado de OT, no hace falta) |
| `BitacoraOT`, `ProductoSucursal` | no corresponde — solo se acceden anidadas bajo un padre protegido |
| `Producto`, `Sucursal` | no corresponde — no son datos personales (`Sucursal` ya usa `Id` en su API por decisión de 2026-08-24) |
| 7 catálogos + `EmpresaSucursal` + `UsuarioSucursal` | no corresponde |

**Dos puntos que no hay que perder:**

1. **Agregar `PublicId` a `OrdenDeTrabajo` ANTES de publicar su primer endpoint.** Hacerlo después rompe URLs ya emitidas. El cambio es barato y no toca ninguna FK: columna + índice único, `UPDATE` para poblar las 12.578 filas migradas, propiedad en la entidad + su `IEntityTypeConfiguration<T>`, y controllers/DTOs resolviendo por `PublicId`.
2. **`NumeroOT` es visible por diseño** — va impreso en el ticket del cliente. Ahí `PublicId` no protege nada: quien conozca un número puede probar los vecinos. Esa superficie se cubre con **autorización por sucursal en el endpoint**. Son medidas complementarias, no alternativas.

**Descartado explícitamente:** cambiar la **clave primaria** a `Guid`. Obligaría a reescribir 5 tablas hijas con sus FKs e índices, el modelo de EF Core y la migración de datos ya ejecutada, sin ganar nada que `PublicId` no dé — y con costo de rendimiento (índice clustered aleatorio, 4× de espacio replicado en cada FK).

**Documentado en:** ADR `0004` (sección nueva "Revisión de cobertura de `PublicId` (2026-08-27) — deuda técnica abierta", con la tabla, el criterio y los costos), `CLAUDE.md` (regla de generación de código + pendiente n° 5), `Manual_Tecnico_Backend_OPT.docx` §7.5/7.6 y `Diccionario_Datos_OPT.docx` §7.

**Sin cambios de código ni de esquema en esta sesión** — es solo el registro de la deuda.

---

## 2026-08-27 — Verificación automática del Diccionario de Datos contra `dbOPT_NET`

En vez de volver a editar a mano, se contrastó el `.docx` completo contra la base con un script (extrae las tablas del documento con `python-docx` y las compara con `sys.columns` / `sys.indexes` / `sys.foreign_keys`):

| Qué se comparó | Resultado |
|---|---|
| Columnas (tipo + nulabilidad) | **145/145** — 0 diferencias |
| Índices no-PK | **43/43** — 0 diferencias |
| Claves foráneas + su `ON DELETE` | **27/27** — 0 diferencias |
| Catálogos sembrados (Rol, EstadoOT, FormaPago, CategoriaProducto, EstadoCuota) | coinciden valor por valor |

Único hallazgo: 4 filas del documento agrupan pares de columnas (`OdEsferaLejos / OdCilindroLejos`), por eso son 141 filas para 145 columnas. Es formato, no un error.

**Lo que sí faltaba y se agregó — sección 8 «Volumen de datos actual»:** conteo de filas por tabla al 2026-08-27 (138.380 filas en 23 tablas), con el origen de cada una y las observaciones relevantes (`OPT_Producto` parcial, `OPT_ProductoSucursal` en 0 y pendiente, `Usuario`/`Sucursal` con filas de prueba QA). Sirve para dimensionar consultas y decidir qué listados exigen paginación server-side — las 6 tablas sobre 10.000 filas quedan marcadas. Se agregó también la nota de verificación con las cifras de arriba y se amplió §1.4 para que la sección 8 se recalcule en cada mantención (si un conteo no calza, es señal de una migración a medias).

El resto del documento no requirió cambios: ya estaba correcto tras las actualizaciones de esta misma jornada.

---

## 2026-08-27 — API del módulo Comercial: OT, abonos, pagos, cuotas y flujo de estados

**Pedido:** crear las APIs de Orden de Trabajo, pagos, abonos, cuotas y su flujo de estados, considerando la deuda técnica de `OPT_OrdenDeTrabajo`.

Antes de escribir código se consultaron cinco decisiones con el usuario (todas registradas en el **ADR `0007`**): alcance de `PublicId`, reglas de transición de estados, cómo modelar la anulación, de dónde sale el `Precio` y cómo se relacionan pagos y cuotas.

### Base de datos — `005_ot_publicid_estado_anulado.sql` (aplicado y verificado)

- `OPT_OrdenDeTrabajo.PublicId` (`uniqueidentifier NOT NULL DEFAULT NEWID()` + `UQ_OrdenesDeTrabajo_PublicId`). Las 12.578 filas migradas quedaron pobladas: `12.578 PublicId distintos, 0 nulos` (verificado con `sqlcmd`).
- `OPT_EstadoOT (7, 'ANULADO')` — el catálogo pasa de 7 a 8 valores.

Escrito a mano, como `004`, pero esta vez **el modelo de EF Core queda alineado con la base**.

### Deuda técnica del ADR `0006` — cerrada

`OPT.Domain` ya tiene las entidades `Pago`, `Cuota` y `EstadoCuota` con sus `IEntityTypeConfiguration<T>` (espejo exacto de lo que creó `004`), más `Beneficiario`, `FechaAtencion`, `HoraEntrega` y `NumeroCuotas` en `OrdenDeTrabajo`, el seed de `CHEQUE` en `FormaPago` y el de `ANULADO` en `EstadoOT`. Se puede volver a usar `dotnet ef migrations add` siguiendo el procedimiento de `CLAUDE.md`.

### Dominio — la OT como agregado real

`OrdenDeTrabajo` es ahora la única puerta de entrada a detalles, abonos, pagos, cuotas y bitácora:

- **`Precio` derivado del detalle** (`AgregarDetalle` / `ReemplazarDetalles` lo recalculan), `TotalAbonado = abonos + pagos` y `Saldo = Precio − TotalAbonado`, siempre en la misma operación que el movimiento (ADR `0003`).
- **Sobrepago permitido** — se quitó la validación `monto > Saldo` del scaffold, por decisión del negocio.
- **`EstadosOT`** (clase de constantes + reglas): avance de a un paso, retroceso de a un paso con observación obligatoria, `ENTREGADO` y `ANULADO` terminales, sin saltos de etapa.
- **`Anular(motivo, usuario)`** — estado terminal, motivo obligatorio en bitácora, prohibido sobre una OT entregada. Reemplaza al `SP_OTEliminar` del legacy sin borrar ni ocultar la OT.
- **Cuotas**: `GenerarPlanCuotas` (N cuotas parejas, resto a la última, vencimiento mensual) y, sobre todo, **`RegistrarPago` imputa las cuotas pendientes más antiguas** que el monto cubra completas. Esto cierra el ciclo que el legacy dejaba abierto (sus 34.110 cuotas quedaron todas en PENDIENTE mientras los cobros vivían en `OPT_Pago`).

### API

`OrdenesDeTrabajoController` (`/api/ordenes-de-trabajo`), todo por `PublicId`:

| Verbo | Ruta | Qué hace |
|---|---|---|
| GET | `/` | Listado paginado (filtros: `clientePublicId`, `sucursalId`, `estadoOTId`, `soloConSaldo`; búsqueda por número de OT, beneficiario o RUT/nombre del cliente) |
| GET | `/{publicId}` | Vista completa: cabecera + detalle + abonos + pagos + cuotas + bitácora |
| POST | `/` | Alta con detalle, y opcionalmente plan de cuotas y abono inicial en la misma transacción |
| PUT | `/{publicId}` | Cabecera y, si viene, reemplazo del detalle completo |
| POST | `/{publicId}/estado` | Avanzar/retroceder una etapa |
| POST | `/{publicId}/anular` | Anular con motivo |
| POST | `/{publicId}/abonos` | Registrar abono inicial |
| POST | `/{publicId}/pagos` | Registrar cobro posterior (imputa cuotas) |
| POST | `/{publicId}/cuotas` | Generar plan de cuotas |
| POST | `/{publicId}/cuotas/{numero}/pagar` | Marcar cuota pagada (regularización manual) |
| POST | `/{publicId}/cuotas/{numero}/anular` | Anular cuota pendiente |

Más tres catálogos de solo lectura: `/api/estados-ot` (con `esTerminal`, para que el frontend no duplique las reglas de transición), `/api/formas-pago` y `/api/estados-cuota`.

Los hijos usan su Id interno: solo se alcanzan bajo la ruta de su OT, que ya está protegida (ADR `0004`, criterio del "padre protegido").

### Decisiones de implementación que vale la pena recordar

- **`OrdenDeTrabajoDtoFactory`** (registrado a mano en `AddApplication`, el escaneo de MediatR no lo alcanza): los nueve comandos y la consulta por Id devuelven exactamente la misma forma; duplicar ese mapeo en cada handler era la vía rápida a que se desincronizaran.
- **`RepositorioBase<T>.Contexto`** (nuevo): el listado de OT filtra por datos del `Cliente`, así que necesita el `DbContext` además del `DbSet`. Exponerlo en la base evita que cada repositorio derivado capture el parámetro del constructor primario (warning CS9107).
- **`ProductoRepositorio`** se implementó y registró ahora (`IProductoRepositorio` existía sin implementación): el detalle de la OT necesita validar que el producto exista.
- **`IClienteRepositorio.ObtenerPorIdsAsync`**: el listado resuelve los nombres de cliente con **una** consulta por página, no una por fila.

### Estado de verificación

- `dotnet build OPT.sln` — **0 errores, 0 advertencias nuevas** (el único warning es el stub preexistente de `InventarioController`). Nota: si `OPT.API` está corriendo, el build falla con `MSB3027` (archivo bloqueado); se compiló con `--artifacts-path` a un directorio temporal.
- Script `005` aplicado y verificado contra `dbOPT_NET` real.
- **Pendiente: verificación end-to-end de los endpoints** contra `dbOPT_NET` con sesión autenticada, y frontend del módulo (no había nada de Comercial en Angular).

### Pendientes que deja esta sesión

1. **Autorización por sucursal** en los endpoints de OT — hoy son `[Authorize]` genérico. Es la medida que el ADR `0004` señala para proteger la superficie de `NumeroOT`, que es visible por diseño.
2. Anular un abono o un pago ya registrado (hoy solo se anulan cuotas y la OT completa).
3. ~~Los `.docx` no se actualizaron~~ — hecho en la misma sesión (ver abajo).

### Manuales técnicos actualizados (misma sesión)

Editados con `python-docx` preservando formato (no hay pandoc ni LibreOffice en este equipo; ver la nota de entorno):

- **`Diccionario_Datos_OPT.docx`** — §1.2 (cinco scripts), §1.3 (ADRs `0006`/`0007`), §2.3 (`PublicId` en 6 tablas; la tabla de cobertura pasa de "PENDIENTE" a resuelto/no corresponde), §4 `OPT_EstadoOT` (ids 0-7) y `OPT_OrdenDeTrabajo` (título "Auditable + PublicId", fila `PublicId`, índice `UQ_OrdenesDeTrabajo_PublicId`), §5.2 (8 filas, con `ANULADO`), §6 (fila del script `005`), §7 (dos pendientes marcados RESUELTO) y §8 (`OPT_EstadoOT` 7→8, total 138.381, nota de verificación).
- **`Manual_Tecnico_Backend_OPT.docx`** — §1.2, §3.2.3 (entidades `Pago`/`Cuota`/catálogos/`EstadosOT`, OT como raíz del agregado, y el flujo de estados completo reemplazando el aviso de deuda), §3.3 (interfaces nuevas), **§4.3 subsección nueva "Comercial: Orden de Trabajo, abonos, pagos y cuotas"**, §5.1/§5.3 (repos nuevos, `RepositorioBase.Contexto`, registro de `OrdenDeTrabajoDtoFactory`), §6.2 (controllers: OT pasa de Stub a Implementado + los 3 catálogos), §7.1/§7.3/§7.5 (`PublicId` de la OT), §7.6 (las dos deudas marcadas RESUELTAS; autorización por sucursal y anulación de abonos/pagos como vigentes), §7.7 (`OPT_OrdenDeTrabajo` con `PublicId`, `OPT_EstadoOT` con 8 valores), §9.1 (regla nueva sobre máquinas de estado), §11 (3 gotchas) y §12 (ADR `0007`).

`Manual_Tecnico_Frontend_OPT.docx` y `Manual_Tecnico_UX_OPT.docx` **no** se tocaron: esta sesión no cambió el frontend.

---

## Sesión 2026-08-27 — Frontend del módulo Comercial (OT, Abonos, Pagos, Cuotas, Cobranza) + dos endpoints nuevos

Continuación directa de la sesión anterior (API del módulo Comercial, ADR `0007`): esa sesión dejó
el backend listo y **sin nada de frontend**. Esta lo construye completo, tomando el legacy
(`old/Fuente/OPT.Web/Areas/OrdenTrabajo` — Ingreso / Pago / Deuda — y `Areas/Flujo`) como guía de
alcance, no de arquitectura.

### Decisiones que tomó el usuario al inicio

Se le preguntaron tres cosas antes de escribir código (había un bloqueo real y dos alternativas de diseño):

1. **Selector de producto**: el alta de OT necesita `ProductoId` y `InventarioController` es un stub →
   se optó por **agregar `GET /api/productos`** (solo lectura) en vez de recortar el alta.
2. **Estructura de pantallas**: **pantallas separadas como el legacy** (`/ordenes-de-trabajo`,
   `/abonos`, `/pagos`, `/cuotas`), no todo dentro de la ficha de la OT.
3. **Cobranza**: **pantalla propia**, lo que obligó a un endpoint de agregación nuevo.

### Backend agregado (mínimo para desbloquear el frontend)

- `ProductosController` → `GET /api/productos` paginado (`busqueda` contra código y descripción, orden
  por `codigo|descripcion`), con `IProductoRepositorio.BuscarPaginadoAsync` + `ObtenerProductosQuery`.
  **No** abre el resto de Inventario: stock y traslados siguen sin casos de uso.
- `CobranzaController` → `GET /api/cobranza/deudores`: deuda vigente agrupada por empresa convenio
  (`Saldo > 0` y estado ≠ ANULADO), ordenada de mayor a menor saldo. La agrupación la hace la base
  (`ObtenerDeudaPorEmpresaAsync` con `GroupBy` traducido a SQL), no la capa de aplicación. Las OT
  particulares llegan como una fila con `empresaPublicId = null`.
- `ObtenerOrdenesDeTrabajoQuery` ganó el filtro **`empresaPublicId`** (y el repositorio su parámetro
  `empresaId`): es el "detalle del deudor", que deliberadamente **no** tiene endpoint propio — sería
  duplicar la proyección de la OT en dos lugares.
- `ResumenDeudaEmpresa` es un `record` de agrupación en `Domain/Entities/Comercial/`, no una entidad
  (no hereda de `AuditableEntity`).

### Frontend

| Ruta | Pantalla | Reemplaza en el legacy a |
|---|---|---|
| `/ordenes-de-trabajo` | Listado paginado + filtros (estado, solo con saldo, contexto por query param) | `Ingreso/Index` (que tenía 3 campos de filtro sueltos) |
| `/ordenes-de-trabajo/nueva` y `/:publicId/editar` | Formulario con detalle editable y autocompletado de cliente / empresa / producto | `Ingreso/Create` (asistente de 4 pestañas) |
| `/ordenes-de-trabajo/:publicId` | **Ficha de la OT**: cabecera fija + barra del flujo + pestañas Detalle / Abonos / Pagos / Cuotas / Bitácora | `Flujo/_ModalEnviarFlujo` + `Flujo/_ModalBitacora` |
| `/abonos` | Elegir OT y registrar abono inicial | pestaña "Abonos" de `Ingreso/Create` |
| `/pagos` | Elegir OT con saldo y cobrar | `Pago/Index` |
| `/cuotas` | Generar plan, marcar cuota pagada, anular cuota | (no existía: el legacy definía el plan al crear y nunca lo administraba) |
| `/cobranza` | Deudores por empresa + totales | `Deuda/Index` (`sp_ListaDeudores`) |

Diferencias deliberadas respecto al legacy, todas por decisiones ya tomadas en el esquema/dominio:

- **El formulario de OT no incluye la receta**: `RecetaCristales` cuelga del Cliente, no de la OT.
- **No pide N° de OT ni precio**: el correlativo lo genera la BD y el precio es la suma del detalle.
- **El flujo de estados no es un `<select>` con todos los estados**: solo avanzar/retroceder una etapa,
  que es lo único que acepta `OrdenDeTrabajo.CambiarEstado`. Retroceder pide observación y anular pide
  motivo (`MotivoDialog` nuevo — `ConfirmDialog` no sirve porque el backend devuelve 422 sin texto).
- **Anular ≠ eliminar**: la OT anulada sigue en el listado, con chip gris tachado.
- **No hay "Pago Masivo"**: no existe endpoint que cobre varias OT a la vez.

Piezas transversales nuevas (reutilizables fuera de Comercial):

- `shared/pipes/pesos-pipe.ts` — formato CLP con `Intl.NumberFormat('es-CL')`. Se prefirió al
  `CurrencyPipe` de Angular porque este exige registrar los datos de locale en el bundle inicial y sin
  eso imprime `CLP12,578` (separador equivocado para Chile).
- `shared/utils/fechas.util.ts` — `DateOnly` / `DateTimeOffset` / `TimeOnly` desde el datepicker.
  `toISOString()` a secas corre la fecha un día en Chile (UTC-3/-4): ese es el bug que evita.
- `shared/components/motivo-dialog/` — confirmación **con texto obligatorio**.
- `features/ordenes-de-trabajo/components/` — `estado-ot-chip`, `resumen-financiero`, `selector-orden`
  (el buscador de OT que comparten Abonos/Pagos/Cuotas).
- Token de marca nuevo `--opt-estado-ot-anulado` (gris `#D6D3D1` sobre `#3F3A37`, contraste 7,5:1),
  documentado en `.agents/context/branding-ux-ui.md`: **gris, nunca rojo** — anular es una operación
  legítima del mesón, no un error — y con tachado, para no depender solo del color.

Los cuatro módulos de dinero importan el modelo y el servicio de `features/ordenes-de-trabajo`: en la
API no existe `/api/pagos` ni `/api/cuotas` (son subrecursos de la OT, ADR `0007`). Y como todo comando
del agregado devuelve la OT completa recalculada, las pantallas hacen `orden.set(respuesta)` en vez de
releer.

### Estado de verificación

- Backend: `dotnet build OPT.sln --artifacts-path <temp>` limpio (hubo que usar `--artifacts-path`
  porque `OPT.API` estaba corriendo y bloqueaba los `.dll` — gotcha ya documentado).
- Frontend: `npm run build` OK, `npm run lint` OK, `npm test` **77/77** (54 archivos).
- **Pendiente: verificación en navegador contra `dbOPT_NET` con sesión autenticada.** Ojo: la instancia
  de `OPT.API` que estaba corriendo durante la sesión es anterior a estos cambios — hay que reiniciarla
  para que exponga `/api/productos` y `/api/cobranza`.
- **Pendiente: `Manual_Tecnico_Frontend_OPT.docx` y `Manual_Tecnico_UX_OPT.docx`** — esta sesión no los
  tocó (sí se actualizaron `CLAUDE.md` raíz, `src/frontend/CLAUDE.md` y `branding-ux-ui.md`).

---

## 2026-08-28 — Vista "Ver Orden" del módulo Comercial y listado de OT sin carga inicial

**Resumen:**
- El negocio pidió conservar el formulario "Ver Orden" del legacy (modal `Ingreso/_ModalDetalle`) con sus cuatro pestañas y, además, que el listado de OT no traiga datos hasta que se busque (en el legacy traer todo daba timeout). Se preguntaron las tres decisiones que cambiaban el alcance antes de tocar código; el usuario eligió: vincular la receta a la OT como el legacy (no aproximarla), rediseñar la ficha ruteada (no un modal aparte) y aclaró que "Lentes" del legacy es simplemente el **Detalle** (el nombre del legacy no es el correcto).
- **Esquema (`006_receta_ot_detalle_comentario.sql`, escrito a mano, aplicado y verificado):** `OPT_RecetaCristales.OrdenDeTrabajoId` (`int NULL` + FK `FK_RecetasCristales_OrdenesDeTrabajo` + índice) y `OPT_DetalleOT.Comentario` (`nvarchar(200)`). No agrega tablas: siguen siendo 23.
- **Backfill (`migracion/M006_backfill_receta_ot_detalle_comentario.sql`):** 12.574 recetas vinculadas a su OT y 11.168 comentarios de línea — ambos exactamente lo esperado. El mapeo receta legacy→destino es **ordinal** y está justificado: `OPT.Migracion` insertó las recetas `ORDER BY idRecetaCristales` sin saltarse ninguna (13.183 = 13.183), y el script lo verifica fila a fila contra `ClienteId` y `FechaIngreso` antes de escribir (0 desalineadas). El comentario del detalle se re-derivó por `(OT, Producto, Cantidad, ValorUnitario)` con `ROW_NUMBER` de desempate, porque M004 no conservó el `idOTDetalle` legacy.
- **Dominio/API:** `RecetaCristales.OrdenDeTrabajoId` + `AsociarAOrden(...)`; `DetalleOT.Comentario`; `OrdenDeTrabajoDto` gana `Cliente` (`ClienteOTDto`, con comuna y región resueltas por el backend) y `Recetas`; `DetalleOTDto`/`LineaDetalleOTDto` ganan `Comentario`; `Crear`/`ActualizarOrdenDeTrabajoCommand` aceptan `RecetaPublicId`, validando que la receta sea del mismo cliente. `IComunaRepositorio.ObtenerPorIdAsync` e `IRecetaCristalesRepositorio.ObtenerPorOrdenAsync` son nuevos.
- **Frontend:** ficha de la OT rediseñada al layout del legacy (cabecera arriba + pestañas Cliente / Receta / Detalle / Abonos / Pagos / Cuotas / Bitácora); listado con estado inicial "Busca una orden de trabajo" y consulta diferida; formulario con selector de receta del cliente y columna Comentario en el detalle. Se extrajo `features/receta-cristales/components/receta-graduacion/` (la tabla de graduación estaba duplicada en la ficha del cliente) y se promovieron las clases `.opt-chip*` a `styles.scss` — esto último además resolvió el warning de budget de 4 kB que quedó al crecer el SCSS de la ficha.

**Decisiones tomadas:**
- La receta se **vincula** a la OT, no se copia ni se aproxima por fecha: es la prescripción con la que se fabricaron esos cristales. Sigue colgando del Cliente (la FK a la OT es nullable) y la relación es 1:N, porque 2 órdenes migradas tienen dos recetas.
- `RecetaPublicId` en la edición es **estado final**, como `EmpresaPublicId`: si no viene, la orden queda sin receta. El formulario reenvía la actual; queda anotado como gotcha en `CLAUDE.md`.
- Se agregó `DetalleOT.Comentario` aunque el usuario no lo respondió explícitamente: ya se estaba tocando el esquema y el legacy tiene el dato poblado en 11.168 líneas. Se le informó para que lo revierta si no lo quiere.
- Los hijos de la OT siguen sin `PublicId` (ADR `0007`); la receta sí lo tiene, porque es un recurso clínico propio del cliente.

**Verificación:**
- `dotnet build OPT.sln` limpio (con `--artifacts-path`: la API del usuario estaba corriendo y bloqueaba `bin/`).
- `npm run lint` / `npm run build` (sin warnings de budget) / `npm test` 80/80.
- Datos contrastados en SQL contra las capturas del legacy que envió el usuario — OT 17067: receta OD -1.25 / OI -2.00 / DP 64 / obs BLU, detalle "OTRO-LENTE-DE-LEJOS--NA" con comentario "FORMOSA F4 C2", abono 20.000.
- **No verificado por HTTP con sesión autenticada:** los usuarios migrados conservan la clave de 4 caracteres del legacy y `LoginCommandValidator` exige 6, así que hoy no hay credenciales usables en `dbOPT_NET`.

**Actualización de documentación (misma sesión, a pedido del usuario):**
- **ADR `0008`** — *Receta vinculada a la Orden de Trabajo y vista "Ver Orden"*: registra las tres decisiones de alcance con sus alternativas descartadas (aproximar la receta por fecha, agregar un modal aparte, mantener la carga inicial del listado) y los costos aceptados. Registrado en `.agents/decisions/README.md`.
- **`AGENTS.md`** — la línea de estado estaba congelada en el 2026-08-26 (decía 20 tablas, OT/Inventario stub, frontend sin consumir el backend). Reescrita al estado real, más la regla crítica 12 (`OrdenDeTrabajo` en la lista de `PublicId`, subrecursos con Id interno), dos entradas "Resuelto" (2026-08-27 y 2026-08-28) y la tabla de referencias (ADRs `0001`-`0008`, `migracion-datos-legacy.md`, `src/basedatos/README.md`).
- **`.agents/context/glosario-dominio.md`** — "Receta de Cristales" y "Detalle de OT" al día (vínculo con la OT, comentario de línea, y que "Lentes" del legacy es el Detalle).
- **`.agents/context/reglas-negocio-legado.md`** — tres reglas nuevas en "Orden de Trabajo" (listado sin carga inicial como defensa contra el timeout; la receta es la de *esa* orden; el comentario del armazón) y una en "Clientes y ficha clínica".
- **`src/frontend/README.md`** — el árbol de `features/` y la sección de decisiones seguían diciendo que Órdenes de Trabajo e Inventario eran placeholders.
- **`src/documentos/README.md`** — descripciones al día y nota de que los ADRs `0004`-`0008` no provienen de la propuesta de arquitectura.
- **`.docx`**: `Diccionario_Datos_OPT` (las 2 columnas nuevas con su índice y FK, y el script `006` en §6), `Manual_Tecnico_Backend_OPT` (§4.3 validación de `RecetaPublicId`, §6.2 forma nueva del DTO, §7.6 nota de los scripts escritos a mano, §7.7, §11, §12 ADR `0008`, §13.3 backfill `M006`) y `Manual_Tecnico_Frontend_OPT` (§1.2 alcance, §2.3 árbol de rutas real, §3.3, §5.6, **§6 reescrita** de "módulos placeholder" a módulo Comercial completo, §11, §12, §13).

**Próximos pasos sugeridos:**
1. Crear un usuario de desarrollo con clave válida (≥6) para poder verificar la API end-to-end; hoy es el único bloqueo para cerrar la verificación de todo el módulo Comercial.
2. Revisar en navegador la ficha rediseñada y el listado diferido con datos reales.
3. `Manual_Tecnico_UX_OPT.docx` sigue en v1.0 y no refleja las pasadas de UX/UI v2.0/v2.1 ni la ficha de la OT — es el único documento de `src/documentos/` que quedó desalineado.

---

## 2026-08-28 (2ª sesión) — Asistente de "Nueva orden de trabajo" a la altura del legacy

**Resumen:**
- Se contrastó el formulario de alta actual contra `Areas/OrdenTrabajo/Controllers/IngresoController.cs` y sus cinco partials. El legacy hacía tres cosas que la migración había perdido: dar de alta al **cliente** dentro del propio ingreso (`ClienteInsertar`), **tomar la receta** ahí mismo (pestaña Receta) y cerrar con una pantalla de **confirmación + impresión del ticket** (`Finaliza` → `Areas/Imprimir/rptTicketOT.rdlc`). Además exigía plan de cuotas cuando quedaba saldo.
- El usuario eligió recuperar las cuatro capacidades, con layout de **stepper** y **reusando las APIs existentes** (sin endpoint transaccional nuevo).

**Cambios (solo frontend, sin tocar backend ni base de datos):**
- `orden-de-trabajo-form` reescrito como `mat-stepper` de 4 pasos — Cliente / Receta / Detalle / Pago — que es la secuencia de las 4 pestañas del legacy. Lineal al crear, no lineal al editar (donde el paso Pago no existe: el dinero de una OT ya creada se maneja desde su ficha). El `form` único se separó en `formCliente` / `formOrden` / `formPago` para poder usarlos como `stepControl`.
- **Cliente inline**: cuando la búsqueda no arroja resultados aparece "Crear cliente", que abre el diálogo `ClienteForm` ya existente con el RUT tecleado precargado (`ClienteFormDialogData.rutInicial`, campo nuevo); al cerrar, el cliente creado queda seleccionado. El cliente elegido se muestra en una ficha resumida con botón "Editar datos" — el legacy también reescribía sus datos al guardar la OT.
- **Receta inline**: botón "Tomar receta nueva" que abre `RecetaCristalesForm` con el `clientePublicId`; la receta creada se agrega al listado y queda seleccionada. La elegida se previsualiza con `app-receta-graduacion`.
- **Regla de cuotas**: validador de grupo en `formPago` — si tras el abono queda saldo hay que indicar cuotas + primer vencimiento, o marcar "Sin plan de cuotas (cobro directo)". El legacy lanzaba excepción sin alternativa; el sistema nuevo sí permite cobrar con pagos sueltos, así que se pide confirmarlo en vez de bloquearlo.
- **Cierre + ticket**: `components/orden-creada-dialog/` (equivale a `Finaliza.cshtml`) con el ticket a la vista y los botones Imprimir / Nueva OT / Ver orden. "Nueva OT" reinicia el asistente sin recargar. `components/ticket-ot/` es el comprobante imprimible (reemplaza al `.rdlc`: N° de OT, cliente, fechas, detalle, receta, totales y plan de cuotas). Las reglas de `@media print` van en `styles.scss` (un `@media print` encapsulado por componente no puede ocultar el resto de la app): quien imprime marca el `<body>` con `opt-imprimiendo` y el overlay de Material se devuelve al flujo del documento.

**Decisiones tomadas:**
- El alta de cliente y de receta **reusa los diálogos existentes** en vez de duplicar sus campos dentro del stepper: `ClienteForm` ya trae la cascada Región→Comuna y sus validaciones, y `RecetaCristalesForm` las tablas de graduación. Consecuencia aceptada: cliente y receta se persisten antes que la OT, así que si el POST de la orden falla quedan creados — son datos válidos por sí mismos y es exactamente lo que hacía el legacy.
- Lo que la migración ya mejoró **no se revierte**: el N° de OT lo sigue generando la base de datos (el legacy lo tecleaba el operador y validaba duplicados con `Existe`) y el precio sigue siendo la suma del detalle.

**Verificación:**
- `npm run build` limpio (sin warnings de budget), `npm run lint` OK, `npm test` **89/89** (80 previos + 9 nuevos: ticket, diálogo de cierre, regla de cuotas y detección del RUT tecleado).
- **No verificado en navegador con sesión autenticada** — sigue el bloqueo de credenciales de la sesión anterior (usuarios migrados con clave de 4 caracteres, `LoginCommandValidator` exige 6).

**Actualización de documentación (misma sesión, a pedido del usuario):**
- **ADR `0009`** — *Alta de Orden de Trabajo como asistente por pasos: cliente y receta en línea, y ticket imprimible*: registra las cuatro capacidades del legacy recuperadas, las alternativas descartadas (página única / pestañas sin orden; endpoint transaccional; bloqueo duro de cuotas) y los costos aceptados (alta no atómica, impresión sin cobertura de tests). Registrado en `.agents/decisions/README.md`.
- **`AGENTS.md`** — línea de estado al día y entrada "Resuelto (2026-08-28, 2ª sesión)" con la regla de contexto que dejó: **el contrato de la API no es el flujo de trabajo** — antes de dar por terminada una pantalla migrada, recorrer el controller del legacy completo y preguntarse qué podía hacer el operador ahí que ahora ya no puede.
- **`.agents/context/reglas-negocio-legado.md`** — seis reglas nuevas en "Orden de Trabajo": el ingreso por pasos, el alta de cliente dentro del ingreso, la toma de receta, la pantalla de cierre con ticket, la exigencia de cuotas (con su matiz) y la hora de entrega fija en 12:00 del legacy, que se mejora.
- **`.agents/context/glosario-dominio.md`** — entrada "Ticket de OT".
- **`.agents/context/branding-ux-ui.md`** — sección "Asistentes por pasos e impresión" (patrón de stepper, reuso de diálogos para entidades relacionadas y el contrato completo de `@media print`), más su línea en "Cómo aplicar este contexto".
- **`CLAUDE.md` raíz, `src/frontend/CLAUDE.md`, `src/frontend/README.md`, `src/documentos/README.md`** — estado del módulo Comercial, patrones nuevos y rango de ADRs (`0001`-`0009`).
- **`.docx`**: `Manual_Tecnico_Frontend_OPT` (§1.2 alcance, §3.5 tabla de features —`ordenes-de-trabajo/` seguía marcado "Placeholder" y faltaban `abonos/pagos/cuotas` y `cobranza`—, **§6.3 reescrita** como asistente por pasos, **§6.4 nueva** de cierre/ticket/impresión, §6.5 piezas compartidas, §9.3 testing, §11 tres gotchas nuevos, §12 ADR `0009`, §13 dos pasos nuevos) y `Manual_Tecnico_Backend_OPT` (§12 ADR `0009` — registra que la decisión fue **no** agregar un endpoint transaccional, y cuál sería la salida si el negocio pidiera atomicidad; metadatos a v1.2).
- **Sin tocar**: `Diccionario_Datos_OPT.docx` (no hubo cambios de esquema) y `Manual_Tecnico_UX_OPT.docx`, que sigue en v1.0 y es el único documento desalineado de `src/documentos/`.

**Próximos pasos sugeridos:**
1. Crear el usuario de desarrollo con clave válida y recorrer el asistente completo en navegador, incluida la impresión del ticket (el `@media print` es lo único que no se puede verificar con tests).
2. Evaluar reutilizar `app-ticket-ot` desde la ficha de la OT, para poder reimprimir el comprobante de una orden ya creada.
3. `Manual_Tecnico_UX_OPT.docx` sigue en v1.0: no refleja las pasadas de UX/UI v2.0/v2.1, la ficha de la OT ni los patrones de stepper e impresión. Es el único documento de `src/documentos/` desalineado.

---

## 2026-09-08 — Seis observaciones sobre la ficha/alta de OT: propuesta, luego implementación

**Resumen:**
- El usuario levantó seis observaciones puntuales sobre el módulo Orden de Trabajo: (1) una OT `Entregado` debía dejar de ser editable sin aviso visible; (2) faltaban campos de cabecera (N° OT, fecha de atención con default hoy, fecha/hora de entrega, empresa convenio); (3) faltaba "Beneficiario" en la pestaña Cliente; (4) la pestaña Receta necesitaba un combo con la receta más reciente (últimos 3 meses) y el botón "Guardar" de Nueva Receta quedaba deshabilitado; (5) "Beneficiario" debía mudarse de Detalle a Cliente; (6) el paso Pago necesitaba una mejora visual con tres modalidades explícitas (pagar el total / abonar y financiar el resto en cuotas / pagar todo en cuotas) y una previsualización del calendario de cuotas.
- Primera pasada: análisis contra el código real (no contra supuestos) y propuesta en ADR `0010`, estado "Propuesta". Dos preguntas se resolvieron con `AskUserQuestion`: la ventana de 3 meses se cuenta **desde hoy**, no desde la fecha de atención; y las observaciones OD/OI/DP de Lejos/Cerca en Receta se vuelven **opcionales** (eran la causa del botón "Guardar" bloqueado).
- Segunda pasada, a pedido explícito del usuario ("Genera los cambios... Cuatas mensuales como esta hoy"): implementación real en frontend y backend, confirmando que las cuotas siguen siendo **mensuales** (`AddMonths`), no de 30 días fijos.

**Hallazgo:** el punto (1) ya estaba resuelto en el dominio (`OrdenDeTrabajo.GarantizarModificable` bloquea toda mutación en `Entregado`/anulada) — faltaba solo comunicarlo en la UI, no reimplementar la regla.

**Cambios (frontend):**
- `orden-de-trabajo-ficha.html`/`.scss`: aviso `.aviso--bloqueo` (ícono + texto) cuando `estaAnulada()` o `esTerminal()`.
- `orden-de-trabajo-form.ts`/`.html`/`.scss`: cabecera (`fechaAtencion`, `fechaEntrega`, `horaEntrega`, `beneficiario`) movida de `formOrden`/Detalle al paso Cliente (`formCliente`); paso Receta con combo `recetasRecientes` (últimos 3 meses, preseleccionado al crear) + "Ver historial completo" para el `mat-radio-group` de siempre; paso Pago rediseñado con `<app-resumen-financiero>`, `formPago.modalidadPago` (`'total' | 'abonoCuotas' | 'cuotas' | null`, **nulo y `Validators.required` por defecto** — no un valor "útil" que hubiera roto el spec de plan de cuotas obligatorio), un `effect()` que sincroniza qué controles quedan habilitados según la modalidad, y `cuotasPreview` (`computed()`) que replica la fórmula exacta de `GenerarPlanCuotas` del backend para mostrar la tabla de cuotas antes de guardar. Se retiró el checkbox "Sin plan de cuotas".
- `receta-cristales-form.ts`: `alternarLejos()`/`alternarCerca()` dejaron de agregar/quitar `Validators.required` a las 6 observaciones — solo enable/disable.

**Cambios (backend) — hallazgo al documentar, no pedido explícitamente por el usuario:**
- `CrearRecetaCristalesCommandValidator`/`ActualizarRecetaCristalesCommandValidator` todavía exigían esas 6 observaciones con `NotEmpty().When(x => x.IncluirLejos/IncluirCerca)` (agregadas en el script `007` de una sesión previa del mismo día). El cambio de frontend por sí solo habría dejado el botón habilitado pero la API habría seguido rechazando con 422 — una regresión peor que el bug original (botón visiblemente deshabilitado vs. guardado que falla en silencio). Se quitaron las 6 reglas `NotEmpty` de ambos validadores; se mantiene `MaximumLength(50)`. Sin cambios de esquema.

**Verificación:**
- No se pudo compilar `OPT.sln` (`dotnet` no está instalado en el entorno donde se hizo el cambio) ni correr `ng test` (Vitest no devolvió resultado dentro del tiempo disponible) — ambas son limitaciones del entorno de ejecución remoto, no del código. Se verificó manualmente, trazando el código contra los tests existentes de `orden-de-trabajo-form.spec.ts`, que el diseño de `modalidadPago` (nulo por defecto) preserva el comportamiento esperado por `exige un plan de cuotas cuando queda saldo...` y `pide el primer vencimiento cuando se indican cuotas`.
- **Pendiente antes de desplegar:** `dotnet build OPT.sln` y `ng test` en un entorno con esas herramientas disponibles.

**Actualización de documentación (misma sesión, a pedido del usuario):**
- **ADR `0010`** — *Mejoras a la ficha/alta de Orden de Trabajo: bloqueo de edición, cabecera, receta reciente y rediseño de Pago*: registra las seis observaciones, las alternativas consideradas, la decisión tomada punto por punto, las dos preguntas resueltas con el usuario y, agregada después, la corrección de backend. Estado pasó de "Propuesta" a "Aceptada" tras la implementación. Registrado en `.agents/decisions/README.md`.
- **`AGENTS.md`** — línea de estado al día, entrada "Resuelto (2026-09-08)" con la regla de contexto que dejó: **una regla de UI y su regla de validación en el backend son la misma regla** — no basta con corregir un lado. Rango de ADRs `0001`-`0010`.
- **`CLAUDE.md` raíz** — nota sobre la relajación de las 6 observaciones junto a la descripción del script `007`; extensión del párrafo largo de Comercial con los cambios de ADR `0010`; bullets de `Documentos técnicos` y rango de ADRs actualizados.
- **`src/frontend/CLAUDE.md`** — bullets nuevos en "Módulo Comercial" documentando el combo de receta reciente, la cabecera en el paso Cliente y el patrón de `modalidadPago` con su `effect()`/`cuotasPreview`.
- **`src/frontend/README.md`** — bullet del asistente de alta extendido con los cambios de ADR `0010`.
- **`src/documentos/README.md`** — descripciones de los dos manuales tocados al día; rango de ADRs `0004`-`0010`.
- **`.agents/context/reglas-negocio-legado.md`** — la regla de "Sin plan de cuotas" actualizada (checkbox retirado en favor de las tres modalidades); regla nueva en "Clientes y ficha clínica" documentando que las 6 observaciones de Receta pasaron a ser opcionales en ambos lados.
- **`.agents/context/branding-ux-ui.md`** — tres filas nuevas en "Patrones de UI agregados" (aviso de bloqueo, combo + historial colapsable, tarjetas de modalidad excluyente) y su línea en "Cómo aplicar este contexto".
- **`.docx`**: `Manual_Tecnico_Backend_OPT` (nota de la relajación del validador y ADR `0010` en su sección de ADRs) y `Manual_Tecnico_Frontend_OPT` (sección del módulo Comercial extendida con los tres patrones nuevos del alta/ficha de OT).
- **Sin tocar**: `Diccionario_Datos_OPT.docx` (no hubo cambios de esquema) y `Manual_Tecnico_UX_OPT.docx`, que sigue en v1.0 y es el único documento de `src/documentos/` desalineado.

**Próximos pasos sugeridos:**
1. Compilar `OPT.sln` y correr `ng test`/`npm run build`/`npm run lint` en un entorno con `dotnet` y sin el bloqueo de Vitest, antes de desplegar.
2. Recorrer en navegador el paso Pago con las tres modalidades y confirmar visualmente la tabla de cuotas previsualizada contra la que genera el backend al guardar.
3. `Manual_Tecnico_UX_OPT.docx` sigue en v1.0: no refleja ninguna de las pasadas de UX/UI posteriores. Sigue siendo el único documento de `src/documentos/` desalineado.

---

## 2026-09-08 (2ª sesión) — UX/UI del diálogo de Receta de cristales

**Resumen:** dos pedidos puntuales del usuario sobre `RecetaCristalesForm`, resueltos en la misma sesión, sin ADR (mejoras de UI/UX y un ajuste de comportamiento menor, no decisiones de arquitectura).

**Pedido 1 — "el modal es muy pequeño" + DP/ADD numéricos + auto-check de Cerca:**
- El diálogo pasó de `min-width: 560px` (con las tablas Lejos/Cerca apilándose por falta de espacio) a un ancho explícito de 960px, coordinado entre el `:host` del componente y la config de las 3 llamadas a `dialog.open(...)` (`cliente-ficha.ts` ×2, `orden-de-trabajo-form.ts`) — el CDK de Material limita a `80vw` por defecto si no se pasa `width`/`maxWidth` en el `.open()`, aunque el `:host` pida más.
- DP y ADD pasaron a ser inputs numéricos (`type="number"`), igual que Esférico/Cilíndrico/Eje. La API sigue recibiendo esos tres campos como `string` (sin cambio de esquema — el legacy los guardaba como texto libre sin formato fijo); la conversión número↔texto pasa solo al guardar/cargar. Riesgo aceptado y documentado en el código: una receta migrada con un DP en formato no numérico (p. ej. "31/30") se ve vacía en ese campo al editarla.
- Al ingresar un ADD > 0, "Incluir Cristales Cerca" se marca automáticamente y sus inputs quedan habilitados. El legacy ya recalculaba los valores de Cerca a partir de ADD, pero no marcaba el checkbox — el operador tenía que acordarse de tildarlo a mano.

**Pedido 2 — mover el check "Incluir Cerca" junto a sus inputs + mejorar los checks:**
- Los checks "Incluir Cristales Lejos"/"Incluir Cristales Cerca" se mudaron de una fila aparte (junto a "Urgente"/"Cristales de laboratorio", sin relación directa) a la `<caption>` de su propia tabla — el checkbox pasa a ser el título del bloque Lejos/Cerca en vez de una opción más en una lista.
- Se agregó feedback visual: cada tabla se atenúa (opacidad + sin borde) mientras su check está desmarcado, con un signal (`toSignal(control.valueChanges, { initialValue: … })`) que sincroniza una clase `.tabla-receta--activa` — antes la única señal era el gris nativo de los inputs deshabilitados, poco notorio.
- "Cristales de laboratorio"/"Urgente" quedaron agrupados en una franja con fondo propio para distinguirlos de los checks que sí activan/desactivan un bloque.

**Patrones nuevos, reusables en cualquier diálogo similar:** ancho explícito de diálogo coordinado `:host` + `.open()`, y checkbox-como-título-de-bloque con signal de atenuación — documentados en `.agents/context/branding-ux-ui.md` ("Patrones de UI agregados") y `src/frontend/CLAUDE.md` (sección nueva "RecetaCristalesForm — diálogo con tablas anchas").

**Verificación:** `tsc --noEmit` limpio y `npx sass` compila el SCSS sin errores. No se pudo correr `ng build` completo (bloqueo de red hacia Google Fonts en este entorno, no relacionado con el cambio) ni `ng test` (Vitest no responde en este entorno, limitación ya conocida). Pendiente confirmar visualmente en un navegador real.

**Actualización de documentación (misma sesión, a pedido del usuario):**
- **`src/frontend/CLAUDE.md`** — sección nueva "RecetaCristalesForm — diálogo con tablas anchas" con los dos patrones reusables.
- **`CLAUDE.md` raíz** — nota corta en el párrafo del módulo Clínico.
- **`.agents/context/branding-ux-ui.md`** — dos filas nuevas en "Patrones de UI agregados".
- **`.agents/context/reglas-negocio-legado.md`** — regla nueva sobre el auto-check de Cerca (matiz de mejora sobre una regla ya "Preservada" del legacy).
- **`Manual_Tecnico_Frontend_OPT.docx`** — nota agregada en la sección del módulo Clínico.
- **Sin tocar**: `AGENTS.md` (no hubo decisión de arquitectura ni ADR nuevo — solo UI/UX), `Manual_Tecnico_Backend_OPT.docx` (sin cambios de backend) y `Diccionario_Datos_OPT.docx` (sin cambios de esquema).

---

## 2026-09-11 — N° de OT manual, con validación de duplicados por año

**Resumen:**
- A pedido del negocio, `OPT_OrdenDeTrabajo.NumeroOT` dejó de generarse atómicamente en la base de datos (`SEQ_NumeroOT`, decisión de la sesión 2026-08-19) y vuelve a ingresarse a mano, igual que el legacy — el operador necesita poder tipear el número, no que se lo asigne el sistema. A diferencia del legacy (que nunca validaba duplicados de forma confiable), el sistema nuevo agrega una regla que allá no existía: un `NumeroOT` no puede repetirse con otra OT del **mismo año** (por `CreadoEn`) que **no esté anulada**; si la OT que ya lo tiene está `ANULADO`, el número queda libre para reutilizarse.

**Cambios (backend):**
- `OrdenDeTrabajo.Crear` pasa a recibir `numeroOT` como primer parámetro (antes lo fijaba el `DEFAULT` de la BD), con guarda de dominio `numeroOT > 0` → `DomainException`.
- `IOrdenDeTrabajoRepositorio.ExisteNumeroOTVigenteAsync(numeroOT, año)` (nuevo) — `CrearOrdenDeTrabajoCommand` gana el campo `NumeroOT` (validado `> 0` en `CrearOrdenDeTrabajoCommandValidator`) y `CrearOrdenDeTrabajoCommandHandler` lo consulta antes de crear, lanzando `DomainException` (422) si hay choque. Deliberadamente **no** se acumuló en el diccionario `errores` de `ValidationException` — el frontend solo muestra el `title` del problema en el toast, no el detalle por campo.
- `NumeroOT` queda **inmutable tras crear la orden**: `ActualizarOrdenDeTrabajoCommand` no lo recibe.
- Esquema: `src/basedatos/008_numero_ot_manual.sql` quita el `DEFAULT (NEXT VALUE FOR SEQ_NumeroOT)` de la columna (constraint sin nombre explícito, se busca dinámicamente por tabla+columna) y reemplaza el índice único global `UQ_OrdenesDeTrabajo_NumeroOT` por uno **filtrado**, `UQ_OrdenesDeTrabajo_NumeroOT_Vigente` (`WHERE EstadoOTId <> 7`), como respaldo de la BD. Ese índice es deliberadamente más estricto que la regla de negocio (no distingue año, solo "no anulada") — decisión tomada con el usuario como defensa adicional; un choque cruzado entre años con una OT antigua todavía activa es un caso extremo sin mensaje amigable (caería en el 500 genérico). `SEQ_NumeroOT` queda creada sin uso, por si hiciera falta de respaldo.

**Cambios (frontend):**
- `numeroOT` vuelve a ser el primer campo del paso Cliente del asistente de alta (`orden-de-trabajo-form`), obligatorio y `> 0` en el cliente, deshabilitado en edición igual que `cliente`/`sucursalId`. El mensaje de duplicado llega tal cual desde el backend vía el `errorInterceptor` existente, sin lógica nueva en el frontend.

**Verificación:**
- **No compilado ni probado en esta sesión** — sin acceso a `dotnet build`/`npm test` en el entorno donde se hizo el cambio. Pendiente correr `dotnet build OPT.sln`, `npm run lint`/`build`/`test`, y aplicar `008_numero_ot_manual.sql` contra `dbOPT_NET` antes de dar el cambio por verificado.

**Actualización de documentación (misma sesión, a pedido del usuario):**
- **`CLAUDE.md` raíz** — párrafo nuevo "N° de OT manual, con validación de duplicados por año" en "Estado actual".
- **`src/frontend/CLAUDE.md`** — bullet "N° de OT manual" documentando el campo en el paso Cliente del asistente.
- **`src/basedatos/README.md`** — fila del script `008` en la tabla de scripts aplicados, y nota de que el gotcha de `SEQ_NumeroOT` ya no aplica a esta columna.
- **`.agents/context/reglas-negocio-legado.md`** — la regla sobre el número de OT calculado en la app (antes clasificada "Mejorar", corregida con `SEQUENCE`) se revirtió a "Preservar, con validación nueva": el ingreso manual vuelve, pero con la validación por año que el legacy nunca tuvo.

**Próximos pasos sugeridos:**
1. Compilar `OPT.sln` y correr `npm run lint`/`build`/`test` antes de dar el cambio por verificado.
2. Aplicar `008_numero_ot_manual.sql` contra `dbOPT_NET` y confirmar que el índice filtrado `UQ_OrdenesDeTrabajo_NumeroOT_Vigente` quedó creado.
3. Actualizar `Diccionario_Datos_OPT.docx` y `Manual_Tecnico_Backend_OPT.docx` con la nueva forma de `NumeroOT` (columna sin `DEFAULT`, índice filtrado) — quedó pendiente en esta sesión y se resolvió recién en la sesión de documentación del 2026-09-15.

---

## 2026-09-15 — Autorización por rol y sucursal (BOLA/IDOR) + selector de sucursal en el menú + rediseño del ticket imprimible

**Resumen:**
- Sesión de dos partes, ambas resolviendo huecos que quedaban abiertos desde sesiones anteriores. La primera: el legacy nunca tuvo ningún control de autorización en el servidor (el rol de `OPT_Usuario` solo ocultaba ítems de menú en el cliente) y el sistema nuevo, hasta esta sesión, corría con `[Authorize]` genérico — cualquier usuario autenticado podía operar cualquier recurso de cualquier sucursal. Se implementó autorización por rol y control de acceso por sucursal (BOLA/IDOR), resolviendo el punto 5 de "Pendiente" de `CLAUDE.md` señalado desde el ADR `0007`/`0004`. La segunda: un rediseño del ticket imprimible de OT para acercarlo al formato real del legacy (3 copias con firma) y la posibilidad de reimprimirlo desde la ficha de una orden ya existente, cosa que la migración había dejado disponible solo al momento de crear la OT.

**Cambios backend — autorización por rol:**
- `OPT.API/Authorization/AutorizarRolesAttribute.cs` — `IAuthorizationFilter` propio que lee el claim numérico `rolId` del JWT (no usa `[Authorize(Roles=...)]` porque ese mecanismo compara contra `ClaimTypes.Role`, que el token no emite).
- `OPT.Domain/Common/RolesOPT.cs` — constantes de los 8 roles reales de `OPT_Rol` (`Administrador=1` … `Externo=8`, sembrados en `002_catalogos.sql`) agrupados por función: `Administracion`, `GestionComercial`, `OperacionClinica`, `OperacionComercial`/`OperacionComercialConCalidad`, `AnulacionComercial`, `Cobranza`, `AccesoTotalSucursales`.
- Aplicado a: `UsuariosController` (Administrador exclusivo), `SucursalesController`/`EmpresasController` (mutaciones: Administrador/Supervisor), `ClientesController`/`AnamnesisController`/`RecetaCristalesController` (datos clínicos ADR `0004`: Administrador/Supervisor/JefeSucursal/Vendedor/TecnicoMedico/Operador, sin ControlCalidad ni Externo), `OrdenesDeTrabajoController` (por acción: lectura y flujo normal amplio, `Anular`/`AnularCuota` reservado a Administrador/Supervisor/JefeSucursal, `CambiarEstado` suma ControlCalidad), `CobranzaController` (Administrador/Supervisor/JefeSucursal). Los catálogos de solo lectura quedan sin restricción.

**Cambios backend — autorización por sucursal (BOLA/IDOR):**
- `OPT.Application/Common/Security/AutorizacionSucursal.cs` — `ValidarAcceso(currentUser, sucursalId)`, invocado desde los handlers de `OrdenesDeTrabajo` que cargan o crean una OT: Crear, Actualizar, CambiarEstado, Anular, RegistrarAbono, RegistrarPago, GenerarPlanCuotas, PagarCuota, AnularCuota, ObtenerPorId. Lanza `ForbiddenAccessException` (nueva, mapeada a HTTP 403 en `ExceptionHandlingMiddleware`) si la OT no es de la sucursal activa del usuario ni de ninguna de sus sucursales asignadas.
- `ICurrentUserService` ganó `RolId` y `SucursalesAsignadas`; `TokenService` agrega el claim `sucursales` (CSV de `UsuarioSucursal`) al JWT emitido en el login.
- `ObtenerOrdenesDeTrabajoQuery`/su handler restringen de oficio el listado a la sucursal activa del usuario cuando no filtra explícitamente por sucursal y no tiene alcance nacional (`RolesOPT.AccesoTotalSucursales`, hoy solo Administrador).

**Cambios frontend — selector de sucursal en el menú:**
- `Auth` (`core/services/auth.ts`) gana `sucursalActualId` (signal), `cambiarSucursal(id)` y persistencia en `sessionStorage` (clave `opt.sucursalActual`) — puramente de sesión/UI: como el backend ya autoriza contra cualquiera de las sucursales asignadas al usuario (no solo la activa del JWT), cambiar de sucursal en el menú no requiere reemitir el token. Al hacer login se fija siempre a `sucursalActivaId` (la primera asignada), descartando cualquier elección previa.
- `JwtClaims`/`UsuarioActual` (`core/models/auth.models.ts`) ganaron `sucursales`/`sucursalesAsignadas`.
- `Shell` (`layout/shell/`) agrega un selector de sucursal en la barra (`mat-menu`, ícono `storefront`): lee `Sucursales.listar()` en el constructor y lo filtra client-side contra `usuarioActual()?.sucursalesAsignadas` — deliberadamente sin un endpoint "mis sucursales" nuevo. Con una sola sucursal asignada se muestra de solo lectura (`.sucursal-unica`); con más de una, cada ítem del menú llama a `cambiarSucursal`. Colapsa a `icon-button` en pantallas angostas, igual que el resto de la barra.

**Cambios frontend — rediseño del ticket imprimible (sin cambios de backend):**
- `features/ordenes-de-trabajo/components/ticket-ot/` — replica el formato de `_ParcialTicketOT.cshtml` del legacy: encabezado con los datos fijos de la empresa (constante `EMPRESA` en `ticket-ot.ts` — nombre "Centro Óptico BL", RUT, fono, dirección, con nota en el código de que está pendiente de definición final), texto "Nota de venta y autorización de descuento" con líneas de firma, e impresión en `NUMERO_DE_COPIAS = 3` copias (una visible en pantalla, tres en el papel vía `@for` + reglas de impresión en `ticket-ot.scss`) — el legacy generaba 3 copias para que el cliente firme el compromiso de pago.
- `shared/utils/impresion.util.ts` (nuevo) — `imprimirConClaseBody()`, extrae la lógica de impresión que antes vivía solo en `orden-creada-dialog` (marca `<body class="opt-imprimiendo">`, llama a `window.print()`, limpia en `afterprint` con un `finally` de respaldo). `OrdenCreadaDialog` se simplificó para usarla y perdió los botones "Nueva OT"/"Ver orden" (quedó solo el ticket a la vista con Imprimir/Cerrar).
- `features/ordenes-de-trabajo/components/imprimir-ticket-dialog/` (nuevo, `ImprimirTicketDialog`) — reimprime el ticket de una OT ya existente desde el botón "Imprimir ticket" de `orden-de-trabajo-ficha`, disponible aunque la orden esté anulada o entregada (reimprimir no modifica nada, así que no queda sujeto al aviso de bloqueo de edición de la sesión 2026-09-08). Es la versión de `OrdenCreadaDialog` sin los botones de continuar el flujo de alta; comparte `<app-ticket-ot>` y `imprimirConClaseBody()`.

**Verificación:**
- Backend: `dotnet build OPT.sln` compila limpio. **No** se corrió `dotnet test` ni una sesión autenticada por rol/sucursal contra `dbOPT_NET` real — pendiente antes de dar el punto por cerrado.
- Frontend: no verificado en esta sesión de documentación (fuera de su alcance) — ver el estado de build/lint/test real en el propio commit de código cuando se revise.

**Actualización de documentación (misma sesión, a pedido del usuario):**
- **`.agents/context/seguridad-apis.md`** (nuevo) — snapshot completo de controles de seguridad implementados/faltantes, con la decisión de secuencia (ahora vs. antes de producción) y las reglas de forma de trabajo para un agente IA. Referenciado desde `.agents/context/README.md`.
- **`src/documentos/Manual_Tecnico_Seguridad_OPT.docx`** (nuevo) — checklist de 18 controles de seguridad con estado real verificado en código, incluidas las secciones 3.12/3.13 (autorización por rol y por sucursal, ambas "resuelto 2026-09-15") y 4.1/4.2 (las mismas brechas, marcadas RESUELTO). Referenciado desde `src/documentos/README.md`.
- **`CLAUDE.md` raíz** — párrafo nuevo en "Estado actual" con el detalle de ambos mecanismos de autorización y el selector de sucursal; punto 5 de "Pendiente" actualizado (la autorización por sucursal deja de estar pendiente, se detalla qué queda realmente pendiente: rate limiting, security headers, logging de seguridad, refresh tokens); frase agregada en el párrafo largo de Comercial sobre el rediseño del ticket y `imprimir-ticket-dialog`.
- **`src/frontend/CLAUDE.md`** — bullet nuevo sobre el selector de sucursal en la sección de convenciones de UI; bullet de impresión extendido con el rediseño de 3 copias y `ImprimirTicketDialog`.
- **`.agents/context/reglas-negocio-legado.md`** — regla nueva en "Autenticación y sesión" documentando que el legacy no tenía control de rol server-side (contexto para la sección de seguridad); regla de ticket precisada con el detalle de las 3 copias y la reimpresión; nota agregada al selector de sucursal activa.
- **`Manual_Tecnico_Backend_OPT.docx`** — sección nueva 8.3 "Autorización por rol y por sucursal (2026-09-15)", nota en 3.2.3/4.3/6.2/7.7 sobre `NumeroOT` manual y la resolución del punto pendiente de autorización por sucursal.
- **`Manual_Tecnico_Frontend_OPT.docx`** — nota en 3.4 (selector de sucursal en `Shell`), 4.2 (`Auth.cambiarSucursal`), sección 6 extendida con el rediseño del ticket y `ImprimirTicketDialog` (6.7/6.8), y sección 8.4 nueva sobre autorización por rol/sucursal desde la perspectiva del frontend.
- **Sin tocar**: `Diccionario_Datos_OPT.docx` (sin cambios de esquema en esta sesión) y `Manual_Tecnico_UX_OPT.docx` (sigue en v1.0, sin relación con seguridad).
- **Edición hecha con `python-docx`, no verificada visualmente** (no hay pandoc/LibreOffice/zip en este entorno de documentación) — releída con `python-docx` tras guardar, no renderizada a PDF ni abierta en Word.

**Próximos pasos sugeridos:**
1. Correr `dotnet test` y probar en navegador con un usuario de cada rol contra `dbOPT_NET` real, para cerrar la verificación end-to-end de ambos controles de autorización.
2. Implementar los puntos que `seguridad-apis.md` deja deliberadamente para la pasada final antes de producción: rate limiting en `/api/auth/login`, security headers, logging de eventos de seguridad, refresh tokens/revocación de JWT.
3. Confirmar visualmente el rediseño del ticket (3 copias, líneas de firma) y el selector de sucursal en un navegador real — ninguno de los dos se verificó fuera de la lectura de código en esta sesión.
4. `Manual_Tecnico_UX_OPT.docx` sigue en v1.0 y es el documento más desalineado de `src/documentos/` — no refleja ninguna pasada de UX/UI posterior a su primera versión.

---

## 2026-09-15 (2ª sesión) — Módulo Operativo: requerimiento y Etapa 1 (esquema)

**Resumen:**
- El usuario aportó `OPT_Requerimiento_Modulo_Operativo.md`, un levantamiento funcional para un nuevo "Módulo Operativo" (Operativos Oftalmológicos en terreno): agrupar las OT de una jornada bajo un Operativo, registrar sus gastos, filtrar Cobranza/reporte de cristales por Operativo y calcular ganancia/pérdida.
- **Hallazgo importante**: el documento estaba escrito para otro proyecto, no para OPT_NET — usaba multi-tenant (`TenantId`), PK `UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID()`, `IsDeleted`, un header `X-Sucursal-Id` y módulos "Agenda"/"Atención" que no existen acá, y daba por **no implementada** la entidad `OrdenTrabajo` (en OPT_NET, `OrdenDeTrabajo` está completa desde el 2026-08-27, ADR `0007`). Se tradujeron todas las convenciones antes de diseñar el esquema — tabla completa de la traducción en `.agents/context/modulo-operativo.md` § 2.
- Se resolvieron con el usuario (`AskUserQuestion`) los 4 puntos abiertos del requerimiento que bloqueaban el diseño de tablas: Correlativo autogenerado (no manual), `Operativo.SucursalId` obligatorio (reusa `AutorizacionSucursal` a futuro), `GastoOperativo` solo Monto+N°Documento+Observación (sin fecha propia ni categoría), y Anular solo permitido desde PROSPECTO/INGRESADO (no desde COBRANZA).
- Se generó **`src/basedatos/009_modulo_operativo.sql`** (idempotente, escrito a mano — mismo patrón que `004`/`005`/`008`): `OPT_EstadoOperativo` (catálogo, 5 valores), `SEQ_CorrelativoOperativo`, `OPT_Operativo` (`AuditableEntity` + `PublicId`; `MontoTotalVendido`/`MontoTotalPagado`/`MontoTotalGastos` persistidos, a recalcular transaccionalmente igual que `OrdenDeTrabajo.Precio`/`TotalAbonado`/`Saldo`), `OPT_OperativoOT` (relación pura 1→N, índice único en `OrdenDeTrabajoId` — una OT pertenece a lo sumo un Operativo, sin `PublicId` propio) y `OPT_GastoOperativo` (`AuditableEntity`).
- Alcance de esta sesión: **solo esquema**, a pedido explícito del usuario ("como primera etapa"). No se tocaron entidades de `OPT.Domain` ni `IEntityTypeConfiguration<T>` — el modelo de EF Core y la base quedan desalineados hasta la siguiente sesión, mismo patrón de deuda que dejó `004` con `Pago`/`Cuota`/`EstadoCuota` (resuelto recién en 2026-08-27).
- **No se aplicó el script a `dbOPT_NET`** — solo se generó y se registró en la documentación; queda pendiente correrlo.

**Decisiones tomadas:** ver tabla completa en `.agents/context/modulo-operativo.md` § 3 (Correlativo, SucursalId, campos de GastoOperativo, transición a Anulado).

**Actualización de documentación (misma sesión, a pedido del usuario "actualiza .md para contexto y Técnico correspondientes"):**
- **`.agents/context/modulo-operativo.md`** (nuevo) — documento de contexto completo del módulo: problema, tabla de traducción de convenciones, decisiones cerradas, detalle del esquema `009`, puntos que siguen abiertos (fórmula de ganancia/pérdida, permisos, invariantes no modeladas como constraint de BD, migración histórica fuera de alcance) y próximos pasos por capa. Incluye el texto completo del requerimiento original al final, para trazabilidad. Referenciado desde `.agents/context/README.md`.
- **`src/basedatos/README.md`** — fila nueva para `009_modulo_operativo.sql` en la tabla "Estado actual"; total de tablas actualizado a 27 (pendiente de aplicar).
- **`CLAUDE.md` raíz** — punto 7 nuevo en "Pendiente" con el resumen del módulo, la advertencia sobre el origen del documento, el detalle del esquema `009` y los próximos pasos.
- **`Diccionario_Datos_OPT.docx`** y **`Manual_Tecnico_Backend_OPT.docx`** — actualizados con las 4 tablas nuevas (ver detalle abajo).

**Verificación:** ninguna — sesión de solo-esquema y documentación, sin acceso en este entorno a `sqlcmd`/`dotnet build` para aplicar o compilar nada.

**Próximos pasos sugeridos:**
1. Aplicar `009_modulo_operativo.sql` contra `dbOPT_NET` (verificar primero contra un entorno de desarrollo, no producción).
2. Sesión de dominio: entidades `Operativo`/`OperativoOT`/`GastoOperativo`/`EstadosOperativo` en `OPT.Domain` + `IEntityTypeConfiguration<T>`, cerrando la deuda de alineación con EF Core.
3. Sesión de Application/API: `Features/Operativos/`, `OperativosController`, filtro por Operativo en `ObtenerOrdenesDeTrabajoQuery` y en `CobranzaController`.
4. Sesión de frontend: `features/operativos/`.
5. Confirmar con el usuario la fórmula de ganancia/pérdida a mostrar (8.3) y los permisos por rol (8.7) antes de la sesión de dominio — ver puntos abiertos en `.agents/context/modulo-operativo.md` § 5.

---

## 2026-09-15 (3ª sesión) — Módulo Operativo: Domain/Application/API completos (ADR 0011)

**Resumen:**
- Continuación directa de la sesión anterior (mismo día): con el esquema de `009_modulo_operativo.sql` ya escrito, el usuario pidió generar las APIs de backend para el módulo. Se construyó `OPT.Domain`/`OPT.Infrastructure`/`OPT.Application`/`OPT.API` completos, siguiendo al pie el patrón ya aceptado para `OrdenDeTrabajo` (ADR `0007`): agregado con raíz propia, catálogo de estados con reglas de transición, repositorio con `BuscarPaginadoAsync` + whitelist de orden, `DtoFactory` registrado a mano, controller con `AutorizarRoles` por acción.
- **Decisión de diseño no trivial**: `Operativo` (nuevo agregado) no referencia `OrdenDeTrabajo` en `OPT.Domain`, para no acoplar dos agregados de módulos distintos. Esto obligó a agregar dos columnas que el script `009` original (sesión anterior) no tenía: `OPT_OperativoOT.MontoVendidoSnapshot`/`MontoPagadoSnapshot` — sin ellas, `Operativo` no podía mantener sus totales sin consultar `OrdenDeTrabajo` desde el dominio. El script sigue sin aplicarse a ninguna base, así que extenderlo no rompió nada ya migrado.
- Se resolvieron sin `AskUserQuestion` (el usuario no estaba disponible para las preguntas puntuales) los 3 puntos abiertos que quedaban del requerimiento: 8.1 (snapshot + refresco manual vía `POST /recalcular-montos`, sin sincronización automática al abonar/pagar una OT ya asociada), 8.3 (ambas fórmulas de ganancia/pérdida, derivadas en el DTO) y 8.7 (reuso directo de los grupos de `RolesOPT` ya existentes — `OperacionComercial`/`AnulacionComercial` — sin crear uno nuevo). Las tres decisiones quedaron documentadas explícitamente como decisión de IA (no de negocio) en el ADR `0011`, para que una sesión futura con el usuario disponible las pueda confirmar o revertir.
- El módulo **no tiene bitácora propia** (a diferencia de `OrdenDeTrabajo`/`BitacoraOT`): el requerimiento nunca la pidió y agregarla habría sido esquema no solicitado. El motivo de una anulación queda concatenado en `Operativo.Observacion`.
- El filtro de Cobranza por Operativo (sección 6 del requerimiento) se resolvió agregando `OperativoPublicId` a `ObtenerOrdenesDeTrabajoQuery`/`operativoId` a `IOrdenDeTrabajoRepositorio.BuscarPaginadoAsync` — mismo mecanismo que ya existía para `empresaPublicId`, sin tocar `CobranzaController`. El filtro por Operativo en el "reporte de cristales" del requerimiento no se implementó porque ese reporte no existe como endpoint en el sistema nuevo.

**Piezas construidas:**
- `OPT.Domain/Entities/Operativo/`: `Operativo` (agregado raíz), `OperativoOT`, `GastoOperativo`, `EstadoOperativo`, `EstadosOperativo` (flujo `Prospecto(1)→Ingresado(2)→Cobranza(3)→Cerrado(4)`, sin retroceso; `Anulado(5)` solo alcanzable desde `Prospecto`/`Ingresado`).
- `OPT.Domain/Interfaces/Repositories/IOperativoRepositorio.cs`, `IEstadoOperativoRepositorio.cs`; extensión de `IOrdenDeTrabajoRepositorio.BuscarPaginadoAsync` con el parámetro `operativoId`.
- `OPT.Infrastructure/Persistence/Configurations/Operativo/` (4 configuraciones EF), `Repositories/OperativoRepositorio.cs`, `Repositories/EstadoOperativoRepositorio.cs`; `AppDbContext` con los 4 `DbSet` nuevos y la secuencia `SEQ_CorrelativoOperativo`; `DependencyInjection` con los 2 repos nuevos registrados.
- `OPT.Application/Features/Operativos/`: `OperativoDto.cs`, `OperativoDtoFactory.cs` (registrada a mano en `AddApplication`, igual que `OrdenDeTrabajoDtoFactory`), y 9 Commands (Crear, Actualizar, CambiarEstado, Anular, AsociarOrden, QuitarOrden, RecalcularMontos, RegistrarGasto, EliminarGasto) + 2 Queries (ObtenerTodos paginado, ObtenerPorId), cada uno con su Validator donde aplica.
- `OPT.Application/Features/EstadosOperativo/`: catálogo de solo lectura.
- `OPT.API/Controllers/OperativosController.cs` (`/api/operativos`, siempre por `PublicId`, subrecursos `ordenes`/`gastos`/`estado`/`anular`/`recalcular-montos` anidados) y `EstadosOperativoController.cs`.

**Verificación:**
- `dotnet build OPT.sln` compila limpio (única advertencia preexistente y no relacionada en `InventarioController.cs`).
- `dotnet ef dbcontext info --project OPT.Infrastructure --startup-project OPT.API` — el modelo de EF Core valida sin errores (ejercita `OnModelCreating`, incluidas las 4 configuraciones nuevas).
- **No verificado**: el script `009_modulo_operativo.sql` (con las columnas de snapshot ya agregadas) no se aplicó a ninguna base real; no hubo sesión autenticada de prueba contra `dbOPT_NET`; no se generó el script `dotnet ef migrations script --idempotent` de comparación (se verificó la paridad columna-por-columna a mano contra el `.sql`, dado que no había una migration base previa contra la cual diffear sin el procedimiento largo de `CLAUDE.md`).

**Actualización de documentación (misma sesión, a pedido del usuario "actualiza archivos .md ... y manuales técnicos"):**
- **`.agents/decisions/0011-api-modulo-operativo.md`** (nuevo ADR) — las 6 decisiones de diseño de esta sesión, con alternativas descartadas y consecuencias/riesgos aceptados explícitos. Indexado en `.agents/decisions/README.md`.
- **`.agents/context/modulo-operativo.md`** — nueva sección 5 "Dominio, Application y API — completados", detalle de dónde vive cada pieza y qué puntos abiertos del requerimiento quedaron resueltos (remite al ADR `0011` para el porqué); sección 6 "Próximos pasos" reescrita (ya no queda dominio/Application/API pendiente, solo aplicar el script, probar end-to-end y frontend); encabezado actualizado a "Etapas 1 y 2 completadas".
- **`.agents/context/README.md`** — descripción de `modulo-operativo.md` actualizada.
- **`src/basedatos/README.md`** — fila de `009_modulo_operativo.sql` actualizada: menciona las columnas de snapshot y que Domain/Application/API ya están completos (sigue sin aplicarse a una base real).
- **`CLAUDE.md` raíz** — punto 7 de "Pendiente" reescrito en la sesión anterior a esta entrada de progreso (ver el propio `CLAUDE.md`) con el detalle de la Etapa 2; no requirió cambios adicionales en esta pasada de documentación.
- **`Manual_Tecnico_Backend_OPT.docx`** y **`Diccionario_Datos_OPT.docx`** — actualizados con el detalle de Domain/Application/API y las 2 columnas de snapshot (ver detalle en la entrada de esta sesión más abajo si se agregó una sección aparte, o el propio `.docx`).

**Próximos pasos sugeridos:**
1. Aplicar `009_modulo_operativo.sql` (ya con las columnas de snapshot) contra `dbOPT_NET` de desarrollo.
2. Probar `OperativosController` end-to-end con una sesión autenticada real.
3. Confirmar con el usuario las 3 decisiones que esta sesión tomó sin preguntarle (refresco manual de montos, reuso de roles, ausencia de bitácora) — ADR `0011`.
4. Sesión de frontend: `features/operativos/`.

---

## 2026-09-15 (4ª sesión) — Módulo Operativo: frontend completo

**Resumen:**
- Continuación de la sesión anterior (mismo día): con el backend del módulo ya completo (Etapas 1 y 2), el usuario pidió generar el frontend siguiendo el estándar de codificación/arquitectura del proyecto y consultando las APIs disponibles ante cualquier duda.
- Se investigó primero el contrato real de `OperativosController`/`EstadosOperativoController` (rutas, roles, shape de `OperativoDto`/`OperativoResumenDto`, reglas de `EstadosOperativo`) y los patrones ya establecidos en `src/frontend/CLAUDE.md` para el módulo Comercial (`ordenes-de-trabajo`), en vez de adivinar campos — siguiendo la regla ya escrita en "Antes de escribir código de un feature nuevo".
- Se construyó `features/operativos/` completo: `models/operativo.model.ts` (espejo exacto de los DTOs verificados), `services/operativos.ts` + `services/estados-operativo.ts`, `operativos.routes.ts`, `pages/operativos-list` (paginado, auto-carga al entrar — a diferencia del listado de OT, Operativo no tiene volumen que lo justifique), `pages/operativo-form` (diálogo de alta/edición; Empresa por autocompletado igual que en `orden-de-trabajo-form`, Sucursal por `mat-select`, ambas inmutables tras crear), `pages/operativo-ficha` (ruteada, cabecera + resumen financiero de 5 cifras + barra de flujo de estados sin retroceso + pestañas Órdenes/Gastos), y los componentes `estado-operativo-chip`, `asociar-orden-dialog` (envuelve `<app-selector-orden>` ya existente) y `gasto-operativo-dialog`.
- **Decisión de diseño**: el chip de estado no creó tokens de color nuevos (`--opt-estado-operativo-*` al estilo de `EstadoOT`) — reutiliza las clases globales `.opt-chip--info/si/alerta` de `styles.scss`. Evita el proceso de auditoría WCAG que el branding doc exige para sumar un color, que un catálogo de 5 estados no justificaba.
- **Cambio adicional acotado**: se agregó el filtro de contexto `operativoPublicId` a `FiltrosOrdenesDeTrabajo`/`OrdenesDeTrabajo.buscar()`/`ordenes-de-trabajo-list` (mismo mecanismo que `empresaPublicId`/`clientePublicId`) para que "Ver en el listado de OT" desde la ficha del Operativo funcione — el backend ya lo soportaba desde la Etapa 2. No se tocó `CobranzaController` (no expone ese filtro) ni el reporte de cristales (no existe como endpoint).
- Registrado en `app.routes.ts` (`/operativos`) y en el grupo "Comercial" del menú de `Shell`.
- Todos los componentes se generaron con `ng generate` (convención del proyecto), no a mano.

**Verificación:**
- `npm run lint` — limpio.
- `npm run build` — limpio (el único warning de budget es preexistente, en `orden-de-trabajo-ficha.scss`, no tocado esta sesión).
- `npm test` — 65/66 archivos, 105/107 tests. Los 2 tests que fallan son de `orden-de-trabajo-form.spec.ts` (lógica de `sinPlanCuotas`/`primerVencimiento`), preexistentes a esta sesión (ese archivo ya figuraba modificado sin commitear antes de empezar) y no relacionados con Operativo.
- **No verificado en navegador contra `dbOPT_NET` real** — bloqueado en que se aplique `009_modulo_operativo.sql` (pendiente desde la Etapa 1/2).

**Actualización de documentación (misma sesión, a pedido del usuario "Actualiza archivos .md ... Actualiza Manuales técnicos"):**
- **`CLAUDE.md` raíz** — punto 7 de "Pendiente" ampliado con la Etapa 3 (frontend); árbol de `Estructura de proyectos` actualizado con `Entities/Operativo/`, `Features/Operativos|EstadosOperativo/` y los 2 controllers nuevos.
- **`src/frontend/CLAUDE.md`** — nueva sección "Módulo Operativo (sesión 2026-09-15, 2ª)" con las decisiones de UI (diálogo vs. ficha ruteada, flujo sin retroceso, reuso de `selector-orden`, chip sin tokens nuevos, filtro `operativoPublicId`).
- **`src/frontend/README.md`** — `operativos/` agregado al árbol de `features/`.
- **`.agents/context/modulo-operativo.md`** — nueva sección 5a "Frontend — completado"; sección 6 "Próximos pasos" y el encabezado actualizados (ya no queda frontend pendiente, solo aplicar el script y verificar end-to-end).
- **`.agents/context/README.md`** — descripción de `modulo-operativo.md` actualizada.
- **`.agents/decisions/0011-api-modulo-operativo.md`** — el riesgo aceptado "Sin frontend" marcado como resuelto, con la aclaración de que el refresco de montos quedó manual (botón "Recalcular montos"), no automático.
- **`Manual_Tecnico_Frontend_OPT.docx`** — nueva sección para el módulo Operativo (ver detalle más abajo).

**Próximos pasos sugeridos:**
1. Aplicar `009_modulo_operativo.sql` contra `dbOPT_NET` de desarrollo.
2. Probar `OperativosController` y el frontend end-to-end con una sesión autenticada real.
3. Confirmar con el usuario las 3 decisiones de la Etapa 2 tomadas sin preguntarle (ADR `0011`) y evaluar si el refresco de montos debe automatizarse.
4. Migración de OT históricas a Operativos y filtro de Operativo en Cobranza, si el negocio los pide (ambos fuera de alcance hasta ahora).

---

## 2026-09-16 — Módulo Operativo: se aplicó el script `009`, se corrigió el alta silenciosa y se agregó `Nombre`

**Resumen:**
- El usuario reportó que el diálogo "Nuevo Operativo" no guardaba registro. Se aplicó `009_modulo_operativo.sql` contra `dbOPT_NET` (verificado con `sqlcmd` antes y después: la tabla `OPT_Operativo` ya existía y estaba vacía, así que ese script no era la causa — quedó aplicado igual, cerrando el pendiente de la Etapa 1/2). La causa real estaba en `operativo-form.ts`: el control `empresa` no tenía ningún `Validator`, así que `form.invalid` no reflejaba que el usuario no había elegido una empresa del autocompletado (solo tipeado texto) — `guardar()` bloqueaba el envío con `!this.esEdicion && !this.empresaElegida()` pero retornaba **sin ningún error visible**, dando la sensación de que el botón "no hacía nada".
- **Fix**: se agregó `empresaSeleccionadaValidator` (exige que el valor del control sea el objeto `Empresa` elegido, no un string) al `FormControl` de empresa — con eso `form.invalid` ya refleja el estado real y se agregó un `<mat-error>` en el template ("Elige una empresa de la lista de sugerencias"). Se quitó el chequeo redundante y silencioso de `guardar()`.
- **Nuevo campo `Nombre` en `Operativo`** (a pedido del usuario, no estaba en el requerimiento original): columna `nvarchar(200) NOT NULL` (script `010_operativo_nombre.sql`, aplicado), propiedad en la entidad de dominio (`Operativo.Crear`/`Actualizar` la reciben como primer parámetro obligatorio), `IEntityTypeConfiguration` actualizada, `CrearOperativoCommand`/`ActualizarOperativoCommand` + validadores (`NotEmpty`, `MaximumLength(200)`), `OperativoDto`/`OperativoResumenDto`/`OperativoDtoFactory`/`ObtenerOperativosQueryHandler` actualizados, y `OperativoRepositorio` ahora también busca (`busqueda`) y ordena (`ordenarPor=nombre`) por este campo. Frontend: campo de texto nuevo (primero del diálogo) en `operativo-form`, columna "Nombre" en `operativos-list`, y el título/cabecera de `operativo-ficha` pasa a mostrar el nombre en vez de solo "Operativo N° {correlativo}".
- Tabla `OPT_Operativo` estaba vacía en `dbOPT_NET` al momento de agregar la columna — no hizo falta backfill.

**Verificación:**
- `dotnet build OPT.sln` — limpio.
- `npm run build` / `npm run lint` — limpios.
- `npm test` — 65/66 archivos, 105/107 tests; los 2 que fallan son los mismos preexistentes de `orden-de-trabajo-form.spec.ts` de la sesión anterior, no relacionados.
- **No se probó en navegador con sesión autenticada real** (sigue pendiente, ver punto 2 de la entrada anterior).

**Próximos pasos sugeridos:**
1. Probar el alta de un Operativo en el navegador contra `dbOPT_NET` con una sesión autenticada real — confirmar que el fix del validador resuelve el caso reportado.
2. Seguir con los puntos 2-4 de la entrada anterior (verificación end-to-end de todo el módulo, confirmar decisiones del ADR `0011`, migración de OT históricas).

---

## 2026-09-22 — Módulo OT: HU-OT-01 a HU-OT-05 (`src/documentos/HU/01_HU_Modulo_OT.html`)

**Resumen:**
Se analizaron las 5 historias de usuario de `01_HU_Modulo_OT.html` (y el `00_Analisis_Impacto.html` que las origina) — todas de bajo impacto, sin tocar reglas de negocio del agregado `OrdenDeTrabajo` (confirmado explícitamente en el propio documento). Estado de cada una:

- **HU-OT-01** (Empresa opcional en Sucursal) — **ya estaba resuelta**: el campo Empresa del asistente de alta (`orden-de-trabajo-form`) ya es opcional (`Validators` sin `required`, etiqueta "Empresa convenio (opcional)", hint "Déjalo vacío si la orden es particular"). No se tocó código. El criterio sobre precargar Empresa al crear desde la ficha de un Operativo queda fuera de alcance de este documento (pertenece al submenú Recepción de `02_HU_Modulo_Operativo.html`, todavía no construido).
- **HU-OT-04** (bloqueo de edición en OT `Entregado`/anulada) — **ya estaba resuelta**: el aviso `.aviso--bloqueo` vive en la ficha de la OT (`orden-de-trabajo-ficha`), independiente de la pantalla desde la que se navegue a ella. Sin cambios.
- **HU-OT-05** (combinar `operativoPublicId` + `estadoOTId` en el listado) — **verificado, ya funcionaba**: `OrdenDeTrabajoRepositorio.BuscarPaginadoAsync` aplica ambos filtros como `Where` independientes (AND), no son mutuamente excluyentes. Sin cambios.
- **HU-OT-02** (distinguir origen Sucursal/Operativo en el listado) — **implementado**: `OrdenDeTrabajoResumenDto` gana `OperativoPublicId`/`OperativoNombre` (100% derivados, sin persistir nada nuevo — ver comentario en `OrdenDeTrabajoDto.cs`), poblados con un lookup por página vía el método nuevo `IOperativoRepositorio.ObtenerPorOrdenesDeTrabajoIdsAsync(ids)` (mismo patrón que el lookup de clientes/sucursales/estados que ya hacía el handler). `ObtenerOrdenesDeTrabajoQuery` gana `SoloSucursal` (bool?) y `OrdenDeTrabajoRepositorio.BuscarPaginadoAsync` un parámetro homónimo que excluye las OT con alguna fila en `OPT_OperativoOT`. Frontend: columna nueva "Origen" en `ordenes-de-trabajo-list` (chip `Sucursal` de solo lectura o chip clicable con el nombre del Operativo, que navega a `/operativos/:publicId`) y checkbox "Solo Sucursal" (deshabilitado cuando ya hay un filtro de contexto `operativoPublicId`, para no combinar dos filtros contradictorios).
- **HU-OT-03** (ver el Operativo desde la ficha de la OT) — **implementado**: `OrdenDeTrabajoDto` gana `OperativoPublicId`/`OperativoCorrelativo`/`OperativoNombre`, poblados por `OrdenDeTrabajoDtoFactory` (mismo método nuevo del repositorio, con un solo Id). Frontend: la cabecera de `orden-de-trabajo-ficha` muestra un campo "Operativo" con enlace a `/operativos/:publicId` **solo si** la OT tiene uno asociado (no se muestra la sección vacía, como pide el criterio de aceptación).

**Decisiones tomadas sin preguntar (de bajo riesgo, documentadas acá por transparencia):**
- El campo "Operativo" en el listado se llamó columna **"Origen"** (no "Línea de negocio", que es el término del documento de análisis) — más corto para el ancho de columna y consistente con el lenguaje ya usado en pantalla.
- El chip del Operativo en el listado es clicable y navega directo a su ficha (no solo un texto) — la HU no lo pedía explícitamente para el listado (sí para la ficha de la OT en HU-OT-03), pero es consistente con el mismo patrón ya aplicado en la ficha.

**Verificación:**
- `dotnet build OPT.sln` — limpio.
- `npm run build` — limpio (el único warning de budget, en `orden-de-trabajo-ficha.scss`, es preexistente — confirmado comparando contra el `git stash` previo a esta sesión).
- `npm run lint` — limpio.
- `npm test` — 66/66 archivos, 105/105 tests (los 2 tests preexistentes de `orden-de-trabajo-form.spec.ts` que fallaban en las dos sesiones anteriores ya no aparecen — ese archivo seguía modificado sin commitear de antes, no se investigó más a fondo por estar fuera de alcance de esta HU).
- **No verificado en navegador contra `dbOPT_NET` real con sesión autenticada** (mismo bloqueo estructural de sesiones anteriores).

**Próximos pasos sugeridos:**
1. Verificar en navegador con sesión autenticada real: el chip "Origen" del listado, el filtro "Solo Sucursal" y el enlace "Operativo" de la ficha de la OT.
2. Cuando se construya el submenú Recepción del módulo Operativo (`02_HU_Modulo_Operativo.html`), retomar el criterio pendiente de HU-OT-01 (precargar y bloquear Empresa al crear una OT desde ese contexto).

---

## 2026-09-22 (2ª) — Módulo Operativo: HU-OP-01 a HU-OP-10 (`src/documentos/HU/02_HU_Modulo_Operativo.html`)

**Resumen:**
Se analizaron las 22 HU del módulo Operativo (4 épicas) junto con `00_Analisis_Impacto.html`. Dado el riesgo de diseño de la Épica C (Cobranza: introduce "vínculo laboral", "desvinculación" y "pérdida" — conceptos de negocio nuevos, con 4 preguntas abiertas sin resolver en el propio análisis), se preguntó al usuario el alcance de la sesión — eligió **Épica A (contacto) + Épica B (submenú Recepción)**, dejando C y D para después. También se resolvió la pregunta abierta N.º 5 (cierre de Operativo con saldo pendiente → "exigir todo resuelto", queda documentada para cuando se aborde la Épica C).

Implementado: HU-OP-01/02 (`NombreContacto`/`MailContacto`/`TelefonoContacto` en `Operativo`, propios de cada jornada) y HU-OP-03 a HU-OP-10 (listado de OT del Operativo con estado y fecha de atención, crear/asociar/desasociar/anular/avanzar etapa de una OT sin salir de la ficha, transición Prospecto→Ingresado renombrada a "Iniciar recepción", y Reporte de Cristales con exportación a Excel/PDF). Detalle completo de diseño, decisiones y archivos tocados en `.agents/context/modulo-operativo.md` § 10 (no se duplica acá).

**Piezas nuevas de nota:**
- `IRecetaCristalesRepositorio.ObtenerPorOrdenesAsync` (bulk, nuevo) para que el Reporte de Cristales no haga una consulta por cada OT del Operativo.
- `RolesOPT.OperacionComercialConCalidad` ahora incluye `TecnicoMedico` (HU-OP-08 lo pide explícitamente) — se aplicó a los endpoints de OT/Operativo que ya usaban ese grupo, sin crear uno nuevo.
- Exportación del reporte: Excel = CSV client-side (sin librería nueva), PDF = diálogo imprimible reutilizando `imprimirConClaseBody()` (mismo mecanismo del ticket de OT) — "Guardar como PDF" lo hace el navegador. El formato de columnas del Excel queda como primera versión, no validada con el usuario (el propio HU lo señala pendiente).
- `orden-de-trabajo-form` acepta contexto `operativoPublicId`/`empresaPublicId`/`sucursalId` por query param: precarga y bloquea Empresa, usa la Sucursal del Operativo (no la activa del menú) y asocia la OT recién creada automáticamente, con aviso si la asociación falla sin perder la OT ya creada.

**Verificación:**
- `dotnet build OPT.sln` — limpio.
- `dotnet ef dbcontext info` — modelo EF válido.
- Script `012_operativo_contacto.sql` — **aplicado a `dbOPT_NET`** y columnas verificadas con `sqlcmd`.
- `npm run build` / `npm run lint` — limpios.
- `npm test` — 66/66 archivos, 105/105 tests (sin regresiones).
- **No verificado en navegador contra `dbOPT_NET` con sesión autenticada real** (mismo bloqueo estructural de sesiones anteriores: usuarios migrados con clave de 4 caracteres, validador exige 6).

**Próximos pasos sugeridos:**
1. Épica C (Cobranza) en sesión dedicada — resolver primero las preguntas abiertas 4-6 del análisis (disparador Prospecto→Ingresado — aunque HU-OP-09 ya quedó resuelto con acción manual explícita en esta sesión —, y sobre todo cómo se registra la desvinculación) antes de modelar `OPT_EstadoCuota.PERDIDA` y el vínculo laboral Cliente-Empresa-Operativo.
2. Épica D (Gastos: categoría + fecha) — bajo esfuerzo, quedó fuera solo por alcance de esta sesión.
3. Verificar en navegador con sesión autenticada real todo lo construido hoy.
