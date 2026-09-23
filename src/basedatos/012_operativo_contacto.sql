/* =============================================================================
   012_operativo_contacto.sql
   -----------------------------------------------------------------------------
   Sesion 2026-09-22 -- HU-OP-01/02 (02_HU_Modulo_Operativo.html): datos de
   contacto de la persona de la Empresa a cargo de la jornada. Se agregan a
   OPT_Operativo (no a OPT_Empresa) por decision registrada en el propio
   documento de HU: son propios de cada Operativo, pueden cambiar de una
   jornada a otra aunque sea la misma Empresa (pregunta abierta N.º 3 de
   00_Analisis_Impacto.html, resuelta con el supuesto de trabajo ahi descrito).

   Los tres campos son NULL -- opcionales al guardar como Prospecto, ver
   Operativo.Crear/Actualizar. Escrito a mano siguiendo el patron idempotente
   de 003/006/007/010 -- entidad (OPT.Domain) y su IEntityTypeConfiguration
   ya actualizadas en la misma sesion (ver Operativo.cs / OperativoConfiguration.cs).

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[OPT_Operativo]') AND name = N'NombreContacto'
)
BEGIN
    ALTER TABLE [OPT_Operativo] ADD [NombreContacto] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[OPT_Operativo]') AND name = N'MailContacto'
)
BEGIN
    ALTER TABLE [OPT_Operativo] ADD [MailContacto] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[OPT_Operativo]') AND name = N'TelefonoContacto'
)
BEGIN
    ALTER TABLE [OPT_Operativo] ADD [TelefonoContacto] nvarchar(30) NULL;
END;
GO

COMMIT;
GO

PRINT '012_operativo_contacto.sql aplicado.';
GO
