/* =============================================================================
   M007_backfill_producto_categoria_marca_modelo_color.sql
   -----------------------------------------------------------------------------
   Reclasifica los productos migrados (hoy todos en la categoria "General") y separa
   su Descripcion legacy "MARCA-MODELO-COLOR" en Marca / Modelo / Color. Requiere
   haber aplicado 014.

   Solo toca filas con CategoriaId = 1 (General): re-ejecutarlo no pisa lo que ya se
   clasifico ni lo que alguien edito a mano.

   Categoria (por la descripcion):
     - empieza con CRISTALES              -> Cristales (3)
     - contiene la palabra SOL            -> Lentes de Sol (4)
     - contiene LENTE o empieza con OTRO- -> Otros (5)
     - el resto                           -> Marcos (2)
   Solo los Marcos se separan en Marca/Modelo/Color; cristales, sol y otros quedan con
   los tres en NULL (y su Descripcion intacta).

   Marca (decisiones del negocio, 2026-10-19):
     - ANGELO, ANGELO-FALCONI y ANGELOFALCONI  -> "ANGELO FALCONI"
     - RAY, RAY-BAN y RAYBAN                   -> "RAYBAN"
     - BL, CX, GEORGE, JEAN, CARLOS, COOL      -> tal cual (primer segmento)
     - cualquier otra: el primer segmento antes del primer guion.
   Color: el ultimo segmento si es C1..C99 o un color conocido (NEGRO, CAFE, BLK...).
   Modelo: lo que queda entre la marca y el color. Si no queda nada, Modelo = NULL y la
   fila sale en el informe de excepciones para corregirla a mano.

   Descripcion: se recalcula como Marca-Modelo[-Color] solo en las filas con Marca y Modelo.

   Se ejecuta en una transaccion; imprime el informe al final.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'OPT_Producto', N'Marca') IS NULL
    THROW 50000, N'Falta aplicar 014_producto_categorias_marca_modelo_color.sql antes de M007.', 1;
GO

BEGIN TRANSACTION;
GO

IF OBJECT_ID(N'tempdb..#p') IS NOT NULL DROP TABLE #p;

/* 1. Descripcion normalizada + categoria */
SELECT p.[ProductoId] AS Id,
       CAST(UPPER(LTRIM(RTRIM(p.[Descripcion]))) AS nvarchar(200)) AS D,
       CAST(0 AS int) AS Cat,
       CAST(NULL AS nvarchar(100)) AS Marca,
       CAST(NULL AS nvarchar(200)) AS Resto,
       CAST(NULL AS nvarchar(100)) AS Modelo,
       CAST(NULL AS nvarchar(50))  AS Color
INTO #p
FROM [OPT_Producto] p
WHERE p.[CategoriaId] = 1;

UPDATE #p SET Cat =
    CASE WHEN D LIKE N'CRISTALES%'                                   THEN 3
         WHEN N'-' + REPLACE(D, N' ', N'-') + N'-' LIKE N'%-SOL-%'   THEN 4
         WHEN D LIKE N'%LENTE%' OR D LIKE N'OTRO-%'                  THEN 5
         ELSE 2 END;

/* 2. Marca y resto (solo Marcos) */
UPDATE #p SET
    Marca = CASE WHEN D LIKE N'RAY-BAN-%'          THEN N'RAYBAN'
                 WHEN D LIKE N'RAY-%'              THEN N'RAYBAN'
                 WHEN D LIKE N'ANGELO-FALCONI-%'   THEN N'ANGELO FALCONI'
                 WHEN D LIKE N'ANGELOFALCONI-%'    THEN N'ANGELO FALCONI'
                 WHEN D LIKE N'ANGELO-%'           THEN N'ANGELO FALCONI'
                 ELSE LEFT(D, CHARINDEX(N'-', D + N'-') - 1) END,
    Resto = CASE WHEN D LIKE N'RAY-BAN-%'          THEN SUBSTRING(D, 9, 200)
                 WHEN D LIKE N'RAY-%'              THEN SUBSTRING(D, 5, 200)
                 WHEN D LIKE N'ANGELO-FALCONI-%'   THEN SUBSTRING(D, 16, 200)
                 WHEN D LIKE N'ANGELOFALCONI-%'    THEN SUBSTRING(D, 15, 200)
                 WHEN D LIKE N'ANGELO-%'           THEN SUBSTRING(D, 8, 200)
                 ELSE SUBSTRING(D, CHARINDEX(N'-', D + N'-') + 1, 200) END
WHERE Cat = 2;

/* guiones sobrantes al final del resto */
UPDATE #p SET Resto = LEFT(Resto, LEN(Resto) - 1) WHERE Cat = 2 AND RIGHT(Resto, 1) = N'-';
UPDATE #p SET Resto = LEFT(Resto, LEN(Resto) - 1) WHERE Cat = 2 AND RIGHT(Resto, 1) = N'-';
/* y al inicio (p. ej. "ANGELO-FALCONI--FXAF-031X-C4") */
UPDATE #p SET Resto = SUBSTRING(Resto, 2, 200) WHERE Cat = 2 AND LEFT(Resto, 1) = N'-';
UPDATE #p SET Resto = SUBSTRING(Resto, 2, 200) WHERE Cat = 2 AND LEFT(Resto, 1) = N'-';
UPDATE #p SET Resto = LTRIM(RTRIM(Resto)) WHERE Cat = 2;

/* 3. Color = ultimo segmento si parece un color; Modelo = el resto */
;WITH t AS (
    SELECT Id, Resto,
           CASE WHEN CHARINDEX(N'-', Resto) > 0
                THEN REVERSE(LEFT(REVERSE(Resto), CHARINDEX(N'-', REVERSE(Resto)) - 1)) END AS Ultimo,
           CASE WHEN CHARINDEX(N'-', Resto) > 0
                THEN LEFT(Resto, LEN(Resto) - CHARINDEX(N'-', REVERSE(Resto))) END AS SinUltimo
    FROM #p WHERE Cat = 2
)
UPDATE p SET
    Color  = CASE WHEN t.Ultimo IS NOT NULL
                   AND (t.Ultimo LIKE N'C[0-9]' OR t.Ultimo LIKE N'C[0-9][0-9]'
                        OR t.Ultimo IN (N'NEGRO', N'CAFE', N'BLK', N'GRIS', N'LILA', N'BRWN', N'AZUL', N'DORADO',
                                        N'BLUE', N'TRANSPARENTE', N'RED', N'BURDEO', N'BLACK', N'GUN', N'PURPLE', N'ROJO'))
                  THEN t.Ultimo END,
    Modelo = CASE WHEN t.Ultimo IS NOT NULL
                   AND (t.Ultimo LIKE N'C[0-9]' OR t.Ultimo LIKE N'C[0-9][0-9]'
                        OR t.Ultimo IN (N'NEGRO', N'CAFE', N'BLK', N'GRIS', N'LILA', N'BRWN', N'AZUL', N'DORADO',
                                        N'BLUE', N'TRANSPARENTE', N'RED', N'BURDEO', N'BLACK', N'GUN', N'PURPLE', N'ROJO'))
                  THEN NULLIF(t.SinUltimo, N'')
                  ELSE NULLIF(p.Resto, N'') END
FROM #p p JOIN t ON t.Id = p.Id;

/* Resto de un solo segmento (sin guion): es el modelo, salvo que sea claramente un color */
UPDATE #p SET Modelo = NULLIF(Resto, N'')
WHERE Cat = 2 AND CHARINDEX(N'-', Resto) = 0 AND Color IS NULL;

/* 4. Aplicar */
UPDATE pr SET
    pr.[CategoriaId] = p.Cat,
    pr.[Marca]  = CASE WHEN p.Cat = 2 THEN p.Marca END,
    pr.[Modelo] = CASE WHEN p.Cat = 2 THEN p.Modelo END,
    pr.[Color]  = CASE WHEN p.Cat = 2 THEN p.Color END,
    pr.[Descripcion] = CASE WHEN p.Cat = 2 AND p.Marca IS NOT NULL AND p.Modelo IS NOT NULL
                            THEN LEFT(p.Marca + N'-' + p.Modelo + ISNULL(N'-' + p.Color, N''), 200)
                            ELSE pr.[Descripcion] END
FROM [OPT_Producto] pr
JOIN #p p ON p.Id = pr.[ProductoId];

/* 5. Informe */
SELECT c.[Nombre] AS Categoria, COUNT(*) AS Productos
FROM [OPT_Producto] pr JOIN [OPT_CategoriaProducto] c ON c.[Id] = pr.[CategoriaId]
GROUP BY c.[Nombre] ORDER BY c.[Nombre];

SELECT COUNT(*) AS MarcosSinMarcaOModelo
FROM [OPT_Producto] WHERE [CategoriaId] = 2 AND ([Marca] IS NULL OR [Modelo] IS NULL);

SELECT COUNT(DISTINCT [Marca]) AS MarcasDistintas FROM [OPT_Producto] WHERE [Marca] IS NOT NULL;

DROP TABLE #p;
GO

COMMIT TRANSACTION;
GO
