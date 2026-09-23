/* =============================================================================
   011_operativoot_snapshot_montos.sql
   -----------------------------------------------------------------------------
   Corrige un desfase entre 009_modulo_operativo.sql y dbOPT_NET: OPT_OperativoOT
   se creo (primera etapa del modulo Operativo) ANTES de que MontoVendidoSnapshot/
   MontoPagadoSnapshot se agregaran al script (segunda etapa, mismo dia), y como
   009 guarda el CREATE TABLE con "IF OBJECT_ID(...) IS NULL", re-ejecutarlo no
   agrega las columnas a una tabla que ya existe -- quedaron ausentes en la base
   real. Sintoma: "Invalid column name 'MontoPagadoSnapshot'"/'MontoVendidoSnapshot'
   al abrir la ficha de un Operativo (ObtenerCompletaPorPublicIdAsync).

   Verificado con sqlcmd (sys.columns): OPT_OperativoOT en dbOPT_NET solo tenia
   OperativoOTId/OperativoId/OrdenDeTrabajoId antes de este script.

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[OPT_OperativoOT]') AND name = N'MontoVendidoSnapshot'
)
BEGIN
    ALTER TABLE [OPT_OperativoOT]
        ADD [MontoVendidoSnapshot] decimal(18,2) NOT NULL
            CONSTRAINT [DF_OPT_OperativoOT_MontoVendidoSnapshot] DEFAULT (0);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[OPT_OperativoOT]') AND name = N'MontoPagadoSnapshot'
)
BEGIN
    ALTER TABLE [OPT_OperativoOT]
        ADD [MontoPagadoSnapshot] decimal(18,2) NOT NULL
            CONSTRAINT [DF_OPT_OperativoOT_MontoPagadoSnapshot] DEFAULT (0);
END;
GO

COMMIT;
GO

PRINT '011_operativoot_snapshot_montos.sql aplicado.';
GO
