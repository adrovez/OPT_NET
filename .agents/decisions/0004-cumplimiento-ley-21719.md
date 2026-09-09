# 0004 — Cumplimiento Ley N° 21.719 (Protección de Datos Personales) y convenciones asociadas de esquema

**Estado:** Aceptada
**Fecha:** 2026-08-19

## Contexto

La Ley N° 21.719, que reforma la Ley N° 19.628 sobre protección de datos personales, fue publicada en el Diario Oficial el 13-12-2024 y su implementación es obligatoria desde el 01-12-2026 — dentro del horizonte de puesta en producción de OPT. Aplica a toda organización, pública o privada, que trate datos personales en Chile, sin distinción de tamaño (con un régimen transitorio de solo amonestaciones para PYMEs entre diciembre 2026 y diciembre 2027). Crea la Agencia de Protección de Datos Personales (APDP), exige notificación de brechas de seguridad en 72 horas, y contempla multas de hasta 20.000 UTM o 4% de los ingresos anuales en caso de reincidencia. [Fuentes: preyproject.com, thomsonreuters.cl, ciberlex.cl — ver enlaces al final]

La ley clasifica como **datos sensibles** (con estándar de protección reforzado) los datos de salud, biométricos, perfil biológico, origen étnico o racial, afiliación política/sindical/gremial, situación socioeconómica, convicciones ideológicas o religiosas, y vida sexual/orientación sexual/identidad de género. Amplía los derechos ARCO (Acceso, Rectificación, Cancelación, Oposición) agregando **Portabilidad** — de ahí "ARCO+".

En el dominio de OPT, esto es directamente relevante:

- `Anamnesis` (hipertensión, diabetes, alergias, uso previo de lentes) es **dato de salud** → dato sensible.
- `RecetaCristales` (graduación óptica por ojo) es **dato de salud/biométrico-adyacente** → dato sensible.
- `Cliente` (RUT, nombre, dirección, teléfono, email) y `Empresa` (RUT, razón social, contacto) son **datos personales** de identificación directa, no sensibles per se, pero de alto impacto si se filtran o se enumeran masivamente.
- `Usuario` combina dato personal (RUT, nombre, email) con credenciales de acceso al sistema — superficie de ataque adicional si sus registros son enumerables.

Este ADR se abre a partir de una revisión del esquema generado en la sesión anterior (`src/basedatos/001_esquema_inicial.sql`), donde se identificó que los identificadores internos (`int IDENTITY`) de estas tablas, si se exponen tal cual en URLs o respuestas de API, permiten enumerar clientes/usuarios de forma trivial (`/api/clientes/1`, `/api/clientes/2`, ...) — un riesgo de seguridad concreto sobre datos personales y sensibles que la Ley 21.719 obliga a mitigar (principio de seguridad, art. 14 y siguientes del texto refundido).

## Alternativas consideradas

**Sobre el identificador expuesto externamente:**
- **Mantener el `int IDENTITY` como único identificador, también en API/URLs**: descartado — es el statu quo que motiva este ADR; permite enumeración trivial de registros con datos personales y sensibles.
- **Reemplazar el `int IDENTITY` por `GUID` como clave primaria**: evaluada y descartada por ahora — cambia el tipo de dato de la PK y de **todas** las FK que la referencian (`OrdenDeTrabajo.ClienteId`, `Anamnesis.ClienteId`, `RecetaCristales.ClienteId`, `EmpresaSucursales.EmpresaId`, `UsuarioSucursales.UsuarioId`, etc.), con impacto en performance de `JOIN`/clustering (los GUID aleatorios fragmentan el índice clustered) y en todo el dominio C# (tipos `int` → `Guid` en cascada). El beneficio de seguridad se puede obtener con menor costo con la alternativa siguiente.
- **Mantener `int IDENTITY` como PK interna (clustering, joins, FKs sin cambios) y agregar una columna `Guid PublicId` única adicional, generada en la base de datos, que es la que se expone en API/URLs**: **seleccionada**. Es el patrón estándar para este problema — separa el identificador de almacenamiento (optimizado para el motor) del identificador de exposición externa (optimizado para no ser adivinable/enumerable).

**Sobre el alcance de `PublicId`:**
- **Solo `Cliente` y `Empresa`** (alcance inicialmente planteado): descartado por insuficiente — `Usuario` expone credenciales y `Anamnesis`/`RecetaCristales` son datos de salud con el mismo riesgo de enumeración si se listan/consultan por Id secuencial vía API.
- **Extender a `Usuario`, `Anamnesis` y `RecetaCristales`**: **seleccionada**, por tratarse de datos sensibles o de alto impacto bajo el mismo criterio.

**Sobre el alcance del cumplimiento a implementar ahora:**
- **Implementar el programa de cumplimiento completo de la ley (consentimiento explícito para datos sensibles, política de retención/purga real para el derecho de cancelación, registro de actividades de tratamiento, proceso de notificación de brechas, evaluación de necesidad de Delegado de Protección de Datos) en esta misma sesión**: descartada — son decisiones de producto/negocio y de proceso operativo que exceden el alcance de un cambio de esquema y requieren validación legal/de negocio que no corresponde asumir unilateralmente en el código.
- **Documentar ahora la medida técnica de seguridad (identificador no enumerable) que sí depende directamente del diseño del esquema, y dejar el resto del programa de cumplimiento como decisión explícitamente pendiente**: **seleccionada**.

## Decisión

1. **Identificador público no enumerable**: `Cliente`, `Empresa`, `Usuario`, `Anamnesis` y `RecetaCristales` agregan una columna `PublicId` de tipo `uniqueidentifier` (`Guid` en el dominio C#), `NOT NULL`, con valor generado en la base de datos (`DEFAULT NEWID()`) y restricción `UNIQUE`. El `int IDENTITY` existente **se mantiene como clave primaria** — no cambia ninguna FK ni el tipo de las relaciones actuales. Toda superficie pública (rutas de API, respuestas JSON, tokens de recursos) debe usar `PublicId`, nunca el `Id` interno — esto se aplicará al implementar los controllers/DTOs de cada módulo (Fase 1 en adelante), no en este ADR.
2. **Convención de nombres de tabla**: las 16 tablas del esquema (incluidos los catálogos `Región`, `Comuna`, `Rol`) se renombran con prefijo `OPT_` y en singular (`OPT_Cliente`, `OPT_Empresa`, `OPT_OrdenDeTrabajo`, `OPT_Region`, etc.), alineado con la convención ya usada en el legacy (`OPT_Cliente`, `OPT_Empresa`) para facilitar la trazabilidad durante la migración de datos (Fase 6) y la identificación de las tablas de este sistema en una base de datos compartida. Esta decisión es independiente de la Ley 21.719 — es una convención de nomenclatura — pero se registra en el mismo ADR por haberse acordado en la misma sesión de trabajo.
3. **Datos sensibles identificados**: `Anamnesis` y `RecetaCristales` quedan formalmente clasificados como tablas con datos sensibles de salud bajo Ley 21.719. Esta clasificación debe guiar decisiones futuras de acceso (autorización más estricta que el resto del sistema), cifrado en reposo (a evaluar — SQL Server `Always Encrypted` u otro mecanismo, fuera de alcance de este ADR) y minimización de datos en logs/reportes.

## Pendiente — explícitamente fuera de alcance de este ADR

Estos puntos requieren una decisión de negocio/legal antes de implementarse en código, y deben resolverse antes de que OPT entre en producción (la ley es obligatoria desde el 01-12-2026):

- **Derecho de cancelación real vs. borrado lógico**: el diseño actual (`Eliminado` bit, ver ADR `0003`) nunca elimina físicamente un registro. Esto es insuficiente por sí solo para el derecho de cancelación de la Ley 21.719 sobre datos de `Cliente`/`Anamnesis`/`RecetaCristales`. Se requiere definir una política de retención (cuánto tiempo se conserva un registro "eliminado" antes de purga o anonimización real) y si la purga es borrado físico o anonimización irreversible (para no romper la integridad referencial de `OrdenDeTrabajo`/`BitacoraOT` históricas).
- **Consentimiento explícito**: si el tratamiento de `Anamnesis`/`RecetaCristales` requiere capturar y trazar el consentimiento del cliente (campo(s) en `Cliente` o tabla de consentimientos separada, con fecha y alcance), y bajo qué base de licitud (consentimiento vs. ejecución de un contrato/prestación de servicios de salud visual).
- **Registro de actividades de tratamiento y base de licitud** por cada finalidad (atención clínica, cobranza, marketing si existiera).
- **Proceso de notificación de brechas en 72 horas**: es un proceso operativo (incident response), no un cambio de esquema — pendiente de definir con el equipo de infraestructura/seguridad.
- **Necesidad de Delegado de Protección de Datos (DPO)**: no es obligatorio salvo que la empresa adopte voluntariamente un modelo de prevención de infracciones; dado el volumen de datos de salud que trata OPT, se recomienda que el equipo evalúe esta decisión con asesoría legal.
- **Portabilidad de datos** (parte de ARCO+): definir el formato de exportación de los datos de un `Cliente` (incluye `Anamnesis`, `RecetaCristales`, historial de `OrdenDeTrabajo`) cuando se ejercite este derecho.

## Revisión de cobertura de `PublicId` (2026-08-27) — resuelta el mismo día para `OrdenDeTrabajo`

> **Actualización (2026-08-27, sesión de la API del módulo Comercial):** la deuda de prioridad alta quedó **cerrada**. `OPT_OrdenDeTrabajo` tiene `PublicId` (script `005_ot_publicid_estado_anulado.sql`, 12.578 filas pobladas y verificadas) y toda su API lo usa. Para `Abono`, `Pago`, `Cuota` y `DetalleOT` se decidió **no** agregarlo: se exponen como subrecursos bajo `/api/ordenes-de-trabajo/{publicId}/...`, es decir bajo un padre ya protegido, que es exactamente la excepción que contempla el criterio de más abajo. Ver ADR `0007`. La tabla original de la revisión se conserva como registro del análisis.

Al terminar la migración del módulo Comercial (ADR `0006`) se revisó, tabla por tabla contra `dbOPT_NET`, cuáles de las 23 tablas actuales deberían tener `PublicId` y no lo tienen. **Ninguna de las 5 originales cambia; lo que sigue es trabajo pendiente.**

El criterio para decidir es doble — hacen falta las dos condiciones:

1. La entidad es un **recurso direccionable de primer nivel** en la API (tiene su propia ruta `/api/{recurso}/{id}`), no un hijo que solo se accede anidado bajo un padre ya protegido.
2. Enumerar su id **filtra datos personales, sensibles o financieros** de una persona identificable.

### Cobertura actual

| Tabla | `PublicId` | Veredicto |
|---|---|---|
| `Cliente`, `Empresa`, `Usuario`, `Anamnesis`, `RecetaCristales` | ✅ | Ya cubiertas por este ADR |
| **`OrdenDeTrabajo`** | ✅ (desde 2026-08-27) | **Debía tenerlo — prioridad alta.** Resuelto en el ADR `0007` |
| **`Abono`**, **`Pago`**, **`Cuota`** | ❌ | Se decidió que **no corresponde**: quedaron como subrecursos de la OT (ADR `0007`) |
| `DetalleOT` | ❌ | Evaluado al diseñar la API: se manipula como parte del agregado OT, no lo necesita |
| `BitacoraOT`, `ProductoSucursal` | ❌ | No corresponde |
| `Producto`, `Sucursal` | ❌ | No corresponde (por ahora) |
| Los 7 catálogos, `EmpresaSucursal`, `UsuarioSucursal` | ❌ | No corresponde |

### Justificación

- **`OrdenDeTrabajo` (12.578 filas) — el hueco real.** Es un recurso de primer nivel y vincula a un cliente con su atención clínica: enumerar `/api/ordenes/1,2,3...` expone la relación paciente↔prestación, que es dato de salud bajo la Ley 21.719. Que `Cliente` tenga `PublicId` no sirve de nada si la OT que lo referencia se puede recorrer con un `int`. **Agregarlo antes de publicar el primer endpoint de OT**, no después: cambiarlo luego rompe URLs ya emitidas.
- **`Abono`, `Pago`, `Cuota` (3.277 / 3.736 / 34.110).** Van a ser direccionables por acciones propias (anular un pago, marcar una cuota como pagada), y su id enumerado filtra el historial financiero de una persona. Menor prioridad que la OT solo porque probablemente se expongan anidados bajo ella.
- **`DetalleOT` (20.573).** Depende del diseño de la API: si las líneas se manipulan como parte del agregado OT (`PUT /api/ordenes/{publicId}` con la lista completa) o anidadas bajo una ruta ya protegida, no hace falta. Si terminan siendo un recurso propio, aplica el mismo criterio que `Abono`.
- **`BitacoraOT` (31.964) y `ProductoSucursal`.** Solo se leen anidadas bajo su padre; nunca se direccionan solas. La protección la da la autorización del padre.
- **`Producto` (4.018) y `Sucursal` (4).** Son recursos de primer nivel, pero no contienen datos personales: enumerarlos filtra a lo sumo el catálogo comercial, que es un riesgo de negocio, no de Ley 21.719. `Sucursal` además ya usa el `Id` interno en su API por decisión tomada (sesión 2026-08-24). Reconsiderar solo si el negocio califica el catálogo de productos y precios como información confidencial.
- **Catálogos y tablas de relación.** Fuera de alcance de este ADR por definición: son datos de referencia públicos, sembrados, sin sujeto de datos.

### Un matiz importante sobre `NumeroOT`

`OrdenDeTrabajo.NumeroOT` es un correlativo **visible por diseño** — va impreso en el ticket que se lleva el cliente. Ahí `PublicId` no evita nada: quien conozca un número de OT puede probar los vecinos. Esa superficie se protege con **autorización en el endpoint** (verificar que el usuario tenga acceso a la sucursal de esa OT), no con un identificador opaco. Las dos medidas son complementarias, no alternativas.

### Costo estimado

Agregar `PublicId` a `OrdenDeTrabajo` es barato y **no toca ninguna FK**: una columna `uniqueidentifier NOT NULL DEFAULT NEWID()` + índice único, un `UPDATE` para poblar las filas ya migradas, la propiedad en la entidad de dominio y su `IEntityTypeConfiguration<T>`, y los controllers/DTOs resolviendo por `PublicId`. Lo mismo para `Abono`/`Pago`/`Cuota`. Cambiar la **clave primaria** a `Guid`, en cambio, obligaría a reescribir 5 tablas hijas con sus FKs e índices, el modelo de EF Core y la migración de datos ya ejecutada — sin ganar nada que `PublicId` no dé ya. **No se hace.**

## Consecuencias

- El esquema gana una columna adicional (`PublicId`) e índice único en 5 tablas — impacto de almacenamiento e inserción marginal (un `uniqueidentifier` de 16 bytes por fila), sin afectar el `IDENTITY` clustered ni las FK existentes.
- Todo endpoint de API que exponga estas 5 entidades debe resolverse por `PublicId`, no por `Id` — esto es una regla de código a aplicar desde la Fase 1 (casos de uso de Clientes) en adelante, documentada aquí para que no se pierda al implementar los controllers.
- El renombrado de tablas (`OPT_` + singular) obliga a regenerar `src/basedatos/001_esquema_inicial.sql` y actualizar los `.ToTable(...)` en las 16 `IEntityTypeConfiguration<T>` existentes — cambio mecánico, sin impacto en el dominio C# (las clases de entidad ya están en singular).
- Este ADR dejó explícitamente pendientes varias decisiones de cumplimiento (retención/purga, consentimiento, DPO) que son bloqueantes para producción bajo la Ley 21.719 pero no para continuar el desarrollo de Fase 1 — deben resolverse antes de la fecha de entrada en vigencia (01-12-2026).

## Fuentes consultadas

- [Ley 21.719: guía 2026 para cumplir con la ley de protección de datos en Chile — Prey Project](https://preyproject.com/es/blog/ley-de-proteccion-de-datos-en-chile)
- [Ley Nº 21.719 y la Reconstrucción del Derecho Chileno de Protección de Datos Personales — Thomson Reuters](https://www.thomsonreuters.cl/es-cl/soluciones-juridicas/biblioteca-contenido-legal/ley-21719-y-la-reconstruccion-del-derecho-chileno-de-proteccion-de-datos-personales)
- [Derechos ARCO+ en la Nueva Ley 21.719: Guía Práctica para Empresas Chilenas — Ciberlex](https://ciberlex.cl/derechos-arco-ley-21719-guia-empresas-chile/)
