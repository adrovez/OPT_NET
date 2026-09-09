/* =============================================================================
   004_comercial_pagos_cuotas.sql
   -----------------------------------------------------------------------------
   Extiende el esquema del modulo Comercial para poder recibir los datos del
   legacy (db_a25cfd_opt2) correspondientes a Orden de Trabajo, Pagos, Abonos y
   Cuotas.

   Contenido:
     1. OPT_FormaPago     : agrega el valor CHEQUE (id 5) -- usado por OPT_Pago
                            en el legacy y sin equivalente en el catalogo actual.
     2. OPT_EstadoCuota   : catalogo nuevo (PENDIENTE / PAGADA / ANULADA).
     3. OPT_OrdenDeTrabajo: agrega Beneficiario, FechaAtencion, HoraEntrega y
                            NumeroCuotas (campos del legacy sin destino).
     4. OPT_Pago          : tabla nueva (3.736 filas en el legacy).
     5. OPT_Cuota         : tabla nueva (34.110 filas en el legacy).

   IMPORTANTE: este script se escribio a mano (no se genero con
   `dotnet ef migrations script`) porque el alcance de la sesion era solo base
   de datos. Las entidades OPT.Domain (Pago, Cuota, EstadoCuota) y sus
   IEntityTypeConfiguration<T> siguen pendientes: hasta que existan, el modelo
   de EF Core y la base de datos estan desalineados.

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

/* -----------------------------------------------------------------------------
   1. OPT_FormaPago -- valor CHEQUE
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [OPT_FormaPago] WHERE [Id] = 5)
BEGIN
    INSERT INTO [OPT_FormaPago] ([Id], [Nombre]) VALUES (5, N'CHEQUE');
END;
GO

/* -----------------------------------------------------------------------------
   2. OPT_EstadoCuota -- catalogo (CatalogEntity: Id + Nombre, sin auditoria)
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_EstadoCuota]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_EstadoCuota] (
        [Id]     int           NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_OPT_EstadoCuota] PRIMARY KEY CLUSTERED ([Id])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [OPT_EstadoCuota])
BEGIN
    INSERT INTO [OPT_EstadoCuota] ([Id], [Nombre]) VALUES
        (1, N'PENDIENTE'),
        (2, N'PAGADA'),
        (3, N'ANULADA');
END;
GO

/* -----------------------------------------------------------------------------
   3. OPT_OrdenDeTrabajo -- columnas del legacy sin destino
   -------------------------------------------------------------------------- */
IF COL_LENGTH(N'OPT_OrdenDeTrabajo', N'Beneficiario') IS NULL
    ALTER TABLE [OPT_OrdenDeTrabajo] ADD [Beneficiario] nvarchar(100) NULL;
GO
IF COL_LENGTH(N'OPT_OrdenDeTrabajo', N'FechaAtencion') IS NULL
    ALTER TABLE [OPT_OrdenDeTrabajo] ADD [FechaAtencion] date NULL;
GO
IF COL_LENGTH(N'OPT_OrdenDeTrabajo', N'HoraEntrega') IS NULL
    ALTER TABLE [OPT_OrdenDeTrabajo] ADD [HoraEntrega] time(0) NULL;
GO
IF COL_LENGTH(N'OPT_OrdenDeTrabajo', N'NumeroCuotas') IS NULL
    ALTER TABLE [OPT_OrdenDeTrabajo] ADD [NumeroCuotas] int NULL;
GO

/* -----------------------------------------------------------------------------
   4. OPT_Pago -- pagos posteriores al abono inicial (AuditableEntity)
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_Pago]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_Pago] (
        [PagoId]            int              NOT NULL IDENTITY(1,1),
        [OrdenDeTrabajoId]  int              NOT NULL,
        [FechaPago]         datetimeoffset   NOT NULL,
        [Monto]             decimal(18,2)    NOT NULL,
        [FormaPagoId]       int              NOT NULL,
        [Referencia]        nvarchar(100)    NULL,
        [CreadoEn]          datetimeoffset   NOT NULL,
        [CreadoPor]         int              NOT NULL,
        [ModificadoEn]      datetimeoffset   NULL,
        [ModificadoPor]     int              NULL,
        [Eliminado]         bit              NOT NULL CONSTRAINT [DF_OPT_Pago_Eliminado] DEFAULT (0),
        [EliminadoEn]       datetimeoffset   NULL,
        [EliminadoPor]      int              NULL,
        CONSTRAINT [PK_OPT_Pago] PRIMARY KEY CLUSTERED ([PagoId]),
        CONSTRAINT [FK_Pagos_OrdenesDeTrabajo] FOREIGN KEY ([OrdenDeTrabajoId])
            REFERENCES [OPT_OrdenDeTrabajo] ([OrdenDeTrabajoId]),
        CONSTRAINT [FK_Pagos_FormasPago] FOREIGN KEY ([FormaPagoId])
            REFERENCES [OPT_FormaPago] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_Pagos_OrdenDeTrabajoId] ON [OPT_Pago] ([OrdenDeTrabajoId]);
    CREATE NONCLUSTERED INDEX [IX_Pagos_FormaPagoId]      ON [OPT_Pago] ([FormaPagoId]);
END;
GO

/* -----------------------------------------------------------------------------
   5. OPT_Cuota -- plan de cuotas de la OT (AuditableEntity)
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_Cuota]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_Cuota] (
        [CuotaId]           int              NOT NULL IDENTITY(1,1),
        [OrdenDeTrabajoId]  int              NOT NULL,
        [Numero]            int              NOT NULL,
        [ValorCuota]        decimal(18,2)    NOT NULL,
        [FechaVencimiento]  date             NOT NULL,
        [FechaPago]         datetimeoffset   NULL,
        [FormaPagoId]       int              NULL,
        [EstadoCuotaId]     int              NOT NULL,
        [CreadoEn]          datetimeoffset   NOT NULL,
        [CreadoPor]         int              NOT NULL,
        [ModificadoEn]      datetimeoffset   NULL,
        [ModificadoPor]     int              NULL,
        [Eliminado]         bit              NOT NULL CONSTRAINT [DF_OPT_Cuota_Eliminado] DEFAULT (0),
        [EliminadoEn]       datetimeoffset   NULL,
        [EliminadoPor]      int              NULL,
        CONSTRAINT [PK_OPT_Cuota] PRIMARY KEY CLUSTERED ([CuotaId]),
        CONSTRAINT [FK_Cuotas_OrdenesDeTrabajo] FOREIGN KEY ([OrdenDeTrabajoId])
            REFERENCES [OPT_OrdenDeTrabajo] ([OrdenDeTrabajoId]),
        CONSTRAINT [FK_Cuotas_FormasPago] FOREIGN KEY ([FormaPagoId])
            REFERENCES [OPT_FormaPago] ([Id]),
        CONSTRAINT [FK_Cuotas_EstadosCuota] FOREIGN KEY ([EstadoCuotaId])
            REFERENCES [OPT_EstadoCuota] ([Id])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_Cuotas_OrdenDeTrabajoId_Numero]
        ON [OPT_Cuota] ([OrdenDeTrabajoId], [Numero]);
    CREATE NONCLUSTERED INDEX [IX_Cuotas_EstadoCuotaId] ON [OPT_Cuota] ([EstadoCuotaId]);
END;
GO

COMMIT;
GO

PRINT '004_comercial_pagos_cuotas.sql aplicado.';
GO
