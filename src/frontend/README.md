# OPT — Frontend

Aplicación Angular (SPA) que consume la API de `OPT.API` (.NET 8). Ver `AGENTS.md` y `CLAUDE.md`
en la raíz del repo para el contexto completo del proyecto; este documento cubre solo el frontend.

Documentación relacionada: [`CLAUDE.md`](CLAUDE.md) (convenciones prescriptivas para generar código
con IA) y `../documentos/Manual_Tecnico_Frontend_OPT.docx` (manual técnico extendido).

## Stack

| Componente | Elección | Motivo |
|---|---|---|
| Framework | Angular 21, standalone, **zoneless** | ADR `0002`. Sin NgModules, sin zone.js — signals como mecanismo de reactividad y detección de cambios. |
| UI | Angular Material 21 (Material 3, tema custom aplicado — ver `.agents/context/branding-ux-ui.md`) | Integración nativa con signals/CDK; suficiente para listados, formularios y modales de un sistema chico sin la sobrecarga de un kit de terceros. |
| Estado | Signals + servicios `providedIn: 'root'` | No se justifica NgRx/otro store para el volumen de este sistema. |
| HTTP | `HttpClient` + interceptores funcionales | `authInterceptor` agrega el JWT; `errorInterceptor` traduce `ProblemDetails` y maneja 401. |
| Testing | Vitest (unit) | Runner por defecto de Angular CLI 21. |
| Lint/format | ESLint (`angular-eslint`) + Prettier | `npm run lint`, `npm run format`. |

## Arrancar en desarrollo

```bash
npm install
npm start          # ng serve — http://localhost:4200
```

Requiere que `OPT.API` esté corriendo (`dotnet run` en `src/backend/OPT.API`) con CORS habilitado
para `http://localhost:4200` (ya configurado en `appsettings.Development.json`). La URL del backend
se ajusta en [src/environments/environment.development.ts](src/environments/environment.development.ts)
— apunta al puerto fijo de `launchSettings.json` (`https://localhost:63595/api`); si tu `dotnet run`
imprime un puerto distinto (perfil o `launchSettings.json` local diferente), actualizar ese archivo.

```bash
npm run build       # build de producción → dist/opt-frontend
npm test            # unit tests (Vitest)
npm run lint        # ESLint
npm run format       # Prettier — aplica formato
npm run format:check # Prettier — solo verifica
```

## Estructura

```
src/app/
├── core/                  # Transversal, un solo consumidor: la app entera
│   ├── guards/             # authGuard (functional CanActivateFn)
│   ├── interceptors/       # authInterceptor, errorInterceptor (functional)
│   ├── models/             # Contratos exactos de la API (LoginRequest/Response, JwtClaims)
│   ├── services/           # Auth (sesión), TokenStorage
│   └── utils/               # decodeJwtPayload
│
├── shared/                # Reutilizable entre features, sin lógica de negocio propia
│   ├── models/              # ApiProblemDetails (contrato de ExceptionHandlingMiddleware)
│   └── services/            # Toast (MatSnackBar — reemplaza SweetAlert del legacy)
│
├── layout/                 # Cascarones de página, sin lógica de negocio
│   ├── shell/                # Toolbar con navbar horizontal + router-outlet — zona autenticada
│   └── auth-layout/          # Contenedor centrado — zona pública (login)
│
├── features/                # Un folder por módulo de negocio, mapeado 1:1 a un Controller del backend
│   ├── auth/                  # login/ + auth.routes.ts
│   ├── clientes/               # pages/ + services/ + models/ + clientes.routes.ts
│   ├── sucursales/              # CRUD completo (lista + diálogo alta/edición), Id interno en rutas
│   ├── empresas/                # CRUD completo (lista + diálogo alta/edición), PublicId en rutas
│   ├── roles/                   # Solo lectura (catálogo sembrado, sin diálogo de alta/edición)
│   ├── usuarios/                 # CRUD + diálogos de clave/sucursales, PublicId en rutas
│   ├── anamnesis/                # Sin listado propio — se consume desde las pestañas de cliente-ficha
│   ├── receta-cristales/          # Ídem + components/receta-graduacion (compartido con la ficha de la OT)
│   ├── regiones/ comunas/          # Catálogos de solo lectura (selector Región→Comuna)
│   ├── ordenes-de-trabajo/          # Agregado Comercial: listado, asistente de alta, ficha "Ver Orden" y ticket
│   ├── abonos/ pagos/ cuotas/        # Pantallas propias sobre subrecursos de la OT (no hay /api/pagos)
│   ├── cobranza/                      # Deudores por empresa convenio
│   ├── operativos/                     # Agrupa OT de una jornada en terreno + gastos + ganancia/pérdida;
│   │                                    # listado, diálogo de alta/edición y ficha ruteada (Órdenes/Gastos)
│   └── inventario/                     # Solo el catálogo de productos (GET /api/productos); el resto es stub
│
├── app.routes.ts            # Composición: /login (público) vs. shell con children lazy-loaded + authGuard
└── app.config.ts            # providers raíz: router, http (+interceptores), animaciones, zoneless
```

### Convención para agregar un feature nuevo

Sigue el patrón de `features/clientes/` (CRUD completo con paginación server-side — la plantilla del proyecto):

```
features/<modulo>/
    models/<entidad>.model.ts       # Refleja 1:1 los campos reales de la entidad en OPT.Domain
    services/<modulo>.ts             # Único punto de acceso HTTP — los componentes nunca llaman a HttpClient directo
    pages/<pagina>/                  # Componentes standalone, lazy-loaded vía loadComponent
    <modulo>.routes.ts               # Routes exportadas, importadas con loadChildren desde app.routes.ts
```

Generar con Angular CLI para mantener la convención de nombres 2025 (`ng g component features/x/pages/y`,
`ng g service features/x/services/x`) — no crear archivos a mano salvo el contenido interno.

## Decisiones específicas del frontend

- **Sesión**: el JWT se guarda en `sessionStorage` (no `localStorage`) — se pierde al cerrar la
  pestaña, acotando la ventana de exposición para un sistema con datos clínicos sensibles
  (Ley 21.719). No se persiste nada más que el token.
- **Sin endpoint `/me`**: como el backend aún no lo expone, `Auth` reconstruye la sesión
  decodificando los claims del propio JWT al recargar la página (`core/services/auth.ts`).
  Si más adelante se agrega `/me`, reemplazar `restaurarSesion()` por una llamada real.
- **`PublicId` en rutas**: todo servicio que hable con `Cliente`, `Empresa`, `Usuario`, `Anamnesis`,
  `RecetaCristales` u `OrdenDeTrabajo` usa el `publicId` (Guid) en la URL, nunca un id numérico
  (ADR `0004`). Los subrecursos de la OT (detalle, abono, pago, cuota, bitácora) sí llevan su Id
  interno: solo se alcanzan anidados bajo la orden (ADR `0007`).
- **Sin NgRx**: signals + un servicio `providedIn: 'root'` por feature alcanzan para el volumen de
  datos y de pantallas de este sistema. Reconsiderar solo si aparece estado compartido complejo
  entre features no relacionados.
- **El módulo Comercial vive repartido en cinco features, pero es un solo agregado**: los modelos, los
  servicios y los componentes compartidos están en `features/ordenes-de-trabajo/`, y `abonos`, `pagos`,
  `cuotas` y `cobranza` los importan de ahí. En la API no existen `/api/pagos` ni `/api/cuotas` — son
  subrecursos de la OT (ADR `0007`).
- **La ficha de la OT replica la vista "Ver Orden" del legacy** (ADR `0008`): cabecera de la orden arriba
  y pestañas Cliente / Receta / Detalle / Abonos —las del legacy— más Pagos / Cuotas / Bitácora. Su
  listado **no consulta al entrar** (más de 12.000 órdenes): espera una búsqueda, un filtro, o un filtro
  de contexto en la URL.
- **El alta de una OT es un asistente por pasos** (ADR `0009`, rediseñado 2026-09-08 en ADR `0010`):
  `mat-stepper` con Cliente / Receta / Detalle / Pago, lineal al crear y no lineal al editar (donde el
  paso Pago no existe). Permite crear el cliente y tomar la receta sin abandonar la orden —reabriendo
  los diálogos `ClienteForm` y `RecetaCristalesForm`, no duplicando sus campos— y cierra con
  `orden-creada-dialog`, que muestra el ticket imprimible (`app-ticket-ot`, el reemplazo del reporte
  `.rdlc` del legacy). Las reglas de `@media print` viven en `src/styles.scss`, nunca en el SCSS de un
  componente. El paso Cliente concentra toda la cabecera (fechas de atención/entrega, hora de entrega,
  beneficiario); el paso Receta ofrece un combo con la receta más reciente del cliente dentro de los
  últimos 3 meses (preseleccionada al crear) más un historial completo opcional; el paso Pago pide
  elegir explícitamente una de tres modalidades (`modalidadPago`: total / abono + cuotas / solo cuotas)
  en vez del checkbox "Sin plan de cuotas" que se retiró, y previsualiza la tabla de cuotas (mensuales,
  `AddMonths`) antes de guardar. La ficha, además, bloquea la edición con un aviso visible cuando la OT
  está `Entregado` o anulada.
- **`features/inventario` está a medias**: solo el catálogo de productos tiene backend
  (`GET /api/productos`, el que alimenta el selector del detalle de una OT). El resto de
  `InventarioController` sigue siendo un stub — completar el patrón de `clientes/` cuando se implemente.
- **Alta/edición vía `MatDialog`, no rutas propias**: `Sucursales`, `Empresas` y `Usuarios` abren un
  componente de diálogo (`<entidad>-form`) desde la página de listado en vez de navegar a
  `/nuevo` o `/editar/:id` — evita duplicar layout de página para un formulario chico y es el patrón
  a seguir para el próximo módulo con CRUD simple. `shared/components/confirm-dialog/` es el diálogo
  de confirmación genérico reutilizado por las 3 acciones de eliminar (y por activar/desactivar
  Usuario) — reutilizarlo en vez de crear uno nuevo por feature.
- **Límite real de la API de Usuario**: `UsuarioDto` solo expone `sucursalActivaId` (la sucursal
  activa, que además solo la fija el login — no hay endpoint para setearla manualmente), no la lista
  completa de sucursales asignadas. El diálogo `usuario-sucursal-dialog` asigna/quita sin poder
  mostrar qué sucursales tiene el usuario hoy — es una limitación de la API, no un bug del frontend.
- **Branding/tema**: el tema Angular Material M3 (`src/theme-colors.scss` + `src/styles.scss`) y los
  tokens de marca sin equivalente M3 (`src/theme-tokens.scss` — colores de `OPT_EstadoOT`/`OPT_FormaPago`,
  tipografía de titulares) ya están aplicados. Antes de escribir CSS/estilos nuevos, leer la sección
  "Implementación en código" de `.agents/context/branding-ux-ui.md` — consumir esas variables, no
  valores hex sueltos.
