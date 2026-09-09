-- Habilita en OPT_RecetaCristales los checkboxes "Incluir Cristales Lejos/Cerca" del legacy
-- (OPT_RecetaCristales.CheckLejos/CheckCerca) y sus 6 observaciones de detalle por ojo/DP
-- (LejosODObservacion, LejosOIObservacion, LejosDPObservacion, CercaODObservacion,
-- CercaOIObservacion, CercaDPObservacion). Coexisten con la columna `Observaciones` ya
-- existente (notas generales / texto combinado de los datos migrados del legacy — ver
-- OPT.Migracion.RecetaCristalesParser.CombinarObservaciones): no se reemplaza, se agrega.
--
-- Filas existentes (migradas o creadas antes de este script) quedan con IncluirLejos/IncluirCerca
-- en 0 y las 6 observaciones en NULL — son datos que no existían para ese concepto antes de esta
-- funcionalidad, no se infieren desde los campos numéricos ya cargados.
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [IncluirLejos] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [IncluirCerca] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [ObservacionOdLejos] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [ObservacionOiLejos] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [ObservacionDpLejos] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [ObservacionOdCerca] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [ObservacionOiCerca] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [ObservacionDpCerca] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908000000_AgregaIncluirYObservacionesDetalleReceta'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908000000_AgregaIncluirYObservacionesDetalleReceta', N'8.0.30');
END;
GO

COMMIT;
GO
