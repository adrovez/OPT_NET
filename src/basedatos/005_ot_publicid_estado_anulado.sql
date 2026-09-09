/* =============================================================================
   005_ot_publicid_estado_anulado.sql
   -----------------------------------------------------------------------------
   Prepara el modulo Comercial para publicar su API (Fase 2):

     1. OPT_OrdenDeTrabajo.PublicId : identificador no enumerable (Guid) que se
        expone en rutas y DTOs, nunca el Id interno. Cierra la deuda tecnica de
        prioridad alta registrada en el ADR 0004, seccion "Revision de cobertura
        de PublicId". Se agrega ANTES de publicar el primer endpoint de OT --
        hacerlo despues romperia URLs ya emitidas.
     2. OPT_EstadoOT (7, 'ANULADO') : el legacy anulaba OT con SP_OTEliminar, sin
        estado en el catalogo. El sistema nuevo modela la anulacion como un
        estado terminal mas, visible en listados y trazado en OPT_BitacoraOT.

   Alcance de PublicId decidido con el usuario (2026-08-27): SOLO OrdenDeTrabajo.
   Abono, Pago, Cuota, DetalleOT y BitacoraOT se exponen anidados bajo la ruta de
   la OT (ya protegida), usando su Id interno.

   IMPORTANTE: escrito a mano, igual que 004. Con este script aplicado y las
   entidades Pago/Cuota/EstadoCuota ya creadas en OPT.Domain, el modelo de EF Core
   y dbOPT_NET vuelven a estar alineados.

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

/* -----------------------------------------------------------------------------
   1. OPT_OrdenDeTrabajo.PublicId
   -------------------------------------------------------------------------- */
IF COL_LENGTH(N'OPT_OrdenDeTrabajo', N'PublicId') IS NULL
BEGIN
    /* Se agrega NULL primero para poblar las 12.578 filas ya migradas y recien
       despues se marca NOT NULL: un ALTER ... ADD NOT NULL DEFAULT NEWID() sobre
       una tabla con datos funciona, pero deja el default como constraint con
       nombre autogenerado. */
    ALTER TABLE [OPT_OrdenDeTrabajo] ADD [PublicId] uniqueidentifier NULL;
END;
GO

UPDATE [OPT_OrdenDeTrabajo]
   SET [PublicId] = NEWID()
 WHERE [PublicId] IS NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns
            WHERE [object_id] = OBJECT_ID(N'OPT_OrdenDeTrabajo')
              AND [name] = N'PublicId' AND [is_nullable] = 1)
BEGIN
    ALTER TABLE [OPT_OrdenDeTrabajo] ALTER COLUMN [PublicId] uniqueidentifier NOT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
                WHERE [name] = N'DF_OPT_OrdenDeTrabajo_PublicId')
BEGIN
    ALTER TABLE [OPT_OrdenDeTrabajo]
        ADD CONSTRAINT [DF_OPT_OrdenDeTrabajo_PublicId] DEFAULT (NEWID()) FOR [PublicId];
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE [name] = N'UQ_OrdenesDeTrabajo_PublicId'
                  AND [object_id] = OBJECT_ID(N'OPT_OrdenDeTrabajo'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_OrdenesDeTrabajo_PublicId]
        ON [OPT_OrdenDeTrabajo] ([PublicId]);
END;
GO

/* -----------------------------------------------------------------------------
   2. OPT_EstadoOT -- estado terminal ANULADO
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [OPT_EstadoOT] WHERE [Id] = 7)
BEGIN
    INSERT INTO [OPT_EstadoOT] ([Id], [Nombre]) VALUES (7, N'ANULADO');
END;
GO

COMMIT;
GO

PRINT '005_ot_publicid_estado_anulado.sql aplicado.';
GO
