/* =============================================================================
   013_bodegas.sql
   -----------------------------------------------------------------------------
   Modulo Bodegas (ADR 0012). Hasta hoy la Sucursal hacia de bodega: el stock
   colgaba de OPT_ProductoSucursal (Producto + Sucursal). Ahora una Sucursal
   tiene una o mas Bodegas y el stock pasa a vivir en la Bodega.

   Decisiones (ADR 0012):
     - OPT_Bodega         : tabla nueva (AuditableEntity + PublicId), 1 Sucursal -> N Bodegas.
                            Una (y solo una) bodega principal por sucursal.
     - OPT_ProductoBodega : stock por Producto + Bodega (StockActual/Minimo/Maximo).
     - OPT_ProductoSucursal conserva SOLO el PrecioVenta (se vende por sucursal, no
       por bodega) y pierde StockActual/StockMinimo/StockMaximo.
     - Stock negativo: no existe en el modelo nuevo (Egresar lo prohibe). Si alguna
       fila de ProductoSucursal lo trajera, se carga en 0 (opcion A del ADR).
     - "Toda sucursal tiene al menos una bodega": se siembra una bodega principal
       por cada sucursal existente; la regla para sucursales nuevas vive en la
       capa de aplicacion (no es constraint de BD).

   ESCRITO A MANO (no generado con dotnet ef) -- la entidad Bodega, ProductoBodega y
   sus IEntityTypeConfiguration se actualizaron en la MISMA sesion.

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;  -- requerido por los indices filtrados (sqlcmd lo deja OFF por defecto)
GO

BEGIN TRANSACTION;
GO

/* -----------------------------------------------------------------------------
   1. OPT_Bodega
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_Bodega]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_Bodega] (
        [BodegaId]       int              NOT NULL IDENTITY(1,1),
        [PublicId]       uniqueidentifier NOT NULL CONSTRAINT [DF_OPT_Bodega_PublicId] DEFAULT (NEWID()),
        [SucursalId]     int              NOT NULL,
        [Codigo]         nvarchar(20)     NOT NULL,
        [Nombre]         nvarchar(100)    NOT NULL,
        [EsPrincipal]    bit              NOT NULL CONSTRAINT [DF_OPT_Bodega_EsPrincipal] DEFAULT (0),
        [Activa]         bit              NOT NULL CONSTRAINT [DF_OPT_Bodega_Activa] DEFAULT (1),
        [CreadoEn]       datetimeoffset   NOT NULL,
        [CreadoPor]      int              NOT NULL,
        [ModificadoEn]   datetimeoffset   NULL,
        [ModificadoPor]  int              NULL,
        [Eliminado]      bit              NOT NULL CONSTRAINT [DF_OPT_Bodega_Eliminado] DEFAULT (0),
        [EliminadoEn]    datetimeoffset   NULL,
        [EliminadoPor]   int              NULL,
        CONSTRAINT [PK_OPT_Bodega] PRIMARY KEY CLUSTERED ([BodegaId]),
        CONSTRAINT [FK_Bodegas_Sucursales] FOREIGN KEY ([SucursalId])
            REFERENCES [OPT_Sucursal] ([SucursalId])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_Bodegas_PublicId] ON [OPT_Bodega] ([PublicId]);
    CREATE NONCLUSTERED INDEX [IX_Bodegas_SucursalId]      ON [OPT_Bodega] ([SucursalId]);
    -- Codigo unico dentro de la sucursal (entre las no eliminadas)
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_Bodegas_SucursalCodigo]
        ON [OPT_Bodega] ([SucursalId], [Codigo]) WHERE [Eliminado] = 0;
    -- Una sola bodega principal por sucursal
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_Bodegas_UnicaPrincipal]
        ON [OPT_Bodega] ([SucursalId]) WHERE [EsPrincipal] = 1 AND [Eliminado] = 0;
END;
GO

/* -----------------------------------------------------------------------------
   2. Una bodega principal por cada sucursal que aun no tenga ninguna
   -------------------------------------------------------------------------- */
DECLARE @usuario int = (SELECT MIN([UsuarioId]) FROM [OPT_Usuario]);

IF @usuario IS NOT NULL
    INSERT INTO [OPT_Bodega] ([SucursalId], [Codigo], [Nombre], [EsPrincipal], [Activa], [CreadoEn], [CreadoPor])
    SELECT s.[SucursalId],
           N'BOD-' + RIGHT(N'00' + CAST(s.[SucursalId] AS nvarchar(10)), 2) + N'-01',
           LEFT(N'Bodega ' + s.[Nombre], 100),
           1, 1, SYSDATETIMEOFFSET(), @usuario
    FROM [OPT_Sucursal] s
    WHERE s.[Eliminado] = 0
      AND NOT EXISTS (SELECT 1 FROM [OPT_Bodega] b WHERE b.[SucursalId] = s.[SucursalId] AND b.[Eliminado] = 0);
GO

/* -----------------------------------------------------------------------------
   3. OPT_ProductoBodega -- stock por Producto + Bodega
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[OPT_ProductoBodega]', N'U') IS NULL
BEGIN
    CREATE TABLE [OPT_ProductoBodega] (
        [ProductoBodegaId] int            NOT NULL IDENTITY(1,1),
        [ProductoId]       int            NOT NULL,
        [BodegaId]         int            NOT NULL,
        [StockActual]      int            NOT NULL CONSTRAINT [DF_OPT_ProductoBodega_StockActual] DEFAULT (0),
        [StockMinimo]      int            NOT NULL CONSTRAINT [DF_OPT_ProductoBodega_StockMinimo] DEFAULT (0),
        [StockMaximo]      int            NOT NULL CONSTRAINT [DF_OPT_ProductoBodega_StockMaximo] DEFAULT (0),
        [CreadoEn]         datetimeoffset NOT NULL,
        [CreadoPor]        int            NOT NULL,
        [ModificadoEn]     datetimeoffset NULL,
        [ModificadoPor]    int            NULL,
        [Eliminado]        bit            NOT NULL CONSTRAINT [DF_OPT_ProductoBodega_Eliminado] DEFAULT (0),
        [EliminadoEn]      datetimeoffset NULL,
        [EliminadoPor]     int            NULL,
        CONSTRAINT [PK_OPT_ProductoBodega] PRIMARY KEY CLUSTERED ([ProductoBodegaId]),
        CONSTRAINT [FK_ProductoBodegas_Productos] FOREIGN KEY ([ProductoId])
            REFERENCES [OPT_Producto] ([ProductoId]),
        CONSTRAINT [FK_ProductoBodegas_Bodegas] FOREIGN KEY ([BodegaId])
            REFERENCES [OPT_Bodega] ([BodegaId]),
        CONSTRAINT [CK_ProductoBodegas_StockNoNegativo] CHECK ([StockActual] >= 0)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_ProductoBodegas_ProductoBodega]
        ON [OPT_ProductoBodega] ([ProductoId], [BodegaId]);
    CREATE NONCLUSTERED INDEX [IX_ProductoBodegas_BodegaId] ON [OPT_ProductoBodega] ([BodegaId]);
END;
GO

/* -----------------------------------------------------------------------------
   4. Mover el stock de OPT_ProductoSucursal a la bodega principal de su sucursal
      y quitar esas columnas. Solo corre si las columnas todavia existen.
   -------------------------------------------------------------------------- */
IF COL_LENGTH(N'OPT_ProductoSucursal', N'StockActual') IS NOT NULL
BEGIN
    EXEC (N'
        INSERT INTO [OPT_ProductoBodega]
            ([ProductoId], [BodegaId], [StockActual], [StockMinimo], [StockMaximo],
             [CreadoEn], [CreadoPor], [Eliminado])
        SELECT ps.[ProductoId], b.[BodegaId],
               CASE WHEN ps.[StockActual] < 0 THEN 0 ELSE ps.[StockActual] END,   -- stock negativo -> 0 (ADR 0012)
               ps.[StockMinimo], ps.[StockMaximo],
               ps.[CreadoEn], ps.[CreadoPor], ps.[Eliminado]
        FROM [OPT_ProductoSucursal] ps
        JOIN [OPT_Bodega] b ON b.[SucursalId] = ps.[SucursalId] AND b.[EsPrincipal] = 1 AND b.[Eliminado] = 0
        WHERE NOT EXISTS (SELECT 1 FROM [OPT_ProductoBodega] pb
                          WHERE pb.[ProductoId] = ps.[ProductoId] AND pb.[BodegaId] = b.[BodegaId]);');

    DECLARE @col sysname, @sql nvarchar(max);
    DECLARE c CURSOR LOCAL FAST_FORWARD FOR
        SELECT name FROM (VALUES (N'StockActual'), (N'StockMinimo'), (N'StockMaximo')) v(name);
    OPEN c; FETCH NEXT FROM c INTO @col;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Constraint DEFAULT (si existe, tiene nombre generado)
        SELECT @sql = N'ALTER TABLE [OPT_ProductoSucursal] DROP CONSTRAINT [' + dc.[name] + N'];'
        FROM sys.default_constraints dc
        JOIN sys.columns col ON col.object_id = dc.parent_object_id AND col.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'OPT_ProductoSucursal') AND col.[name] = @col;
        IF @sql IS NOT NULL EXEC (@sql);
        SET @sql = NULL;

        EXEC (N'ALTER TABLE [OPT_ProductoSucursal] DROP COLUMN [' + @col + N'];');
        FETCH NEXT FROM c INTO @col;
    END;
    CLOSE c; DEALLOCATE c;
END;
GO

COMMIT TRANSACTION;
GO
