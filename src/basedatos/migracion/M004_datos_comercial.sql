/* =============================================================================
   M004_datos_comercial.sql
   -----------------------------------------------------------------------------
   Migracion de datos del modulo Comercial (+ los productos que necesita)
   desde el legacy `db_a25cfd_opt2` hacia `dbOPT_NET`.

   Ejecutar CONECTADO A dbOPT_NET, en la misma instancia que el legacy.
   Requiere haber aplicado antes `../004_comercial_pagos_cuotas.sql`.

   Alcance (decidido con el usuario en la sesion 2026-08-27):
     1. OPT_Producto        -- SOLO los 4.018 productos referenciados por algun
                               detalle de OT (prerequisito: DetalleOT.ProductoId
                               es FK NOT NULL). NO se migra ProductoSucursal.
     2. OPT_OrdenDeTrabajo  -- 12.578
     3. OPT_DetalleOT       -- 20.573
     4. OPT_BitacoraOT      -- 31.964
     5. OPT_Abono           --  3.277
     6. OPT_Pago            --  3.736
     7. OPT_Cuota           -- 34.110
     8. Recalculo de OT.TotalAbonado / OT.Saldo = abonos + pagos

   Reglas de mapeo relevantes:
     - NumeroOT               = legacy idOT (se preserva; tiene indice unico).
     - ClienteId              por Rut (11.881/11.881 resuelven).
     - SucursalId             por Nombre (3/3).
     - EmpresaId              por correspondencia ordinal legacy idEmpresa ->
                               EmpresaId (los ids NO coinciden: el legacy llega
                               a 5484 y el destino a 491; se verifico que la
                               correspondencia ordinal calza 491/491 por
                               RazonSocial, y el script lo re-verifica y aborta
                               si dejara de calzar).
     - EstadoOTId             = estado de la ULTIMA entrada de bitacora.
     - Bitacora.EstadoAnterior= estado de la entrada previa de la misma OT
                               (la primera queda con anterior = nuevo).
     - CreadoPor              se resuelve por el primer nombre del texto libre
                               legacy (OT.Usuario / Bitacora.Responsable) contra
                               OPT_Usuario.Nombre; sin match -> usuario bootstrap
                               (MONICA CALDERON, SILVANA ASTETE, REGINA CARRASCO,
                               KAREN GOMEZ y 'SIN INFORMACION' no son usuarios).
     - Pago.TipoPago (texto)  -> OPT_FormaPago por nombre; sin match -> 0 (SIN
                               INFORMACION). 'CHEQUE' existe gracias al 004.
     - Cuota.Estado           -> OPT_EstadoCuota (todas PENDIENTE en el legacy).
     - Saldo / TotalAbonado   se RECALCULAN: el legacy no descontaba OPT_Pago,
                               3.472 OT tenian el saldo inflado.

   Datos sucios que se preservan tal cual (no hay CHECK constraints en destino):
     23 pagos, 20 abonos y 5 cuotas con monto <= 0; 33 detalles con Cantidad <= 0;
     18 OT sin Precio (se migran con Precio = 0).

   Idempotencia: aborta si el destino ya tiene OT, salvo que se ponga @Force = 1.
   Todo corre en una sola transaccion (rollback total ante cualquier error).
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @Force bit = 0;   -- poner en 1 para re-ejecutar sobre un destino con datos

/* --- Guardas ------------------------------------------------------------- */
DECLARE @BaseActual sysname = DB_NAME();
IF @BaseActual <> N'dbOPT_NET'
BEGIN
    RAISERROR('Este script debe ejecutarse conectado a dbOPT_NET (actual: %s).', 16, 1, @BaseActual);
    RETURN;
END;

IF OBJECT_ID(N'[OPT_Pago]', N'U') IS NULL OR OBJECT_ID(N'[OPT_Cuota]', N'U') IS NULL
BEGIN
    RAISERROR('Faltan OPT_Pago / OPT_Cuota. Aplicar primero 004_comercial_pagos_cuotas.sql.', 16, 1);
    RETURN;
END;

IF @Force = 0 AND EXISTS (SELECT 1 FROM [OPT_OrdenDeTrabajo])
BEGIN
    RAISERROR('OPT_OrdenDeTrabajo ya tiene filas. Poner @Force = 1 si la re-ejecucion es intencional.', 16, 1);
    RETURN;
END;

DECLARE @UsuarioBootstrap int = (SELECT MIN(UsuarioId) FROM [OPT_Usuario]);
DECLARE @CategoriaGeneral int = (SELECT MIN(Id) FROM [OPT_CategoriaProducto]);

IF @UsuarioBootstrap IS NULL OR @CategoriaGeneral IS NULL
BEGIN
    RAISERROR('Falta el usuario bootstrap o la categoria de producto. Migrar antes Organizacion.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

/* =============================================================================
   0. TABLAS DE MAPEO
   ============================================================================= */

/* --- Empresa: correspondencia ordinal legacy idEmpresa -> EmpresaId -------- */
CREATE TABLE #MapEmpresa (idEmpresa int PRIMARY KEY, EmpresaId int NOT NULL);

WITH l AS (
    SELECT idEmpresa, Empresa, ROW_NUMBER() OVER (ORDER BY idEmpresa) AS rn
    FROM db_a25cfd_opt2.dbo.OPT_Empresa
), d AS (
    SELECT EmpresaId, RazonSocial, ROW_NUMBER() OVER (ORDER BY EmpresaId) AS rn
    FROM OPT_Empresa
)
INSERT INTO #MapEmpresa (idEmpresa, EmpresaId)
SELECT l.idEmpresa, d.EmpresaId
FROM l
JOIN d ON d.rn = l.rn;

/* Verificacion dura: si el mapeo ordinal no calza por nombre, abortar. */
IF EXISTS (
    SELECT 1
    FROM #MapEmpresa m
    JOIN db_a25cfd_opt2.dbo.OPT_Empresa le ON le.idEmpresa = m.idEmpresa
    JOIN OPT_Empresa e ON e.EmpresaId = m.EmpresaId
    WHERE e.RazonSocial <> le.Empresa COLLATE DATABASE_DEFAULT
)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('El mapeo ordinal de Empresa no coincide por RazonSocial. Revisar antes de migrar.', 16, 1);
    RETURN;
END;

/* --- Sucursal: por nombre ------------------------------------------------- */
CREATE TABLE #MapSucursal (idSucursal int PRIMARY KEY, SucursalId int NOT NULL);

INSERT INTO #MapSucursal (idSucursal, SucursalId)
SELECT ls.idSucursal, s.SucursalId
FROM db_a25cfd_opt2.dbo.OPT_Sucursal ls
JOIN OPT_Sucursal s ON s.Nombre = ls.Nombre COLLATE DATABASE_DEFAULT;

IF (SELECT COUNT(*) FROM #MapSucursal) <> (SELECT COUNT(*) FROM db_a25cfd_opt2.dbo.OPT_Sucursal)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Hay sucursales legacy sin equivalente por nombre en el destino.', 16, 1);
    RETURN;
END;

/* --- Usuario: primer nombre del texto libre legacy ------------------------ */
CREATE TABLE #MapUsuario (
    Nombre    nvarchar(100) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    UsuarioId int NOT NULL
);

INSERT INTO #MapUsuario (Nombre, UsuarioId)
SELECT n.Nombre, ISNULL(m.UsuarioId, @UsuarioBootstrap)
FROM (
    SELECT DISTINCT LTRIM(RTRIM(Usuario)) COLLATE DATABASE_DEFAULT AS Nombre
    FROM db_a25cfd_opt2.dbo.OPT_OrdenDeTrabajo
    WHERE Usuario IS NOT NULL AND LTRIM(RTRIM(Usuario)) <> ''
    UNION
    SELECT DISTINCT LTRIM(RTRIM(Responsable)) COLLATE DATABASE_DEFAULT
    FROM db_a25cfd_opt2.dbo.OPT_BitacoraOT
    WHERE Responsable IS NOT NULL AND LTRIM(RTRIM(Responsable)) <> ''
) n
OUTER APPLY (
    SELECT TOP (1) u.UsuarioId
    FROM OPT_Usuario u
    WHERE u.Nombre = LEFT(n.Nombre, CHARINDEX(' ', n.Nombre + ' ') - 1)
    ORDER BY u.UsuarioId
) m;

/* =============================================================================
   1. OPT_Producto -- solo los referenciados por algun detalle de OT
   ============================================================================= */
INSERT INTO OPT_Producto
    (Codigo, Descripcion, ControlStock, CategoriaId, CreadoEn, CreadoPor, Eliminado)
SELECT
    p.Codigo,
    p.Producto,
    p.ControlStock,
    @CategoriaGeneral,
    TODATETIMEOFFSET(p.FechaIngreso, 0),
    @UsuarioBootstrap,
    0
FROM db_a25cfd_opt2.dbo.OPT_Producto p
WHERE EXISTS (SELECT 1 FROM db_a25cfd_opt2.dbo.OPT_OrdenDeTrabajoDetalle d WHERE d.idProducto = p.idProducto)
  AND NOT EXISTS (SELECT 1 FROM OPT_Producto x WHERE x.Codigo = p.Codigo COLLATE DATABASE_DEFAULT);

CREATE TABLE #MapProducto (idProducto bigint PRIMARY KEY, ProductoId int NOT NULL);

INSERT INTO #MapProducto (idProducto, ProductoId)
SELECT p.idProducto, np.ProductoId
FROM db_a25cfd_opt2.dbo.OPT_Producto p
JOIN OPT_Producto np ON np.Codigo = p.Codigo COLLATE DATABASE_DEFAULT;

/* =============================================================================
   2. OPT_OrdenDeTrabajo
   ============================================================================= */
INSERT INTO OPT_OrdenDeTrabajo
    (NumeroOT, ClienteId, SucursalId, EstadoOTId, EmpresaId,
     Precio, TotalAbonado, Saldo, Observaciones, FechaEntrega,
     Beneficiario, FechaAtencion, HoraEntrega, NumeroCuotas,
     CreadoEn, CreadoPor, Eliminado)
SELECT
    CAST(o.idOT AS int),
    c.ClienteId,
    ms.SucursalId,
    est.idEstado,
    me.EmpresaId,
    ISNULL(o.Precio, 0),
    0,                                   -- se recalcula en el paso 8
    0,                                   -- se recalcula en el paso 8
    NULL,
    TODATETIMEOFFSET(CAST(o.FechaEntrega AS datetime) + CAST(o.HoraEntrega AS datetime), 0),
    NULLIF(LTRIM(RTRIM(o.Beneficiario)), ''),
    o.FechaAtencion,
    o.HoraEntrega,
    o.NumeroCuota,
    TODATETIMEOFFSET(ISNULL(ini.PrimeraFecha, CAST(o.FechaAtencion AS datetime)), 0),
    ISNULL(mu.UsuarioId, @UsuarioBootstrap),
    0
FROM db_a25cfd_opt2.dbo.OPT_OrdenDeTrabajo o
JOIN OPT_Cliente  c  ON c.Rut = o.RutCliente COLLATE DATABASE_DEFAULT
JOIN #MapSucursal ms ON ms.idSucursal = o.idSucursal
LEFT JOIN #MapEmpresa me ON me.idEmpresa = o.idEmpresa
LEFT JOIN #MapUsuario mu ON mu.Nombre = LTRIM(RTRIM(o.Usuario)) COLLATE DATABASE_DEFAULT
CROSS APPLY (
    SELECT TOP (1) b.idEstado
    FROM db_a25cfd_opt2.dbo.OPT_BitacoraOT b
    WHERE b.idOT = o.idOT
    ORDER BY b.Fecha DESC, b.idBitacoraOT DESC
) est
OUTER APPLY (
    SELECT MIN(b.Fecha) AS PrimeraFecha
    FROM db_a25cfd_opt2.dbo.OPT_BitacoraOT b
    WHERE b.idOT = o.idOT
) ini;

/* El mapa legacy idOT -> OrdenDeTrabajoId es directo: NumeroOT = idOT. */

/* =============================================================================
   3. OPT_DetalleOT
   ============================================================================= */
INSERT INTO OPT_DetalleOT
    (OrdenDeTrabajoId, ProductoId, Cantidad, ValorUnitario, CreadoEn, CreadoPor, Eliminado)
SELECT
    ot.OrdenDeTrabajoId,
    mp.ProductoId,
    d.Cantidad,
    d.ValorUnitario,
    ot.CreadoEn,
    ot.CreadoPor,
    0
FROM db_a25cfd_opt2.dbo.OPT_OrdenDeTrabajoDetalle d
JOIN OPT_OrdenDeTrabajo ot ON ot.NumeroOT = CAST(d.idOT AS int)
JOIN #MapProducto mp ON mp.idProducto = d.idProducto;

/* =============================================================================
   4. OPT_BitacoraOT -- EstadoAnteriorId derivado de la entrada previa
   ============================================================================= */
WITH b AS (
    SELECT
        idBitacoraOT,
        idOT,
        idEstado,
        Fecha,
        Responsable,
        Observacion,
        LAG(idEstado) OVER (PARTITION BY idOT ORDER BY Fecha, idBitacoraOT) AS EstadoPrevio
    FROM db_a25cfd_opt2.dbo.OPT_BitacoraOT
)
INSERT INTO OPT_BitacoraOT
    (OrdenDeTrabajoId, EstadoAnteriorId, EstadoNuevoId, Observacion, CreadoEn, CreadoPor, Eliminado)
SELECT
    ot.OrdenDeTrabajoId,
    ISNULL(b.EstadoPrevio, b.idEstado),
    b.idEstado,
    NULLIF(LTRIM(RTRIM(b.Observacion)), ''),
    TODATETIMEOFFSET(b.Fecha, 0),
    ISNULL(mu.UsuarioId, @UsuarioBootstrap),
    0
FROM b
JOIN OPT_OrdenDeTrabajo ot ON ot.NumeroOT = CAST(b.idOT AS int)
LEFT JOIN #MapUsuario mu ON mu.Nombre = LTRIM(RTRIM(b.Responsable)) COLLATE DATABASE_DEFAULT;

/* =============================================================================
   5. OPT_Abono
   ============================================================================= */
INSERT INTO OPT_Abono
    (OrdenDeTrabajoId, Monto, FormaPagoId, Referencia, CreadoEn, CreadoPor, Eliminado)
SELECT
    ot.OrdenDeTrabajoId,
    a.Monto,
    a.idFormaPago,
    NULL,
    TODATETIMEOFFSET(a.FechaAbono, 0),
    ot.CreadoPor,
    0
FROM db_a25cfd_opt2.dbo.OPT_Abono a
JOIN OPT_OrdenDeTrabajo ot ON ot.NumeroOT = CAST(a.idOT AS int);

/* =============================================================================
   6. OPT_Pago
   ============================================================================= */
INSERT INTO OPT_Pago
    (OrdenDeTrabajoId, FechaPago, Monto, FormaPagoId, Referencia, CreadoEn, CreadoPor, Eliminado)
SELECT
    ot.OrdenDeTrabajoId,
    TODATETIMEOFFSET(p.FechaPago, 0),
    p.Monto,
    ISNULL(f.Id, 0),
    NULL,
    TODATETIMEOFFSET(p.FechaPago, 0),
    ot.CreadoPor,
    0
FROM db_a25cfd_opt2.dbo.OPT_Pago p
JOIN OPT_OrdenDeTrabajo ot ON ot.NumeroOT = CAST(p.idOT AS int)
LEFT JOIN OPT_FormaPago f ON f.Nombre = LTRIM(RTRIM(p.TipoPago)) COLLATE DATABASE_DEFAULT;

/* =============================================================================
   7. OPT_Cuota
   ============================================================================= */
INSERT INTO OPT_Cuota
    (OrdenDeTrabajoId, Numero, ValorCuota, FechaVencimiento, FechaPago,
     FormaPagoId, EstadoCuotaId, CreadoEn, CreadoPor, Eliminado)
SELECT
    ot.OrdenDeTrabajoId,
    q.Numero,
    q.ValorCuota,
    CAST(q.FechaBencimiento AS date),
    CASE WHEN q.FechaPago IS NULL THEN NULL ELSE TODATETIMEOFFSET(q.FechaPago, 0) END,
    f.Id,
    ISNULL(ec.Id, 1),
    ot.CreadoEn,
    ot.CreadoPor,
    0
FROM db_a25cfd_opt2.dbo.OPT_Cuota q
JOIN OPT_OrdenDeTrabajo ot ON ot.NumeroOT = CAST(q.idOT AS int)
LEFT JOIN OPT_FormaPago   f  ON f.Nombre  = LTRIM(RTRIM(q.TipoPago)) COLLATE DATABASE_DEFAULT
LEFT JOIN OPT_EstadoCuota ec ON ec.Nombre = LTRIM(RTRIM(q.Estado))   COLLATE DATABASE_DEFAULT;

/* =============================================================================
   8. Recalculo de TotalAbonado / Saldo  (= abonos + pagos)
   ============================================================================= */
UPDATE ot
SET TotalAbonado = t.Total,
    Saldo        = ot.Precio - t.Total
FROM OPT_OrdenDeTrabajo ot
CROSS APPLY (
    SELECT ISNULL((SELECT SUM(a.Monto) FROM OPT_Abono a WHERE a.OrdenDeTrabajoId = ot.OrdenDeTrabajoId), 0)
         + ISNULL((SELECT SUM(p.Monto) FROM OPT_Pago  p WHERE p.OrdenDeTrabajoId = ot.OrdenDeTrabajoId), 0)
      AS Total
) t;

COMMIT TRANSACTION;

DROP TABLE #MapEmpresa, #MapSucursal, #MapUsuario, #MapProducto;
GO

/* =============================================================================
   VERIFICACION
   ============================================================================= */
SELECT 'OPT_Producto'       AS Tabla, COUNT(*) AS Destino, 4018  AS Esperado FROM OPT_Producto
UNION ALL SELECT 'OPT_OrdenDeTrabajo', COUNT(*), 12578 FROM OPT_OrdenDeTrabajo
UNION ALL SELECT 'OPT_DetalleOT',      COUNT(*), 20573 FROM OPT_DetalleOT
UNION ALL SELECT 'OPT_BitacoraOT',     COUNT(*), 31964 FROM OPT_BitacoraOT
UNION ALL SELECT 'OPT_Abono',          COUNT(*),  3277 FROM OPT_Abono
UNION ALL SELECT 'OPT_Pago',           COUNT(*),  3736 FROM OPT_Pago
UNION ALL SELECT 'OPT_Cuota',          COUNT(*), 34110 FROM OPT_Cuota;
GO
