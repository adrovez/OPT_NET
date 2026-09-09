/* =============================================================================
   006_receta_ot_detalle_comentario.sql
   -----------------------------------------------------------------------------
   Habilita la vista "Ver Orden" del modulo Comercial con las mismas cuatro
   pestanas del legacy (Cliente / Receta / Detalle / Abonos):

     1. OPT_RecetaCristales.OrdenDeTrabajoId : reproduce el
        OPT_RecetaCristales.idOT del legacy. La receta deja de colgar solo del
        Cliente y puede materializarse en una OT concreta, que es la
        prescripcion con la que se fabricaron ESOS cristales -- no "la ultima
        receta del cliente". Nullable a proposito: la ficha clinica puede tomar
        una receta que todavia no se emite en ninguna orden (en el legacy son
        las filas con idOT = NULL).
        En los datos legacy 12.574 de 13.183 recetas tienen idOT, y 2 ordenes
        tienen dos recetas -- por eso la relacion es 1:N, no 1:1.

     2. OPT_DetalleOT.Comentario : anotacion libre por linea (modelo y color del
        armazon, p. ej. "FORMOSA F4 C2"). Equivale a
        OPT_OrdenDeTrabajoDetalle.Comentario del legacy, poblado en 11.168 de
        las 20.573 lineas migradas. Sin esta columna el dato se perdia.

   La carga de datos legacy de ambas columnas va aparte, en
   migracion/M006_backfill_receta_ot_detalle_comentario.sql (esquema y datos no
   se mezclan -- ver src/basedatos/README.md).

   IMPORTANTE: escrito a mano, igual que 004 y 005. Con este script aplicado y
   las propiedades RecetaCristales.OrdenDeTrabajoId y DetalleOT.Comentario ya
   creadas en OPT.Domain (con sus IEntityTypeConfiguration), el modelo de EF Core
   y dbOPT_NET quedan alineados.

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

/* -----------------------------------------------------------------------------
   1. OPT_RecetaCristales.OrdenDeTrabajoId
   -------------------------------------------------------------------------- */
IF COL_LENGTH(N'OPT_RecetaCristales', N'OrdenDeTrabajoId') IS NULL
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [OrdenDeTrabajoId] int NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
                WHERE [name] = N'FK_RecetasCristales_OrdenesDeTrabajo')
BEGIN
    /* NO ACTION (Restrict): una OT no se borra fisicamente -- se anula. Si algun
       dia se borrara, no debe arrastrarse la receta, que es dato clinico. */
    ALTER TABLE [OPT_RecetaCristales] WITH CHECK
        ADD CONSTRAINT [FK_RecetasCristales_OrdenesDeTrabajo]
        FOREIGN KEY ([OrdenDeTrabajoId])
        REFERENCES [OPT_OrdenDeTrabajo] ([OrdenDeTrabajoId])
        ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_RecetasCristales_OrdenDeTrabajoId'
                  AND [object_id] = OBJECT_ID(N'OPT_RecetaCristales'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RecetasCristales_OrdenDeTrabajoId]
        ON [OPT_RecetaCristales] ([OrdenDeTrabajoId]);
END;
GO

/* -----------------------------------------------------------------------------
   2. OPT_DetalleOT.Comentario
   -------------------------------------------------------------------------- */
IF COL_LENGTH(N'OPT_DetalleOT', N'Comentario') IS NULL
BEGIN
    /* nvarchar(200): el maximo real en el legacy es de 100 caracteres; se deja
       holgura sin llegar a texto largo, que no es el uso de este campo. */
    ALTER TABLE [OPT_DetalleOT] ADD [Comentario] nvarchar(200) NULL;
END;
GO

COMMIT TRANSACTION;
GO

/* -----------------------------------------------------------------------------
   Verificacion
   -------------------------------------------------------------------------- */
SELECT 'OPT_RecetaCristales.OrdenDeTrabajoId' AS Objeto,
       CASE WHEN COL_LENGTH(N'OPT_RecetaCristales', N'OrdenDeTrabajoId') IS NULL
            THEN 'FALTA' ELSE 'OK' END AS Estado
UNION ALL
SELECT 'FK_RecetasCristales_OrdenesDeTrabajo',
       CASE WHEN EXISTS (SELECT 1 FROM sys.foreign_keys
                          WHERE [name] = N'FK_RecetasCristales_OrdenesDeTrabajo')
            THEN 'OK' ELSE 'FALTA' END
UNION ALL
SELECT 'IX_RecetasCristales_OrdenDeTrabajoId',
       CASE WHEN EXISTS (SELECT 1 FROM sys.indexes
                          WHERE [name] = N'IX_RecetasCristales_OrdenDeTrabajoId'
                            AND [object_id] = OBJECT_ID(N'OPT_RecetaCristales'))
            THEN 'OK' ELSE 'FALTA' END
UNION ALL
SELECT 'OPT_DetalleOT.Comentario',
       CASE WHEN COL_LENGTH(N'OPT_DetalleOT', N'Comentario') IS NULL
            THEN 'FALTA' ELSE 'OK' END;
GO
