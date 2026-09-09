/* =============================================================================
   M006_backfill_receta_ot_detalle_comentario.sql
   -----------------------------------------------------------------------------
   Carga de datos legacy para las dos columnas que agrega 006:

     1. OPT_RecetaCristales.OrdenDeTrabajoId  <- legacy OPT_RecetaCristales.idOT
        (12.574 de 13.183 recetas tienen idOT)
     2. OPT_DetalleOT.Comentario              <- legacy
        OPT_OrdenDeTrabajoDetalle.Comentario  (11.168 de 20.573 lineas)

   PREREQUISITOS
     - 006_receta_ot_detalle_comentario.sql aplicado.
     - Clinico ya migrado con OPT.Migracion (13.183 recetas).
     - Comercial ya migrado con M004 (12.578 OT, 20.573 lineas de detalle).

   COMO SE MAPEA CADA RECETA
     OPT.Migracion inserto las recetas en una sola pasada, ORDER BY
     idRecetaCristales y sin saltarse ninguna (legacy 13.183 = destino 13.183),
     asi que la correspondencia es ORDINAL: la n-esima receta del legacy es la
     n-esima del destino. No es una suposicion: el script la VERIFICA fila a fila
     contra ClienteId y CreadoEn/FechaIngreso antes de escribir, y aborta si
     alguna no calza (verificado 0 desalineadas el 2026-08-28).

   COMO SE MAPEA CADA COMENTARIO DE DETALLE
     M004 inserto el detalle sin conservar el idOTDetalle legacy, asi que se
     re-deriva la correspondencia por (OT, Producto, Cantidad, ValorUnitario) y
     se desempata por posicion dentro de ese grupo con ROW_NUMBER. Si una OT
     tuviera dos lineas identicas con comentarios distintos, el par podria
     intercambiarse entre esas dos lineas -- no se pierde ni se inventa dato.

   NO ES IDEMPOTENTE: aborta si el destino ya tiene el dato cargado. @Force = 1
   lo re-escribe (pisa lo que haya, incluido lo que se haya editado a mano).

   Ejecutar:
     sqlcmd -S localhost -d dbOPT_NET -E -b -i migracion\M006_backfill_receta_ot_detalle_comentario.sql
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @Force bit = 0;   /* 1 = re-escribir aunque el destino ya tenga datos */

/* --- Guardas ------------------------------------------------------------- */
IF DB_ID('db_a25cfd_opt2') IS NULL
BEGIN
    RAISERROR('No se encuentra la base legacy db_a25cfd_opt2 en esta instancia.', 16, 1);
    RETURN;
END;

IF COL_LENGTH(N'OPT_RecetaCristales', N'OrdenDeTrabajoId') IS NULL
   OR COL_LENGTH(N'OPT_DetalleOT', N'Comentario') IS NULL
BEGIN
    RAISERROR('Falta aplicar 006_receta_ot_detalle_comentario.sql antes de este backfill.', 16, 1);
    RETURN;
END;

IF @Force = 0 AND EXISTS (SELECT 1 FROM OPT_RecetaCristales WHERE OrdenDeTrabajoId IS NOT NULL)
BEGIN
    RAISERROR('OPT_RecetaCristales ya tiene recetas vinculadas a una OT. Usar @Force = 1 solo si es intencional.', 16, 1);
    RETURN;
END;

IF @Force = 0 AND EXISTS (SELECT 1 FROM OPT_DetalleOT WHERE Comentario IS NOT NULL)
BEGIN
    RAISERROR('OPT_DetalleOT ya tiene comentarios cargados. Usar @Force = 1 solo si es intencional.', 16, 1);
    RETURN;
END;

IF (SELECT COUNT(*) FROM OPT_RecetaCristales)
   <> (SELECT COUNT(*) FROM db_a25cfd_opt2.dbo.OPT_RecetaCristales)
BEGIN
    RAISERROR('El conteo de recetas legacy y destino no coincide: el mapeo ordinal no es valido. Abortado.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

/* =============================================================================
   Mapa ordinal legacy idRecetaCristales -> RecetaCristalesId
   ============================================================================= */
CREATE TABLE #MapReceta (
    idRecetaCristales bigint PRIMARY KEY,
    RecetaCristalesId int    NOT NULL,
    idOT              bigint NULL
);

WITH l AS (
    SELECT idRecetaCristales, idOT,
           ROW_NUMBER() OVER (ORDER BY idRecetaCristales) AS rn
    FROM db_a25cfd_opt2.dbo.OPT_RecetaCristales
), d AS (
    SELECT RecetaCristalesId,
           ROW_NUMBER() OVER (ORDER BY RecetaCristalesId) AS rn
    FROM OPT_RecetaCristales
)
INSERT INTO #MapReceta (idRecetaCristales, RecetaCristalesId, idOT)
SELECT l.idRecetaCristales, d.RecetaCristalesId, l.idOT
FROM l JOIN d ON d.rn = l.rn;

/* --- Verificacion dura 1: misma fecha de ingreso -------------------------- */
IF EXISTS (
    SELECT 1
    FROM #MapReceta m
    JOIN db_a25cfd_opt2.dbo.OPT_RecetaCristales lr ON lr.idRecetaCristales = m.idRecetaCristales
    JOIN OPT_RecetaCristales dr ON dr.RecetaCristalesId = m.RecetaCristalesId
    WHERE lr.FechaIngreso IS NOT NULL
      AND CAST(dr.CreadoEn AS datetime2(3)) <> CAST(lr.FechaIngreso AS datetime2(3))
)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('El mapeo ordinal de RecetaCristales no coincide por FechaIngreso. Revisar antes de migrar.', 16, 1);
    RETURN;
END;

/* --- Verificacion dura 2: mismo cliente ----------------------------------- */
IF EXISTS (
    SELECT 1
    FROM #MapReceta m
    JOIN db_a25cfd_opt2.dbo.OPT_RecetaCristales lr ON lr.idRecetaCristales = m.idRecetaCristales
    JOIN OPT_RecetaCristales dr ON dr.RecetaCristalesId = m.RecetaCristalesId
    JOIN OPT_Cliente c ON c.ClienteId = dr.ClienteId
    WHERE c.Rut <> LTRIM(RTRIM(ISNULL(lr.RutCliente, ''))) COLLATE DATABASE_DEFAULT
)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('El mapeo ordinal de RecetaCristales no coincide por RutCliente. Revisar antes de migrar.', 16, 1);
    RETURN;
END;

/* =============================================================================
   1. OPT_RecetaCristales.OrdenDeTrabajoId
   -----------------------------------------------------------------------------
   NumeroOT preserva el idOT legacy (M004), asi que el join es directo. Las
   recetas con idOT = NULL, o cuya OT no llego a migrarse, quedan sin vinculo.
   ============================================================================= */
UPDATE dr
   SET dr.OrdenDeTrabajoId = ot.OrdenDeTrabajoId
FROM OPT_RecetaCristales dr
JOIN #MapReceta m ON m.RecetaCristalesId = dr.RecetaCristalesId
JOIN OPT_OrdenDeTrabajo ot ON ot.NumeroOT = CAST(m.idOT AS int)
WHERE m.idOT IS NOT NULL;

DECLARE @RecetasVinculadas int = @@ROWCOUNT;

/* =============================================================================
   2. OPT_DetalleOT.Comentario
   ============================================================================= */
CREATE TABLE #MapProducto (idProducto bigint PRIMARY KEY, ProductoId int NOT NULL);

INSERT INTO #MapProducto (idProducto, ProductoId)
SELECT p.idProducto, np.ProductoId
FROM db_a25cfd_opt2.dbo.OPT_Producto p
JOIN OPT_Producto np ON np.Codigo = p.Codigo COLLATE DATABASE_DEFAULT;

WITH l AS (
    SELECT
        ot.OrdenDeTrabajoId,
        mp.ProductoId,
        d.Cantidad,
        d.ValorUnitario,
        NULLIF(LTRIM(RTRIM(d.Comentario)), '') AS Comentario,
        ROW_NUMBER() OVER (
            PARTITION BY ot.OrdenDeTrabajoId, mp.ProductoId, d.Cantidad, d.ValorUnitario
            ORDER BY d.idOTDetalle) AS rn
    FROM db_a25cfd_opt2.dbo.OPT_OrdenDeTrabajoDetalle d
    JOIN OPT_OrdenDeTrabajo ot ON ot.NumeroOT = CAST(d.idOT AS int)
    JOIN #MapProducto mp ON mp.idProducto = d.idProducto
), dst AS (
    SELECT
        DetalleOTId,
        OrdenDeTrabajoId,
        ProductoId,
        Cantidad,
        ValorUnitario,
        ROW_NUMBER() OVER (
            PARTITION BY OrdenDeTrabajoId, ProductoId, Cantidad, ValorUnitario
            ORDER BY DetalleOTId) AS rn
    FROM OPT_DetalleOT
)
UPDATE d
   SET d.Comentario = l.Comentario
FROM OPT_DetalleOT d
JOIN dst ON dst.DetalleOTId = d.DetalleOTId
JOIN l   ON l.OrdenDeTrabajoId = dst.OrdenDeTrabajoId
        AND l.ProductoId       = dst.ProductoId
        AND l.Cantidad         = dst.Cantidad
        AND l.ValorUnitario    = dst.ValorUnitario
        AND l.rn               = dst.rn
WHERE l.Comentario IS NOT NULL;

DECLARE @ComentariosCargados int = @@ROWCOUNT;

COMMIT TRANSACTION;

/* =============================================================================
   Verificacion final
   ============================================================================= */
SELECT 'Recetas vinculadas a una OT' AS Concepto,
       @RecetasVinculadas           AS Cargadas,
       (SELECT COUNT(*) FROM db_a25cfd_opt2.dbo.OPT_RecetaCristales lr
         WHERE lr.idOT IS NOT NULL
           AND EXISTS (SELECT 1 FROM OPT_OrdenDeTrabajo o WHERE o.NumeroOT = CAST(lr.idOT AS int)))
                                    AS Esperadas
UNION ALL
SELECT 'Lineas de detalle con comentario',
       @ComentariosCargados,
       (SELECT COUNT(*) FROM db_a25cfd_opt2.dbo.OPT_OrdenDeTrabajoDetalle d
         WHERE LTRIM(RTRIM(ISNULL(d.Comentario, ''))) <> ''
           AND EXISTS (SELECT 1 FROM OPT_OrdenDeTrabajo o WHERE o.NumeroOT = CAST(d.idOT AS int)));

DROP TABLE #MapReceta;
DROP TABLE #MapProducto;
GO
