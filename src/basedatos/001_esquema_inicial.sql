IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE SEQUENCE [dbo].[SEQ_NumeroOT] AS int START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Empresa] (
        [EmpresaId] int NOT NULL IDENTITY,
        [PublicId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [Nombre] nvarchar(100) NOT NULL,
        [Rut] nvarchar(12) NOT NULL,
        [RazonSocial] nvarchar(150) NOT NULL,
        [Giro] nvarchar(150) NOT NULL DEFAULT N'',
        [Direccion] nvarchar(200) NOT NULL DEFAULT N'',
        [Telefono] nvarchar(20) NOT NULL DEFAULT N'',
        [Email] nvarchar(150) NOT NULL DEFAULT N'',
        [Contacto] nvarchar(100) NOT NULL DEFAULT N'',
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_Empresa] PRIMARY KEY ([EmpresaId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Producto] (
        [ProductoId] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(200) NOT NULL,
        [ControlStock] bit NOT NULL,
        [CategoriaId] int NOT NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_Producto] PRIMARY KEY ([ProductoId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Region] (
        [Id] int NOT NULL,
        [CodigoOficial] nvarchar(10) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_OPT_Region] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Rol] (
        [Id] int NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_OPT_Rol] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Sucursal] (
        [SucursalId] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Direccion] nvarchar(200) NULL DEFAULT N'',
        [Telefono] nvarchar(20) NULL DEFAULT N'',
        [EsMatriz] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_Sucursal] PRIMARY KEY ([SucursalId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Comuna] (
        [Id] int NOT NULL,
        [RegionId] int NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_OPT_Comuna] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comunas_Regiones] FOREIGN KEY ([RegionId]) REFERENCES [OPT_Region] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_EmpresaSucursal] (
        [EmpresaSucursalId] int NOT NULL IDENTITY,
        [EmpresaId] int NOT NULL,
        [SucursalId] int NOT NULL,
        CONSTRAINT [PK_OPT_EmpresaSucursal] PRIMARY KEY ([EmpresaSucursalId]),
        CONSTRAINT [FK_EmpresaSucursales_Empresas] FOREIGN KEY ([EmpresaId]) REFERENCES [OPT_Empresa] ([EmpresaId]) ON DELETE CASCADE,
        CONSTRAINT [FK_EmpresaSucursales_Sucursales] FOREIGN KEY ([SucursalId]) REFERENCES [OPT_Sucursal] ([SucursalId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_ProductoSucursal] (
        [ProductoSucursalId] int NOT NULL IDENTITY,
        [ProductoId] int NOT NULL,
        [SucursalId] int NOT NULL,
        [StockActual] int NOT NULL,
        [StockMinimo] int NOT NULL,
        [StockMaximo] int NOT NULL,
        [PrecioVenta] decimal(18,2) NOT NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_ProductoSucursal] PRIMARY KEY ([ProductoSucursalId]),
        CONSTRAINT [FK_ProductoSucursales_Productos] FOREIGN KEY ([ProductoId]) REFERENCES [OPT_Producto] ([ProductoId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductoSucursales_Sucursales] FOREIGN KEY ([SucursalId]) REFERENCES [OPT_Sucursal] ([SucursalId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Usuario] (
        [UsuarioId] int NOT NULL IDENTITY,
        [PublicId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [Rut] nvarchar(12) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Apellido] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [ClaveHash] nvarchar(100) NOT NULL,
        [RolId] int NOT NULL,
        [SucursalActivaId] int NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_Usuario] PRIMARY KEY ([UsuarioId]),
        CONSTRAINT [FK_Usuarios_Roles] FOREIGN KEY ([RolId]) REFERENCES [OPT_Rol] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Usuarios_Sucursales_Activa] FOREIGN KEY ([SucursalActivaId]) REFERENCES [OPT_Sucursal] ([SucursalId]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Cliente] (
        [ClienteId] int NOT NULL IDENTITY,
        [PublicId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [Rut] nvarchar(12) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Apellido] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NULL,
        [Telefono] nvarchar(20) NULL,
        [Direccion] nvarchar(200) NULL,
        [ComunaId] int NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_Cliente] PRIMARY KEY ([ClienteId]),
        CONSTRAINT [FK_Clientes_Comunas] FOREIGN KEY ([ComunaId]) REFERENCES [OPT_Comuna] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_UsuarioSucursal] (
        [UsuarioSucursalId] int NOT NULL IDENTITY,
        [UsuarioId] int NOT NULL,
        [SucursalId] int NOT NULL,
        CONSTRAINT [PK_OPT_UsuarioSucursal] PRIMARY KEY ([UsuarioSucursalId]),
        CONSTRAINT [FK_UsuarioSucursales_Sucursales] FOREIGN KEY ([SucursalId]) REFERENCES [OPT_Sucursal] ([SucursalId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UsuarioSucursales_Usuarios] FOREIGN KEY ([UsuarioId]) REFERENCES [OPT_Usuario] ([UsuarioId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Anamnesis] (
        [AnamnesisId] int NOT NULL IDENTITY,
        [PublicId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [ClienteId] int NOT NULL,
        [Hipertension] bit NOT NULL,
        [Diabetes] bit NOT NULL,
        [Alergias] bit NOT NULL,
        [DetalleAlergias] nvarchar(500) NULL,
        [UsaLentesPrevio] bit NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_Anamnesis] PRIMARY KEY ([AnamnesisId]),
        CONSTRAINT [FK_Anamnesis_Clientes] FOREIGN KEY ([ClienteId]) REFERENCES [OPT_Cliente] ([ClienteId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_OrdenDeTrabajo] (
        [OrdenDeTrabajoId] int NOT NULL IDENTITY,
        [NumeroOT] int NOT NULL DEFAULT (NEXT VALUE FOR [dbo].[SEQ_NumeroOT]),
        [ClienteId] int NOT NULL,
        [SucursalId] int NOT NULL,
        [EstadoOTId] int NOT NULL,
        [EmpresaId] int NULL,
        [Precio] decimal(18,2) NOT NULL,
        [TotalAbonado] decimal(18,2) NOT NULL,
        [Saldo] decimal(18,2) NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [FechaEntrega] datetimeoffset NOT NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_OrdenDeTrabajo] PRIMARY KEY ([OrdenDeTrabajoId]),
        CONSTRAINT [FK_OrdenesDeTrabajo_Clientes] FOREIGN KEY ([ClienteId]) REFERENCES [OPT_Cliente] ([ClienteId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrdenesDeTrabajo_Empresas] FOREIGN KEY ([EmpresaId]) REFERENCES [OPT_Empresa] ([EmpresaId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrdenesDeTrabajo_Sucursales] FOREIGN KEY ([SucursalId]) REFERENCES [OPT_Sucursal] ([SucursalId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_RecetaCristales] (
        [RecetaCristalesId] int NOT NULL IDENTITY,
        [PublicId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [ClienteId] int NOT NULL,
        [OdEsferaLejos] decimal(5,2) NULL,
        [OdCilindroLejos] decimal(5,2) NULL,
        [OdEjeLejos] int NULL,
        [OdEsferaCerca] decimal(5,2) NULL,
        [OdCilindroCerca] decimal(5,2) NULL,
        [OdEjeCerca] int NULL,
        [OiEsferaLejos] decimal(5,2) NULL,
        [OiCilindroLejos] decimal(5,2) NULL,
        [OiEjeLejos] int NULL,
        [OiEsferaCerca] decimal(5,2) NULL,
        [OiCilindroCerca] decimal(5,2) NULL,
        [OiEjeCerca] int NULL,
        [Urgente] bit NOT NULL,
        [RequiereLab] bit NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_RecetaCristales] PRIMARY KEY ([RecetaCristalesId]),
        CONSTRAINT [FK_RecetasCristales_Clientes] FOREIGN KEY ([ClienteId]) REFERENCES [OPT_Cliente] ([ClienteId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_Abono] (
        [AbonoId] int NOT NULL IDENTITY,
        [OrdenDeTrabajoId] int NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [FormaPagoId] int NOT NULL,
        [Referencia] nvarchar(50) NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_Abono] PRIMARY KEY ([AbonoId]),
        CONSTRAINT [FK_Abonos_OrdenesDeTrabajo] FOREIGN KEY ([OrdenDeTrabajoId]) REFERENCES [OPT_OrdenDeTrabajo] ([OrdenDeTrabajoId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_BitacoraOT] (
        [BitacoraOTId] int NOT NULL IDENTITY,
        [OrdenDeTrabajoId] int NOT NULL,
        [EstadoAnteriorId] int NOT NULL,
        [EstadoNuevoId] int NOT NULL,
        [Observacion] nvarchar(200) NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_BitacoraOT] PRIMARY KEY ([BitacoraOTId]),
        CONSTRAINT [FK_BitacoraOT_OrdenesDeTrabajo] FOREIGN KEY ([OrdenDeTrabajoId]) REFERENCES [OPT_OrdenDeTrabajo] ([OrdenDeTrabajoId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE TABLE [OPT_DetalleOT] (
        [DetalleOTId] int NOT NULL IDENTITY,
        [OrdenDeTrabajoId] int NOT NULL,
        [ProductoId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [ValorUnitario] decimal(18,2) NOT NULL,
        [CreadoEn] datetimeoffset NOT NULL,
        [CreadoPor] int NOT NULL,
        [ModificadoEn] datetimeoffset NULL,
        [ModificadoPor] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EliminadoEn] datetimeoffset NULL,
        [EliminadoPor] int NULL,
        CONSTRAINT [PK_OPT_DetalleOT] PRIMARY KEY ([DetalleOTId]),
        CONSTRAINT [FK_DetallesOT_OrdenesDeTrabajo] FOREIGN KEY ([OrdenDeTrabajoId]) REFERENCES [OPT_OrdenDeTrabajo] ([OrdenDeTrabajoId]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesOT_Productos] FOREIGN KEY ([ProductoId]) REFERENCES [OPT_Producto] ([ProductoId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CodigoOficial', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_Region]'))
        SET IDENTITY_INSERT [OPT_Region] ON;
    EXEC(N'INSERT INTO [OPT_Region] ([Id], [CodigoOficial], [Nombre])
    VALUES (1, N''I'', N''Región de Tarapacá''),
    (2, N''II'', N''Región de Antofagasta''),
    (3, N''III'', N''Región de Atacama''),
    (4, N''IV'', N''Región de Coquimbo''),
    (5, N''V'', N''Región de Valparaíso''),
    (6, N''VI'', N''Región del Libertador General Bernardo O''''Higgins''),
    (7, N''VII'', N''Región del Maule''),
    (8, N''VIII'', N''Región del Biobío''),
    (9, N''IX'', N''Región de La Araucanía''),
    (10, N''X'', N''Región de Los Lagos''),
    (11, N''XI'', N''Región de Aysén del General Carlos Ibáñez del Campo''),
    (12, N''XII'', N''Región de Magallanes y de la Antártica Chilena''),
    (13, N''XIII'', N''Región Metropolitana de Santiago''),
    (14, N''XIV'', N''Región de Los Ríos''),
    (15, N''XV'', N''Región de Arica y Parinacota''),
    (16, N''XVI'', N''Región de Ñuble'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CodigoOficial', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_Region]'))
        SET IDENTITY_INSERT [OPT_Region] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_Rol]'))
        SET IDENTITY_INSERT [OPT_Rol] ON;
    EXEC(N'INSERT INTO [OPT_Rol] ([Id], [Descripcion], [Nombre])
    VALUES (1, NULL, N''Administrador''),
    (2, NULL, N''Supervisor''),
    (3, NULL, N''Operador'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[OPT_Rol]'))
        SET IDENTITY_INSERT [OPT_Rol] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre', N'RegionId') AND [object_id] = OBJECT_ID(N'[OPT_Comuna]'))
        SET IDENTITY_INSERT [OPT_Comuna] ON;
    EXEC(N'INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (1, N''Iquique'', 1),
    (2, N''Alto Hospicio'', 1),
    (3, N''Pozo Almonte'', 1),
    (4, N''Camiña'', 1),
    (5, N''Colchane'', 1),
    (6, N''Huara'', 1),
    (7, N''Pica'', 1),
    (8, N''Antofagasta'', 2),
    (9, N''Mejillones'', 2),
    (10, N''Sierra Gorda'', 2),
    (11, N''Taltal'', 2),
    (12, N''Calama'', 2),
    (13, N''Ollagüe'', 2),
    (14, N''San Pedro de Atacama'', 2),
    (15, N''Tocopilla'', 2),
    (16, N''María Elena'', 2),
    (17, N''Copiapó'', 3),
    (18, N''Caldera'', 3),
    (19, N''Tierra Amarilla'', 3),
    (20, N''Chañaral'', 3),
    (21, N''Diego de Almagro'', 3),
    (22, N''Vallenar'', 3),
    (23, N''Alto del Carmen'', 3),
    (24, N''Freirina'', 3),
    (25, N''Huasco'', 3),
    (26, N''La Serena'', 4),
    (27, N''Coquimbo'', 4),
    (28, N''Andacollo'', 4),
    (29, N''La Higuera'', 4),
    (30, N''Paiguano'', 4),
    (31, N''Vicuña'', 4),
    (32, N''Illapel'', 4),
    (33, N''Canela'', 4),
    (34, N''Los Vilos'', 4),
    (35, N''Salamanca'', 4),
    (36, N''Ovalle'', 4),
    (37, N''Combarbalá'', 4),
    (38, N''Monte Patria'', 4),
    (39, N''Punitaqui'', 4),
    (40, N''Río Hurtado'', 4),
    (41, N''Valparaíso'', 5),
    (42, N''Casablanca'', 5);
    INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (43, N''Concón'', 5),
    (44, N''Juan Fernández'', 5),
    (45, N''Puchuncaví'', 5),
    (46, N''Quintero'', 5),
    (47, N''Viña del Mar'', 5),
    (48, N''Isla de Pascua'', 5),
    (49, N''Los Andes'', 5),
    (50, N''Calle Larga'', 5),
    (51, N''Rinconada'', 5),
    (52, N''San Esteban'', 5),
    (53, N''La Ligua'', 5),
    (54, N''Cabildo'', 5),
    (55, N''Papudo'', 5),
    (56, N''Petorca'', 5),
    (57, N''Zapallar'', 5),
    (58, N''Quillota'', 5),
    (59, N''Calera'', 5),
    (60, N''Hijuelas'', 5),
    (61, N''La Cruz'', 5),
    (62, N''Nogales'', 5),
    (63, N''San Antonio'', 5),
    (64, N''Algarrobo'', 5),
    (65, N''Cartagena'', 5),
    (66, N''El Quisco'', 5),
    (67, N''El Tabo'', 5),
    (68, N''Santo Domingo'', 5),
    (69, N''San Felipe'', 5),
    (70, N''Catemu'', 5),
    (71, N''Llaillay'', 5),
    (72, N''Panquehue'', 5),
    (73, N''Putaendo'', 5),
    (74, N''Santa María'', 5),
    (75, N''Quilpué'', 5),
    (76, N''Limache'', 5),
    (77, N''Olmué'', 5),
    (78, N''Villa Alemana'', 5),
    (79, N''Rancagua'', 6),
    (80, N''Codegua'', 6),
    (81, N''Coinco'', 6),
    (82, N''Coltauco'', 6),
    (83, N''Doñihue'', 6),
    (84, N''Graneros'', 6);
    INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (85, N''Las Cabras'', 6),
    (86, N''Machalí'', 6),
    (87, N''Malloa'', 6),
    (88, N''Mostazal'', 6),
    (89, N''Olivar'', 6),
    (90, N''Peumo'', 6),
    (91, N''Pichidegua'', 6),
    (92, N''Quinta de Tilcoco'', 6),
    (93, N''Rengo'', 6),
    (94, N''Requínoa'', 6),
    (95, N''San Vicente'', 6),
    (96, N''Pichilemu'', 6),
    (97, N''La Estrella'', 6),
    (98, N''Litueche'', 6),
    (99, N''Marchihue'', 6),
    (100, N''Navidad'', 6),
    (101, N''Paredones'', 6),
    (102, N''San Fernando'', 6),
    (103, N''Chépica'', 6),
    (104, N''Chimbarongo'', 6),
    (105, N''Lolol'', 6),
    (106, N''Nancagua'', 6),
    (107, N''Palmilla'', 6),
    (108, N''Peralillo'', 6),
    (109, N''Placilla'', 6),
    (110, N''Pumanque'', 6),
    (111, N''Santa Cruz'', 6),
    (112, N''Talca'', 7),
    (113, N''Constitución'', 7),
    (114, N''Curepto'', 7),
    (115, N''Empedrado'', 7),
    (116, N''Maule'', 7),
    (117, N''Pelarco'', 7),
    (118, N''Pencahue'', 7),
    (119, N''Río Claro'', 7),
    (120, N''San Clemente'', 7),
    (121, N''San Rafael'', 7),
    (122, N''Cauquenes'', 7),
    (123, N''Chanco'', 7),
    (124, N''Pelluhue'', 7),
    (125, N''Curicó'', 7),
    (126, N''Hualañé'', 7);
    INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (127, N''Licantén'', 7),
    (128, N''Molina'', 7),
    (129, N''Rauco'', 7),
    (130, N''Romeral'', 7),
    (131, N''Sagrada Familia'', 7),
    (132, N''Teno'', 7),
    (133, N''Vichuquén'', 7),
    (134, N''Linares'', 7),
    (135, N''Colbún'', 7),
    (136, N''Longaví'', 7),
    (137, N''Parral'', 7),
    (138, N''Retiro'', 7),
    (139, N''San Javier'', 7),
    (140, N''Villa Alegre'', 7),
    (141, N''Yerbas Buenas'', 7),
    (142, N''Concepción'', 8),
    (143, N''Coronel'', 8),
    (144, N''Chiguayante'', 8),
    (145, N''Florida'', 8),
    (146, N''Hualqui'', 8),
    (147, N''Lota'', 8),
    (148, N''Penco'', 8),
    (149, N''San Pedro de la Paz'', 8),
    (150, N''Santa Juana'', 8),
    (151, N''Talcahuano'', 8),
    (152, N''Tomé'', 8),
    (153, N''Hualpén'', 8),
    (154, N''Lebu'', 8),
    (155, N''Arauco'', 8),
    (156, N''Cañete'', 8),
    (157, N''Contulmo'', 8),
    (158, N''Curanilahue'', 8),
    (159, N''Los Álamos'', 8),
    (160, N''Tirúa'', 8),
    (161, N''Los Ángeles'', 8),
    (162, N''Antuco'', 8),
    (163, N''Cabrero'', 8),
    (164, N''Laja'', 8),
    (165, N''Mulchén'', 8),
    (166, N''Nacimiento'', 8),
    (167, N''Negrete'', 8),
    (168, N''Quilaco'', 8);
    INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (169, N''Quilleco'', 8),
    (170, N''San Rosendo'', 8),
    (171, N''Santa Bárbara'', 8),
    (172, N''Tucapel'', 8),
    (173, N''Yumbel'', 8),
    (174, N''Alto Biobío'', 8),
    (175, N''Temuco'', 9),
    (176, N''Carahue'', 9),
    (177, N''Cunco'', 9),
    (178, N''Curarrehue'', 9),
    (179, N''Freire'', 9),
    (180, N''Galvarino'', 9),
    (181, N''Gorbea'', 9),
    (182, N''Lautaro'', 9),
    (183, N''Loncoche'', 9),
    (184, N''Melipeuco'', 9),
    (185, N''Nueva Imperial'', 9),
    (186, N''Padre las Casas'', 9),
    (187, N''Perquenco'', 9),
    (188, N''Pitrufquén'', 9),
    (189, N''Pucón'', 9),
    (190, N''Saavedra'', 9),
    (191, N''Teodoro Schmidt'', 9),
    (192, N''Toltén'', 9),
    (193, N''Vilcún'', 9),
    (194, N''Villarrica'', 9),
    (195, N''Cholchol'', 9),
    (196, N''Angol'', 9),
    (197, N''Collipulli'', 9),
    (198, N''Curacautín'', 9),
    (199, N''Ercilla'', 9),
    (200, N''Lonquimay'', 9),
    (201, N''Los Sauces'', 9),
    (202, N''Lumaco'', 9),
    (203, N''Purén'', 9),
    (204, N''Renaico'', 9),
    (205, N''Traiguén'', 9),
    (206, N''Victoria'', 9),
    (207, N''Puerto Montt'', 10),
    (208, N''Calbuco'', 10),
    (209, N''Cochamó'', 10),
    (210, N''Fresia'', 10);
    INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (211, N''Frutillar'', 10),
    (212, N''Los Muermos'', 10),
    (213, N''Llanquihue'', 10),
    (214, N''Maullín'', 10),
    (215, N''Puerto Varas'', 10),
    (216, N''Castro'', 10),
    (217, N''Ancud'', 10),
    (218, N''Chonchi'', 10),
    (219, N''Curaco de Vélez'', 10),
    (220, N''Dalcahue'', 10),
    (221, N''Puqueldón'', 10),
    (222, N''Queilén'', 10),
    (223, N''Quellón'', 10),
    (224, N''Quemchi'', 10),
    (225, N''Quinchao'', 10),
    (226, N''Osorno'', 10),
    (227, N''Puerto Octay'', 10),
    (228, N''Purranque'', 10),
    (229, N''Puyehue'', 10),
    (230, N''Río Negro'', 10),
    (231, N''San Juan de la Costa'', 10),
    (232, N''San Pablo'', 10),
    (233, N''Chaitén'', 10),
    (234, N''Futaleufú'', 10),
    (235, N''Hualaihué'', 10),
    (236, N''Palena'', 10),
    (237, N''Coyhaique'', 11),
    (238, N''Lago Verde'', 11),
    (239, N''Aysén'', 11),
    (240, N''Cisnes'', 11),
    (241, N''Guaitecas'', 11),
    (242, N''Cochrane'', 11),
    (243, N''O''''Higgins'', 11),
    (244, N''Tortel'', 11),
    (245, N''Chile Chico'', 11),
    (246, N''Río Ibáñez'', 11),
    (247, N''Punta Arenas'', 12),
    (248, N''Laguna Blanca'', 12),
    (249, N''Río Verde'', 12),
    (250, N''San Gregorio'', 12),
    (251, N''Cabo de Hornos'', 12),
    (252, N''Antártica'', 12);
    INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (253, N''Porvenir'', 12),
    (254, N''Primavera'', 12),
    (255, N''Timaukel'', 12),
    (256, N''Natales'', 12),
    (257, N''Torres del Paine'', 12),
    (258, N''Santiago'', 13),
    (259, N''Cerrillos'', 13),
    (260, N''Cerro Navia'', 13),
    (261, N''Conchalí'', 13),
    (262, N''El Bosque'', 13),
    (263, N''Estación Central'', 13),
    (264, N''Huechuraba'', 13),
    (265, N''Independencia'', 13),
    (266, N''La Cisterna'', 13),
    (267, N''La Florida'', 13),
    (268, N''La Granja'', 13),
    (269, N''La Pintana'', 13),
    (270, N''La Reina'', 13),
    (271, N''Las Condes'', 13),
    (272, N''Lo Barnechea'', 13),
    (273, N''Lo Espejo'', 13),
    (274, N''Lo Prado'', 13),
    (275, N''Macul'', 13),
    (276, N''Maipú'', 13),
    (277, N''Ñuñoa'', 13),
    (278, N''Pedro Aguirre Cerda'', 13),
    (279, N''Peñalolén'', 13),
    (280, N''Providencia'', 13),
    (281, N''Pudahuel'', 13),
    (282, N''Quilicura'', 13),
    (283, N''Quinta Normal'', 13),
    (284, N''Recoleta'', 13),
    (285, N''Renca'', 13),
    (286, N''San Joaquín'', 13),
    (287, N''San Miguel'', 13),
    (288, N''San Ramón'', 13),
    (289, N''Vitacura'', 13),
    (290, N''Puente Alto'', 13),
    (291, N''Pirque'', 13),
    (292, N''San José de Maipo'', 13),
    (293, N''Colina'', 13),
    (294, N''Lampa'', 13);
    INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (295, N''Tiltil'', 13),
    (296, N''San Bernardo'', 13),
    (297, N''Buin'', 13),
    (298, N''Calera de Tango'', 13),
    (299, N''Paine'', 13),
    (300, N''Melipilla'', 13),
    (301, N''Alhué'', 13),
    (302, N''Curacaví'', 13),
    (303, N''María Pinto'', 13),
    (304, N''San Pedro'', 13),
    (305, N''Talagante'', 13),
    (306, N''El Monte'', 13),
    (307, N''Isla de Maipo'', 13),
    (308, N''Padre Hurtado'', 13),
    (309, N''Peñaflor'', 13),
    (310, N''Valdivia'', 14),
    (311, N''Corral'', 14),
    (312, N''Futrono'', 14),
    (313, N''La Unión'', 14),
    (314, N''Lago Ranco'', 14),
    (315, N''Lanco'', 14),
    (316, N''Los Lagos'', 14),
    (317, N''Máfil'', 14),
    (318, N''Mariquina'', 14),
    (319, N''Paillaco'', 14),
    (320, N''Panguipulli'', 14),
    (321, N''Río Bueno'', 14),
    (322, N''Arica'', 15),
    (323, N''Camarones'', 15),
    (324, N''Putre'', 15),
    (325, N''General Lagos'', 15),
    (326, N''Chillán'', 16),
    (327, N''Bulnes'', 16),
    (328, N''Chillán Viejo'', 16),
    (329, N''El Carmen'', 16),
    (330, N''Pemuco'', 16),
    (331, N''Pinto'', 16),
    (332, N''Quillón'', 16),
    (333, N''San Ignacio'', 16),
    (334, N''Yungay'', 16),
    (335, N''Quirihue'', 16),
    (336, N''Cobquecura'', 16);
    INSERT INTO [OPT_Comuna] ([Id], [Nombre], [RegionId])
    VALUES (337, N''Coelemu'', 16),
    (338, N''Ninhue'', 16),
    (339, N''Portezuelo'', 16),
    (340, N''Ránquil'', 16),
    (341, N''Treguaco'', 16),
    (342, N''San Carlos'', 16),
    (343, N''Coihueco'', 16),
    (344, N''Ñiquén'', 16),
    (345, N''San Fabián'', 16),
    (346, N''San Nicolás'', 16)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre', N'RegionId') AND [object_id] = OBJECT_ID(N'[OPT_Comuna]'))
        SET IDENTITY_INSERT [OPT_Comuna] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_Abonos_OrdenDeTrabajoId] ON [OPT_Abono] ([OrdenDeTrabajoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_Anamnesis_ClienteId] ON [OPT_Anamnesis] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Anamnesis_PublicId] ON [OPT_Anamnesis] ([PublicId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_BitacoraOT_OrdenDeTrabajoId] ON [OPT_BitacoraOT] ([OrdenDeTrabajoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_Clientes_ComunaId] ON [OPT_Cliente] ([ComunaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Clientes_PublicId] ON [OPT_Cliente] ([PublicId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Clientes_Rut] ON [OPT_Cliente] ([Rut]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_Comunas_RegionId] ON [OPT_Comuna] ([RegionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_DetallesOT_OrdenDeTrabajoId] ON [OPT_DetalleOT] ([OrdenDeTrabajoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_DetallesOT_ProductoId] ON [OPT_DetalleOT] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Empresas_PublicId] ON [OPT_Empresa] ([PublicId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Empresas_Rut] ON [OPT_Empresa] ([Rut]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_EmpresaSucursales_SucursalId] ON [OPT_EmpresaSucursal] ([SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_EmpresaSucursales_EmpresaSucursal] ON [OPT_EmpresaSucursal] ([EmpresaId], [SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_OPT_OrdenDeTrabajo_EmpresaId] ON [OPT_OrdenDeTrabajo] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_OrdenesDeTrabajo_ClienteId] ON [OPT_OrdenDeTrabajo] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_OrdenesDeTrabajo_EstadoOTId] ON [OPT_OrdenDeTrabajo] ([EstadoOTId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_OrdenesDeTrabajo_SucursalId] ON [OPT_OrdenDeTrabajo] ([SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_OrdenesDeTrabajo_NumeroOT] ON [OPT_OrdenDeTrabajo] ([NumeroOT]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_Productos_CategoriaId] ON [OPT_Producto] ([CategoriaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Productos_Codigo] ON [OPT_Producto] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_ProductoSucursales_SucursalId] ON [OPT_ProductoSucursal] ([SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_ProductoSucursales_ProductoSucursal] ON [OPT_ProductoSucursal] ([ProductoId], [SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_RecetasCristales_ClienteId] ON [OPT_RecetaCristales] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_RecetasCristales_PublicId] ON [OPT_RecetaCristales] ([PublicId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Regiones_CodigoOficial] ON [OPT_Region] ([CodigoOficial]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Roles_Nombre] ON [OPT_Rol] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UQ_Sucursales_UnicaMatriz] ON [OPT_Sucursal] ([EsMatriz]) WHERE [EsMatriz] = 1 AND [Eliminado] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_OPT_Usuario_RolId] ON [OPT_Usuario] ([RolId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_OPT_Usuario_SucursalActivaId] ON [OPT_Usuario] ([SucursalActivaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Usuarios_Email] ON [OPT_Usuario] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Usuarios_PublicId] ON [OPT_Usuario] ([PublicId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Usuarios_Rut] ON [OPT_Usuario] ([Rut]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE INDEX [IX_UsuarioSucursales_SucursalId] ON [OPT_UsuarioSucursal] ([SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_UsuarioSucursales_UsuarioSucursal] ON [OPT_UsuarioSucursal] ([UsuarioId], [SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820021254_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260820021254_Initial', N'8.0.30');
END;
GO

COMMIT;
GO

