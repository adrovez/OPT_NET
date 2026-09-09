# Source Code — Agent Instructions

> **Última actualización:** 2026-08-28 (vista "Ver Orden" del módulo Comercial y listado de OT diferido — ADR `0008`: se recuperó el vínculo receta↔OT del legacy y el comentario por línea del detalle, script `006` + backfill `M006`. Sesión anterior 2026-08-27: módulo Comercial completo — esquema `004`/`005`, datos `M004`, API del agregado OT con flujo de estados (ADR `0007`), frontend de OT/Abonos/Pagos/Cuotas/Cobranza, paginación server-side transversal y dos pasadas de UX/UI)

Este directorio contiene **todo el código nuevo**. El código legacy vive en `old/` (raíz del proyecto) y NO debe ser modificado.

---

## Nota sobre iteraciones en src/

`src/` contiene **dos capas de desarrollo que no deben mezclarse**:

- **Iteración previa** (`.NET 10 / Angular 21 / multi-tenant`): código construido antes del análisis formal del legacy. Sus decisiones estructurales (multi-tenant, GUID como PK, .NET 10) no están validadas por los ADRs vigentes. Se conserva como referencia de implementación únicamente, a la espera de que el equipo decida si se archiva o elimina.
- **Scaffold Fase 0** (`.NET 8 / Clean Architecture`): **el punto de partida oficial del desarrollo nuevo**, generado el 2026-08-19 siguiendo los ADRs de `.agents/decisions/` y ya extraído/compilando en `src/backend/`.

Para todo desarrollo nuevo, usar **únicamente** el scaffold Fase 0.

---

## Estructura

```
src/
├── backend/
│   ├── OPT.sln
│   ├── OPT.Domain/
│   │   ├── Common/
│   │   │   ├── AuditableEntity.cs         # Base de toda entidad: Id + auditoría + borrado lógico
│   │   │   └── DomainException.cs         # Excepción semántica de negocio → HTTP 422
│   │   ├── Entities/
│   │   │   ├── Organizacion/              # Region, Comuna, Empresa, EmpresaSucursal, Sucursal, Rol, Usuario, UsuarioSucursal
│   │   │   ├── Clinico/                   # Cliente, Anamnesis, RecetaCristales — datos sensibles (ADR 0004)
│   │   │   ├── Comercial/                 # OrdenDeTrabajo, DetalleOT, Abono, BitacoraOT
│   │   │   └── Inventario/               # Producto, ProductoSucursal
│   │   └── Interfaces/Repositories/       # IRepositorioBase<T> + contratos específicos
│   │
│   │   # Cliente, Empresa, Usuario, Anamnesis, RecetaCristales: además de Id (int),
│   │   # tienen PublicId (Guid) — el que se expone en API/URLs (ADR 0004).
│   │
│   ├── OPT.Application/
│   │   ├── Common/
│   │   │   ├── Interfaces/                # IUnitOfWork, IPasswordService, ITokenService, ICurrentUserService
│   │   │   ├── Exceptions/                # NotFoundException, ValidationException
│   │   │   └── Behaviours/                # ValidationBehaviour (pipeline MediatR)
│   │   ├── Features/
│   │   │   ├── Auth/Commands/Login/       # ✅ Implementado: LoginCommand + Handler + Validator
│   │   │   ├── Sucursales/                # ✅ CRUD completo (Id interno en rutas)
│   │   │   ├── Empresas/                  # ✅ CRUD completo (PublicId en rutas)
│   │   │   ├── Usuarios/                  # ✅ CRUD + CambiarClave/Activar/Desactivar/Asignar|QuitarSucursal
│   │   │   ├── Roles/                     # ✅ Solo lectura (catálogo sembrado)
│   │   │   ├── Regiones/                  # ✅ Solo lectura (catálogo sembrado)
│   │   │   ├── Comunas/                   # ✅ Solo lectura, filtrado por RegionId
│   │   │   ├── Clientes/                  # ✅ CRUD + ObtenerTodos paginado (PublicId, Fase 1)
│   │   │   ├── Anamnesis/                 # ✅ CRUD + ObtenerPorCliente (PublicId, Fase 1)
│   │   │   ├── RecetaCristales/           # ✅ CRUD + ObtenerPorCliente (PublicId, Fase 1)
│   │   │   ├── OrdenesDeTrabajo/          # 🔲 Pendiente Fase 2
│   │   │   └── Inventario/                # 🔲 Pendiente Fase 3
│   │   └── DependencyInjection.cs
│   │
│   ├── OPT.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── AppDbContextFactory.cs     # Design-time factory para `dotnet ef`
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── Interceptors/AuditInterceptor.cs
│   │   │   ├── Repositories/              # RepositorioBase<T> + concretos (Usuario, Cliente, Anamnesis, RecetaCristales, OT, Sucursal, Empresa, Rol)
│   │   │   └── Configurations/            # IEntityTypeConfiguration<T> — ✅ completo (4 módulos)
│   │   ├── Identity/                      # PasswordService (BCrypt), TokenService (JWT)
│   │   ├── Services/CurrentUserService.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── OPT.API/
│   │   ├── Program.cs
│   │   ├── appsettings.json               # Credenciales SIEMPRE vacías — usar user-secrets
│   │   ├── Middleware/ExceptionHandlingMiddleware.cs
│   │   └── Controllers/
│   │       ├── AuthController.cs          # ✅ POST /api/auth/login
│   │       ├── SucursalesController.cs    # ✅ CRUD, [Authorize]
│   │       ├── EmpresasController.cs      # ✅ CRUD, [Authorize]
│   │       ├── UsuariosController.cs      # ✅ CRUD + acciones, [Authorize]
│   │       ├── RolesController.cs         # ✅ Solo lectura, [Authorize]
│   │       ├── RegionesController.cs      # ✅ Solo lectura, [Authorize]
│   │       ├── ComunasController.cs       # ✅ Solo lectura, [Authorize]
│   │       ├── ClientesController.cs      # ✅ CRUD + ObtenerTodos paginado, [Authorize]
│   │       ├── AnamnesisController.cs     # ✅ CRUD + ObtenerPorCliente, [Authorize]
│   │       ├── RecetaCristalesController.cs  # ✅ CRUD + ObtenerPorCliente, [Authorize]
│   │       ├── OrdenesDeTrabajoController.cs  # 🔲 Stub
│   │       └── InventarioController.cs    # 🔲 Stub
│   │
│   └── OPT.Migracion/                     # ⚠ Fuera de la arquitectura en capas (ADR 0005) — herramienta temporal, no referenciada por OPT.API
│       ├── Legacy/                        # Lectura de solo lectura contra db_a25cfd_opt2
│       ├── Destino/                       # Lectura/escritura contra dbOPT_NET
│       └── Program.cs                     # dry-run por defecto; `-- --execute` para escribir
│
├── frontend/                              # ✅ Angular 21, standalone, zoneless, Angular Material — ADR 0002 Aceptada
│                                           #    Tema de marca M3 (2026-08-24) + 2 pasadas de UX/UI (2026-08-27).
│                                           #    Pantallas reales contra el backend para Organización, Clínico y
│                                           #    Comercial (OT con vista "Ver Orden", Abonos, Pagos, Cuotas, Cobranza).
│                                           #    Inventario: solo el catálogo de productos — ver src/frontend/CLAUDE.md
│
├── basedatos/                             # Scripts SQL Server — esquema completo + datos iniciales
│   ├── 001_esquema_inicial.sql            # ✅ 17 tablas OPT_+singular + seeds Región/Comuna/Rol
│   ├── 002_catalogos.sql                  # ✅ +3 tablas (EstadoOT/FormaPago/CategoriaProducto), Rol extendido a 8 filas, 3 FKs nuevas — total 20 tablas
│   ├── 003_extras_cliente_receta.sql      # ✅ +5 columnas (sin tablas nuevas): Cliente.FechaNacimiento/TipoPrevision, RecetaCristales.DpLejos/DpCerca/AddLejos — previo a migrar datos clínicos (2026-08-26)
│   ├── 004_comercial_pagos_cuotas.sql     # ✅ +3 tablas (EstadoCuota/Pago/Cuota) + 4 columnas de OrdenDeTrabajo + CHEQUE — total 23 tablas (2026-08-27)
│   ├── 005_ot_publicid_estado_anulado.sql # ✅ +OrdenDeTrabajo.PublicId (12.578 filas pobladas) + estado ANULADO (2026-08-27)
│   ├── 006_receta_ot_detalle_comentario.sql # ✅ +RecetaCristales.OrdenDeTrabajoId (FK+índice) y +DetalleOT.Comentario (2026-08-28, ADR 0008)
│   ├── migracion/                         # ⚠ CARGA DE DATOS legacy — NO es esquema, no es idempotente
│   │   ├── M004_datos_comercial.sql       # ✅ Módulo Comercial completo + los 4.018 productos que usa el detalle
│   │   └── M006_backfill_receta_ot_detalle_comentario.sql  # ✅ 12.574 vínculos receta↔OT y 11.168 comentarios
│   └── README.md                          # Esquema vs. migración, estado de cada script y qué NO hacer
│
└── documentos/                            # TODA la documentación entregable del proyecto (no código) — única carpeta, no existe docs/ en la raíz
    ├── OPT_Propuesta_Arquitectura.docx
    ├── Manual_Tecnico_Backend_OPT.docx     # Manual técnico de BD y Backend
    ├── Manual_Tecnico_Frontend_OPT.docx
    ├── Diccionario_Datos_OPT.docx
    ├── Manual_Tecnico_UX_OPT.docx
    └── README.md
```

---

## Reglas para todo código en src/

1. Todo código nuevo va en el scaffold Fase 0, nunca en la iteración previa ni en `old/`.
2. Al migrar lógica legacy, leer `old/Fuente/` y `.agents/context/reglas-negocio-legado.md` para entender el comportamiento existente antes de reescribirlo.
3. Los contratos frontend ↔ backend se coordinan en `src/documentos/` (documentación de API a crear ahí cuando haya endpoints implementados).
4. Al migrar **datos** (no lógica) en `OPT.Migracion`, leer `.agents/context/migracion-datos-legacy.md` primero — tiene los gotchas de datos reales ya encontrados (ids de catálogo que no coinciden entre legacy y nuevo, RUTs inconsistentes, flags "únicos" duplicados) para no repetir el mismo perfilado desde cero.

## Convención para agregar un nuevo caso de uso

Toda nueva funcionalidad de negocio sigue el patrón de `Auth/Commands/Login/`:

```
OPT.Application/Features/<Modulo>/<Commands|Queries>/<NombreAccion>/
    <NombreAccion>Command.cs          # record : IRequest<TResult>
    <NombreAccion>CommandHandler.cs   # : IRequestHandler<TCommand, TResult>
    <NombreAccion>CommandValidator.cs # : AbstractValidator<TCommand>
```

El controller correspondiente solo llama `await mediator.Send(command, ct)` — sin try/catch, sin lógica de negocio.

## Base de datos — contexto actual

- **Motor:** SQL Server
- **Nombre BD (desarrollo):** `dbOPT_NET`
- **Gestión de esquema:** scripts SQL versionados en `src/basedatos/`, generados desde el modelo EF Core vía `dotnet ef migrations script` (procedimiento completo en `CLAUDE.md`, sección "Base de datos"). No se usa `dotnet ef database update` ni se versiona la carpeta `Migrations/` de EF Core.
- **Naming:** todas las tablas usan prefijo `OPT_` + nombre en singular (`OPT_Cliente`, `OPT_OrdenDeTrabajo`) — ADR `0004`.
- **Identificador público:** `Cliente`, `Empresa`, `Usuario`, `Anamnesis`, `RecetaCristales` y `OrdenDeTrabajo` exponen `PublicId` (Guid) en vez del `Id` interno en cualquier API — ADR `0004`. Sus subrecursos (`DetalleOT`, `Abono`, `Pago`, `Cuota`, `BitacoraOT`) usan el Id interno: solo se alcanzan anidados bajo la OT — ADR `0007`.
- **Estado:** 23 tablas aplicadas y verificadas en `dbOPT_NET` (scripts `001` a `006`; `003`, `005` y `006` solo agregan columnas). Detalle completo en `src/documentos/Diccionario_Datos_OPT.docx` y estado script por script en `src/basedatos/README.md`.
- **Scripts escritos a mano:** `004`, `005` y `006` no se generaron con `dotnet ef migrations script`. El modelo de EF Core y la base **están alineados** porque en cada caso las entidades y sus `IEntityTypeConfiguration` se actualizaron en la misma sesión — mantener esa regla ante cualquier script nuevo escrito a mano.
- **Región/Comuna:** ya estaban sembradas (16 regiones, 346 comunas) pero sin API — desde 2026-08-25 tienen endpoints de solo lectura (`GET /api/regiones`, `GET /api/comunas?regionId={id}`), mismo patrón que `Rol` (`CatalogEntity`, sin Commands).
- **Datos:** Organización — Sucursal (3), Empresa (491), Usuario (13), UsuarioSucursal (19) y EmpresaSucursal (883), migrados 2026-08-24. Clínico — Cliente (11.881), Anamnesis (1.261) y RecetaCristales (13.183), migrados 2026-08-26. Ambos vía `OPT.Migracion`. Comercial — Producto (4.018), OrdenDeTrabajo (12.578), DetalleOT (20.573), BitacoraOT (31.964), Abono (3.277), Pago (3.736) y Cuota (34.110), migrados 2026-08-27 con **SQL directo** (`migracion/M004`, ADR `0006`), más el backfill `M006` del 2026-08-28. ⚠ `OPT.Migracion` **no sabe** que el módulo Comercial ya está migrado: si se le agrega esa fase, debe respetar el guard "si el destino ya tiene OT, omitir". Pendiente: `ProductoSucursal` (13.776) y los 659 productos que ningún detalle referencia — ver `.agents/context/migracion-datos-legacy.md`.

## Próximo script esperado en src/basedatos/

El próximo cambio de esquema se numera `007_descripcion.sql`, y si además carga datos del legacy, su script de datos va aparte como `migracion/M007_descripcion.sql` (los dos grupos no se mezclan — ver `src/basedatos/README.md`). No hay uno planificado todavía; el candidato natural es la migración de `ProductoSucursal`, si revelara la necesidad de una columna o índice nuevo.
