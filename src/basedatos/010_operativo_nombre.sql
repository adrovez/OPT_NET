/* =============================================================================
   010_operativo_nombre.sql
   -----------------------------------------------------------------------------
   Sesion 2026-09-16 -- a pedido del negocio: OPT_Operativo.Nombre, un titulo
   corto libre para identificar la jornada (ademas del Correlativo autogenerado,
   que no es memorizable). Escrito a mano siguiendo el patron idempotente de
   003/006/007 -- entidad (OPT.Domain) y su IEntityTypeConfiguration ya actualizadas
   en la misma sesion (ver Operativo.cs / OperativoConfiguration.cs).

   OPT_Operativo estaba vacia al momento de este script (verificado con
   sqlcmd), asi que no hizo falta backfill real -- el DEFAULT ('') solo cubre
   el caso de una fila que ya exista en otro ambiente al aplicar este script.

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[OPT_Operativo]') AND name = N'Nombre'
)
BEGIN
    ALTER TABLE [OPT_Operativo]
        ADD [Nombre] nvarchar(200) NOT NULL
            CONSTRAINT [DF_OPT_Operativo_Nombre] DEFAULT (N'');
END;
GO

COMMIT;
GO

PRINT '010_operativo_nombre.sql aplicado.';
GO
