/* =============================================================================
   009_modulo_operativo.sql
   -----------------------------------------------------------------------------
   Primera etapa del Modulo Operativo (Operativos Oftalmologicos en terreno),
   segun OPT_Requerimiento_Modulo_Operativo.md (2026-09-15). Alcance de ESTA
   sesion: solo esquema -- entidades OPT.Domain, IEntityTypeConfiguration<T>,
   Application/API y frontend quedan para una sesion siguiente (igual patron
   que 004_comercial_pagos_cuotas.sql).

   El documento de requerimiento fue escrito para OTRO proyecto (multi-tenant,
   PK UNIQUEIDENTIFIER/NEWSEQUENTIALID, IsDeleted, modulo Agenda/Atencion,
   header X-Sucursal-Id) y tuvo que adaptarse a las convenciones reales de
   OPT_NET -- ver CLAUDE.md:
     - Sin multi-tenant: no se agrega TenantId a ninguna tabla.
     - PK interna int IDENTITY (AuditableEntity.Id) + PublicId (Guid,
       DEFAULT NEWID(), NO NEWSEQUENTIALID()) solo en recursos de primer nivel
       expuestos por la API -- igual patron que OrdenDeTrabajo (ADR 0004).
     - Borrado logico: columnas Eliminado/EliminadoEn/EliminadoPor (no IsDeleted).
     - OrdenDeTrabajo YA esta implementada por completo (no es modulo pendiente;
       el requerimiento la daba por no construida). Esto resuelve gratis el
       punto abierto 8.2 del requerimiento ("Monto Total Pagado"): no hace
       falta agregar EstadoPago/MontoPagado a OPT_OrdenDeTrabajo, porque ya
       tiene TotalAbonado (abonos + pagos, recalculado transaccionalmente) y
       Saldo -- "pagado" de un Operativo sale de SUM(OT.TotalAbonado).

   Decisiones tomadas con el usuario (2026-09-15) sobre los puntos abiertos de
   la seccion 8 del requerimiento:
     8.4 GastoOperativo: solo Monto + NumeroDocumento + Observacion (sin fecha
         propia ni categoria) -- tal como estaba redactado en el documento.
     8.5 Correlativo: autogenerado por la base de datos (SEQUENCE), no manual.
     8.6 Transicion a Anulado: solo desde PROSPECTO o INGRESADO -- una vez en
         COBRANZA ya no se puede anular, solo Cerrar. (Regla de flujo: vive en
         OPT.Domain cuando se implemente, igual que OrdenDeTrabajo.CambiarEstado
         -- no se modela como CHECK constraint.)
     8.8 Sucursal: OPT_Operativo lleva SucursalId obligatorio, igual que
         OPT_OrdenDeTrabajo -- permite reusar AutorizacionSucursal.ValidarAcceso
         tal cual cuando se implemente la API.

   Puntos abiertos -- RESUELTOS al construir Domain/Application/API (sesion 2026-09-15,
   misma sesion, sin aplicar aun a dbOPT_NET):
     8.1 MontoTotalVendido/MontoTotalPagado se recalculan sumando los snapshots
         (MontoVendidoSnapshot/MontoPagadoSnapshot, agregados a OPT_OperativoOT en esta
         misma sesion, ver mas abajo) de cada OT asociada -- se fijan al asociar la OT y se
         refrescan explicitamente via POST /api/operativos/{publicId}/recalcular-montos
         (Operativo.RecalcularMontosDesdeOT). MontoTotalGastos SI se recalcula en la misma
         transaccion del movimiento que lo origina (alta/baja de GastoOperativo), igual que
         OrdenDeTrabajo.Precio/TotalAbonado/Saldo (ADR 0003/0006) -- ese caso no necesito
         snapshot porque GastoOperativo es hijo directo del agregado Operativo.
     8.3 Formula de ganancia/perdida: se calculan AMBAS (Pagado - Gastos y
         Vendido - Gastos) en el DTO de detalle, sin persistir ninguna columna
         "Ganancia" -- son derivadas de columnas que ya se persisten.
     8.7 Permisos por rol: implementados con AutorizarRolesAttribute + RolesOPT
         (patron ya usado en el resto de la API) -- ver OperativosController.
     Regla "una OT con Operativo debe tener Empresa asociada": validada en
     AsociarOrdenAOperativoCommandHandler (OPT.Application), no como constraint de BD
     (igual criterio que el resto del esquema: ver reglas de "Base de datos" en CLAUDE.md).
     Regla "un gasto no se puede ingresar si el Operativo esta ANULADO": vive en
     Operativo.RegistrarGasto (OPT.Domain).

   Contenido:
     1. OPT_EstadoOperativo : catalogo nuevo (PROSPECTO/INGRESADO/COBRANZA/
                               CERRADO/ANULADO).
     2. SEQ_CorrelativoOperativo : secuencia para Operativo.Correlativo.
     3. OPT_Operativo       : tabla nueva (AuditableEntity + PublicId).
     4. OPT_OperativoOT     : tabla de relacion Operativo-OrdenDeTrabajo (1:N,
                               una OT pertenece a lo sumo un Operativo).
     5. OPT_GastoOperativo  : tabla nueva (AuditableEntity).

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

/* -----------------------------------------------------------------------------
   1. OPT_EstadoOperativo -- catalogo (CatalogEntity: Id + Nombre, sin auditoria)
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_EstadoOperativo]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_EstadoOperativo] (
        [Id]     int           NOT NULL,
        [Nombre] nvarchar(50)  NOT NULL,
        CONSTRAINT [PK_OPT_EstadoOperativo] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_EstadosOperativo_Nombre]
        ON [OPT_EstadoOperativo] ([Nombre]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [OPT_EstadoOperativo])
BEGIN
    INSERT INTO [OPT_EstadoOperativo] ([Id], [Nombre]) VALUES
        (1, N'PROSPECTO'),
        (2, N'INGRESADO'),
        (3, N'COBRANZA'),
        (4, N'CERRADO'),
        (5, N'ANULADO');
END;
GO

/* -----------------------------------------------------------------------------
   2. SEQ_CorrelativoOperativo -- Operativo.Correlativo (autogenerado, punto 8.5)
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE [name] = N'SEQ_CorrelativoOperativo')
BEGIN
    CREATE SEQUENCE [dbo].[SEQ_CorrelativoOperativo]
        AS int
        START WITH 1
        INCREMENT BY 1
        NO CYCLE;
END;
GO

/* -----------------------------------------------------------------------------
   3. OPT_Operativo -- raiz del modulo (AuditableEntity + PublicId)
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_Operativo]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_Operativo] (
        [OperativoId]         int              NOT NULL IDENTITY(1,1),
        [PublicId]            uniqueidentifier NOT NULL CONSTRAINT [DF_OPT_Operativo_PublicId] DEFAULT (NEWID()),
        [Correlativo]         int              NOT NULL CONSTRAINT [DF_OPT_Operativo_Correlativo] DEFAULT (NEXT VALUE FOR [dbo].[SEQ_CorrelativoOperativo]),
        [EmpresaId]           int              NOT NULL,
        [SucursalId]          int              NOT NULL,
        [EstadoOperativoId]   int              NOT NULL CONSTRAINT [DF_OPT_Operativo_EstadoOperativoId] DEFAULT (1), -- PROSPECTO
        [Fecha]               date             NOT NULL,
        [Observacion]         nvarchar(500)    NULL,
        [MontoTotalVendido]   decimal(18,2)    NOT NULL CONSTRAINT [DF_OPT_Operativo_MontoTotalVendido] DEFAULT (0),
        [MontoTotalPagado]    decimal(18,2)    NOT NULL CONSTRAINT [DF_OPT_Operativo_MontoTotalPagado] DEFAULT (0),
        [MontoTotalGastos]    decimal(18,2)    NOT NULL CONSTRAINT [DF_OPT_Operativo_MontoTotalGastos] DEFAULT (0),
        [CreadoEn]            datetimeoffset   NOT NULL,
        [CreadoPor]           int              NOT NULL,
        [ModificadoEn]        datetimeoffset   NULL,
        [ModificadoPor]       int              NULL,
        [Eliminado]           bit              NOT NULL CONSTRAINT [DF_OPT_Operativo_Eliminado] DEFAULT (0),
        [EliminadoEn]         datetimeoffset   NULL,
        [EliminadoPor]        int              NULL,
        CONSTRAINT [PK_OPT_Operativo] PRIMARY KEY CLUSTERED ([OperativoId]),
        CONSTRAINT [FK_Operativos_Empresas] FOREIGN KEY ([EmpresaId])
            REFERENCES [OPT_Empresa] ([EmpresaId]),
        CONSTRAINT [FK_Operativos_Sucursales] FOREIGN KEY ([SucursalId])
            REFERENCES [OPT_Sucursal] ([SucursalId]),
        CONSTRAINT [FK_Operativos_EstadosOperativo] FOREIGN KEY ([EstadoOperativoId])
            REFERENCES [OPT_EstadoOperativo] ([Id])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_Operativos_PublicId]     ON [OPT_Operativo] ([PublicId]);
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_Operativos_Correlativo]  ON [OPT_Operativo] ([Correlativo]);
    CREATE NONCLUSTERED INDEX [IX_Operativos_EmpresaId]           ON [OPT_Operativo] ([EmpresaId]);
    CREATE NONCLUSTERED INDEX [IX_Operativos_SucursalId]          ON [OPT_Operativo] ([SucursalId]);
    CREATE NONCLUSTERED INDEX [IX_Operativos_EstadoOperativoId]   ON [OPT_Operativo] ([EstadoOperativoId]);
    CREATE NONCLUSTERED INDEX [IX_Operativos_Fecha]               ON [OPT_Operativo] ([Fecha]);
END;
GO

/* -----------------------------------------------------------------------------
   4. OPT_OperativoOT -- relacion Operativo-OrdenDeTrabajo (tabla de union pura,
      sin auditoria, mismo patron que OPT_EmpresaSucursal/OPT_UsuarioSucursal).
      Una OT pertenece a lo sumo un Operativo -> indice unico en OrdenDeTrabajoId.
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_OperativoOT]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_OperativoOT] (
        [OperativoOTId]         int           NOT NULL IDENTITY(1,1),
        [OperativoId]           int           NOT NULL,
        [OrdenDeTrabajoId]      int           NOT NULL,
        -- Snapshot del Precio/TotalAbonado de la OT al momento de asociarla (o del último
        -- refresco explícito vía POST /operativos/{publicId}/recalcular-montos). Se agregó al
        -- construir la API (sesion 2026-09-15, punto abierto 8.1 del requerimiento): sin esto,
        -- Operativo.MontoTotalVendido/MontoTotalPagado no se pueden mantener como totales
        -- corrientes al asociar/quitar una OT sin recalcular todo el conjunto contra el agregado
        -- Comercial en cada operacion (acoplaria OPT.Domain.Entities.Operativo a OrdenDeTrabajo).
        [MontoVendidoSnapshot]  decimal(18,2) NOT NULL CONSTRAINT [DF_OPT_OperativoOT_MontoVendidoSnapshot] DEFAULT (0),
        [MontoPagadoSnapshot]   decimal(18,2) NOT NULL CONSTRAINT [DF_OPT_OperativoOT_MontoPagadoSnapshot] DEFAULT (0),
        CONSTRAINT [PK_OPT_OperativoOT] PRIMARY KEY CLUSTERED ([OperativoOTId]),
        CONSTRAINT [FK_OperativoOT_Operativos] FOREIGN KEY ([OperativoId])
            REFERENCES [OPT_Operativo] ([OperativoId]) ON DELETE CASCADE,
        CONSTRAINT [FK_OperativoOT_OrdenesDeTrabajo] FOREIGN KEY ([OrdenDeTrabajoId])
            REFERENCES [OPT_OrdenDeTrabajo] ([OrdenDeTrabajoId]) ON DELETE NO ACTION
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_OperativoOT_OrdenDeTrabajoId]
        ON [OPT_OperativoOT] ([OrdenDeTrabajoId]);
    CREATE NONCLUSTERED INDEX [IX_OperativoOT_OperativoId]
        ON [OPT_OperativoOT] ([OperativoId]);
END;
GO

/* -----------------------------------------------------------------------------
   5. OPT_GastoOperativo -- gastos de la jornada (AuditableEntity)
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_GastoOperativo]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_GastoOperativo] (
        [GastoOperativoId]  int              NOT NULL IDENTITY(1,1),
        [OperativoId]       int              NOT NULL,
        [Monto]             decimal(18,2)    NOT NULL,
        [NumeroDocumento]   nvarchar(50)     NULL,
        [Observacion]       nvarchar(500)    NULL,
        [CreadoEn]          datetimeoffset   NOT NULL,
        [CreadoPor]         int              NOT NULL,
        [ModificadoEn]      datetimeoffset   NULL,
        [ModificadoPor]     int              NULL,
        [Eliminado]         bit              NOT NULL CONSTRAINT [DF_OPT_GastoOperativo_Eliminado] DEFAULT (0),
        [EliminadoEn]       datetimeoffset   NULL,
        [EliminadoPor]      int              NULL,
        CONSTRAINT [PK_OPT_GastoOperativo] PRIMARY KEY CLUSTERED ([GastoOperativoId]),
        CONSTRAINT [FK_GastosOperativo_Operativos] FOREIGN KEY ([OperativoId])
            REFERENCES [OPT_Operativo] ([OperativoId])
    );

    CREATE NONCLUSTERED INDEX [IX_GastosOperativo_OperativoId] ON [OPT_GastoOperativo] ([OperativoId]);
END;
GO

COMMIT;
GO

PRINT '009_modulo_operativo.sql aplicado.';
GO
