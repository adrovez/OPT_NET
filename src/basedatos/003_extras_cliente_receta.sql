BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826151718_AgregaExtrasClienteReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [AddLejos] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826151718_AgregaExtrasClienteReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [DpCerca] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826151718_AgregaExtrasClienteReceta'
)
BEGIN
    ALTER TABLE [OPT_RecetaCristales] ADD [DpLejos] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826151718_AgregaExtrasClienteReceta'
)
BEGIN
    ALTER TABLE [OPT_Cliente] ADD [FechaNacimiento] date NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826151718_AgregaExtrasClienteReceta'
)
BEGIN
    ALTER TABLE [OPT_Cliente] ADD [TipoPrevision] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826151718_AgregaExtrasClienteReceta'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260826151718_AgregaExtrasClienteReceta', N'8.0.30');
END;
GO

COMMIT;
GO

