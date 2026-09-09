BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    CREATE TABLE [OPT_CategoriaProducto] (
        [Id] int NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_OPT_CategoriaProducto] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    CREATE TABLE [OPT_EstadoOT] (
        [Id] int NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_OPT_EstadoOT] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    CREATE TABLE [OPT_FormaPago] (
        [Id] int NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_OPT_FormaPago] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_CategoriaProducto]'))
        SET IDENTITY_INSERT [OPT_CategoriaProducto] ON;
    EXEC(N'INSERT INTO [OPT_CategoriaProducto] ([Id], [Nombre])
    VALUES (1, N''General'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_CategoriaProducto]'))
        SET IDENTITY_INSERT [OPT_CategoriaProducto] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_EstadoOT]'))
        SET IDENTITY_INSERT [OPT_EstadoOT] ON;
    EXEC(N'INSERT INTO [OPT_EstadoOT] ([Id], [Nombre])
    VALUES (0, N''INGRESADO''),
    (1, N''EN PROCESO''),
    (2, N''MONTAJE''),
    (3, N''LABORATORIO''),
    (4, N''CALIDAD''),
    (5, N''DESPACHO''),
    (6, N''ENTREGADO'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_EstadoOT]'))
        SET IDENTITY_INSERT [OPT_EstadoOT] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_FormaPago]'))
        SET IDENTITY_INSERT [OPT_FormaPago] ON;
    EXEC(N'INSERT INTO [OPT_FormaPago] ([Id], [Nombre])
    VALUES (0, N''SIN INFORMACION''),
    (1, N''EFECTIVO''),
    (2, N''TARJETA CREDITO''),
    (3, N''TARJETA DEBITO''),
    (4, N''TRANSFERENCIA'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_FormaPago]'))
        SET IDENTITY_INSERT [OPT_FormaPago] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_Rol]'))
        SET IDENTITY_INSERT [OPT_Rol] ON;
    EXEC(N'INSERT INTO [OPT_Rol] ([Id], [Descripcion], [Nombre])
    VALUES (4, NULL, N''Jefe Sucursal''),
    (5, NULL, N''Vendedor''),
    (6, NULL, N''Tecnico Medico''),
    (7, NULL, N''Control Calidad''),
    (8, NULL, N''Externo'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_Rol]'))
        SET IDENTITY_INSERT [OPT_Rol] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    CREATE INDEX [IX_OPT_Abono_FormaPagoId] ON [OPT_Abono] ([FormaPagoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_CategoriasProducto_Nombre] ON [OPT_CategoriaProducto] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_EstadosOT_Nombre] ON [OPT_EstadoOT] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_FormasPago_Nombre] ON [OPT_FormaPago] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    ALTER TABLE [OPT_Abono] ADD CONSTRAINT [FK_Abonos_FormasPago] FOREIGN KEY ([FormaPagoId]) REFERENCES [OPT_FormaPago] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    ALTER TABLE [OPT_OrdenDeTrabajo] ADD CONSTRAINT [FK_OrdenesDeTrabajo_EstadosOT] FOREIGN KEY ([EstadoOTId]) REFERENCES [OPT_EstadoOT] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    ALTER TABLE [OPT_Producto] ADD CONSTRAINT [FK_Productos_CategoriasProducto] FOREIGN KEY ([CategoriaId]) REFERENCES [OPT_CategoriaProducto] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821205721_AgregaCatalogosEstadoOTFormaPagoCategoriaYFks', N'8.0.30');
END;
GO

COMMIT;
GO

