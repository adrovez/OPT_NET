/* =============================================================================
   014_producto_categorias_marca_modelo_color.sql
   -----------------------------------------------------------------------------
   Catalogo de Productos (ADR 0013).

     1. OPT_CategoriaProducto: ademas de "General" (1, se conserva) se siembran
        Marcos (2), Cristales (3), Lentes de Sol (4) y Otros (5).
     2. OPT_Producto: columnas Marca, Modelo y Color (todas NULL: las filas existentes
        parten vacias y se completan con migracion/M007; cristales y "Otros" no las usan).
        Marca y Modelo son obligatorias solo para Marcos y Lentes de Sol, y eso lo valida
        el dominio, no la BD. Descripcion pasa a ser CALCULADA ("Marca-Modelo-Color") para
        esas dos categorias, tambien en el dominio.

   ESCRITO A MANO (no generado con dotnet ef) -- la entidad Producto, la constante
   CategoriasProducto y ProductoConfiguration se actualizaron en la MISMA sesion.

   Idempotente: se puede re-ejecutar sin efecto.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

/* -----------------------------------------------------------------------------
   1. Categorias
   -------------------------------------------------------------------------- */
INSERT INTO [OPT_CategoriaProducto] ([Id], [Nombre])
SELECT v.[Id], v.[Nombre]
FROM (VALUES (2, N'Marcos'), (3, N'Cristales'), (4, N'Lentes de Sol'), (5, N'Otros')) AS v ([Id], [Nombre])
WHERE NOT EXISTS (SELECT 1 FROM [OPT_CategoriaProducto] c WHERE c.[Id] = v.[Id]);
GO

/* -----------------------------------------------------------------------------
   2. Marca / Modelo / Color
   -------------------------------------------------------------------------- */
IF COL_LENGTH(N'OPT_Producto', N'Marca') IS NULL
    ALTER TABLE [OPT_Producto] ADD [Marca] nvarchar(100) NULL;
GO
IF COL_LENGTH(N'OPT_Producto', N'Modelo') IS NULL
    ALTER TABLE [OPT_Producto] ADD [Modelo] nvarchar(100) NULL;
GO
IF COL_LENGTH(N'OPT_Producto', N'Color') IS NULL
    ALTER TABLE [OPT_Producto] ADD [Color] nvarchar(50) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Productos_Marca' AND object_id = OBJECT_ID(N'[OPT_Producto]'))
    CREATE INDEX [IX_Productos_Marca] ON [OPT_Producto] ([Marca]);
GO

COMMIT TRANSACTION;
GO

PRINT N'014 aplicado: categorias Marcos/Cristales/Lentes de Sol/Otros + Marca/Modelo/Color en OPT_Producto.';
GO
