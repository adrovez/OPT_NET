/* =============================================================================
   008_numero_ot_manual.sql
   -----------------------------------------------------------------------------
   OPT_OrdenDeTrabajo.NumeroOT deja de generarse con la SEQUENCE SEQ_NumeroUT y
   pasa a ingresarse manualmente, igual que en el legacy (a pedido del usuario,
   2026-09-11). Reglas de negocio nuevas (no existian en el legacy, que solo
   dependia de una IDENTITY):

     1. No se puede ingresar un NumeroOT que ya exista en el mismo anio (por
        OPT_OrdenDeTrabajo.CreadoEn) en una OT que NO este anulada.
     2. Si esa OT con el mismo numero esta anulada, se permite reutilizar el
        numero.

   La validacion "mismo anio" la hace OPT.Application (necesita YEAR(CreadoEn),
   que un indice filtrado simple no puede expresar sin una columna calculada
   nueva). La BD aporta un respaldo mas estricto: un indice unico filtrado que
   nunca deja compartir NumeroOT a dos OT vigentes (no anuladas), sin importar
   el anio -- decision tomada con el usuario, ver CLAUDE.md.

   Cambios:
     1. Quita el DEFAULT (NEXT VALUE FOR SEQ_NumeroUT) de la columna --
        el nombre del constraint es autogenerado por SQL Server (nunca se le
        puso nombre explicito en 001_esquema_inicial.sql), asi que se busca
        dinamicamente por tabla+columna en vez de asumir un nombre fijo.
     2. Reemplaza el indice unico UQ_OrdenesDeTrabajo_NumeroOT (global, sobre
        TODAS las OT) por UQ_OrdenesDeTrabajo_NumeroOT_Vigente, filtrado a
        EstadoOTId <> 7 (ANULADO).
     3. SEQ_NumeroUT NO se elimina: queda creada sin uso, por si hiciera falta
        de respaldo (decision del usuario). No se llama mas desde el modelo de
        EF Core salvo por la declaracion en AppDbContext, que ya no la referencia
        desde ninguna columna.

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

/* -----------------------------------------------------------------------------
   1. Quitar el DEFAULT de OPT_OrdenDeTrabajo.NumeroOT
   -------------------------------------------------------------------------- */
DECLARE @nombreDefault sysname;

SELECT @nombreDefault = dc.[name]
  FROM sys.default_constraints dc
  JOIN sys.columns c
    ON c.[object_id] = dc.[parent_object_id]
   AND c.[column_id] = dc.[parent_column_id]
 WHERE dc.[parent_object_id] = OBJECT_ID(N'OPT_OrdenDeTrabajo')
   AND c.[name] = N'NumeroOT';

IF @nombreDefault IS NOT NULL
BEGIN
    DECLARE @sql nvarchar(max) =
        N'ALTER TABLE [OPT_OrdenDeTrabajo] DROP CONSTRAINT [' + @nombreDefault + N'];';
    EXEC sp_executesql @sql;
END;
GO

/* -----------------------------------------------------------------------------
   2. Reemplazar el indice unico global por uno filtrado (excluye ANULADO)
   -------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.indexes
            WHERE [name] = N'UQ_OrdenesDeTrabajo_NumeroOT'
              AND [object_id] = OBJECT_ID(N'OPT_OrdenDeTrabajo'))
BEGIN
    DROP INDEX [UQ_OrdenesDeTrabajo_NumeroOT] ON [OPT_OrdenDeTrabajo];
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE [name] = N'UQ_OrdenesDeTrabajo_NumeroOT_Vigente'
                  AND [object_id] = OBJECT_ID(N'OPT_OrdenDeTrabajo'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_OrdenesDeTrabajo_NumeroOT_Vigente]
        ON [OPT_OrdenDeTrabajo] ([NumeroOT])
        WHERE [EstadoOTId] <> 7;
END;
GO

COMMIT;
GO

PRINT '008_numero_ot_manual.sql aplicado.';
GO
