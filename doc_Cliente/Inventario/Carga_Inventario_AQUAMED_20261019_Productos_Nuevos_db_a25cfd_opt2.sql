/* =====================================================================
   Alta de productos NUEVOS - AQUAMED (idSucursal = 2)
   Base   : db_a25cfd_opt2 (LEGACY / produccion)
   Origen : doc_Cliente/Inventario/INVENTARIO_AQUAMED_16102026_actualizado.xlsx (Hoja1)
   Cruce  : contra doc_Cliente/Inventario/InventarioProduccion_20261019.xlsx
   Generado: 2026-10-19

   Resultado del analisis (filas del Excel con mismo Marca+Modelo+Color se suman):
     - 63 productos NO existen (66 u.) -> se insertan
     - ver script *_Actualiza_Existentes* para los 189 productos que ya existen (226 u.)

   Reglas (mismas del inventario BRILAZ del 2026-10-08):
     - Producto: Codigo = MODELO+COLOR sin separadores; Producto = MARCA-MODELO-COLOR; ControlStock = 1.
     - Una fila de OPT_ProductoSucursal por cada sucursal (como hace el sistema al crear un producto);
       el stock queda SOLO en la sucursal 2 (AQUAMED), con precio 0 en todas.
     - Se registra la entrada en OPT_RegularizaProducto (StockAnterior 0 -> StockNuevo).
     - Marcas normalizadas al nombre usado en produccion: ROLLET -> ROLETT, LE GIRO -> LEGIRO,
       MOOD KIDS -> MOODKIDS, FOOSE KIDS -> FOOSE. BEVELLE y BELLEVUE se dejaron tal como vienen.
     - Correcciones de lectura aplicadas al Excel: Jorgio FXJ0014 / FXJOO28 / DLJ0051 / DLJO-50 ... -> FXJOnnn / DLJOnnn;
       fila 101 AMA2261 -> AMAF2261.
     - Filas del Excel NO cargadas (ambiguas, requieren decision): 66 (OSO EYEWEAR JH4 / 96),
       69 y 202 (ROLETT/ROLLET "TRANSPARENTE" sin modelo) y 115 (fila vacia).
     - Posibles duplicados con productos ya existentes (nombre casi igual en produccion) - revisar
       y, si corresponde, sacarlos de #Nuevos y cargarlos como existentes:
         BL-WD12117-C2  ~  BL-WD-1217-C2
         GEORGE-GE3543X-C16  ~  GEORGE-GE-3543X-C6
         TOOINCH-A10644-BEIGE  ~  TOOINCH-A110644-BEIGE
         COOL-K2008-C6  ~  COOL-K2-2008-C6
         COOL-K2008-C1  ~  COOL-K2-2008-C1
       (detalle completo en Revision_Carga_Inventario_AQUAMED.xlsx, hoja "Nuevos")

   USO: ejecutar primero con @Confirmar = 0 (hace todo y hace ROLLBACK, muestra el resumen).
   Si el resumen coincide con lo esperado, cambiar a @Confirmar = 1 y re-ejecutar.
   Proteccion: aborta si ya existe una regularizacion con el mismo Motivo o si algun codigo nuevo
   ya existe en OPT_Producto.
   ===================================================================== */
USE db_a25cfd_opt2;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Confirmar   bit           = 0;          -- 0 = prueba (ROLLBACK) | 1 = aplicar (COMMIT)
DECLARE @idSucursal  int           = 2;          -- AQUAMED
DECLARE @Motivo      nvarchar(200) = N'Inventario armazones AQUAMED 2026-10-16 - productos nuevos';
DECLARE @Responsable nvarchar(100) = N'ALEXIS';
DECLARE @Ahora       datetime      = GETDATE();

IF EXISTS (SELECT 1 FROM dbo.OPT_RegularizaProducto WHERE Motivo = @Motivo)
BEGIN
    RAISERROR(N'Estos productos ya fueron cargados (existe una regularizacion con el mismo Motivo). Se aborta.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.OPT_Sucursal WHERE idSucursal = @idSucursal)
BEGIN
    RAISERROR(N'La sucursal indicada no existe. Se aborta.', 16, 1);
    RETURN;
END;

/* ---------------------------------------------------------------------
   1. Productos NUEVOS (63)
   --------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Nuevos') IS NOT NULL DROP TABLE #Nuevos;
CREATE TABLE #Nuevos (
    FilaExcel  varchar(60)   NOT NULL,
    Codigo     nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    Producto   nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL,
    Cantidad   int           NOT NULL
);
INSERT INTO #Nuevos (FilaExcel, Codigo, Producto, Cantidad) VALUES
    (N'6', N'FXJO036C4', N'JORGIO-FXJO036-C4', 1),
    (N'17', N'FXJO039C3', N'JORGIO-FXJO039-C3', 1),
    (N'26', N'FXJO039C6', N'JORGIO-FXJO039-C6', 1),
    (N'27', N'FXJO029C2', N'JORGIO-FXJO029-C2', 1),
    (N'40', N'GE3560XC13', N'GEORGE-GE3560X-C13', 1),
    (N'43', N'28021C5', N'ROLETT-28021-C5', 1),
    (N'54', N'28078C7', N'ROLETT-28078-C7', 1),
    (N'55,257', N'TR27094C4', N'LUNAVISION-TR27094-C4', 2),
    (N'56', N'27094C1', N'LUNAVISION-27094-C1', 1),
    (N'57,72', N'VS84019C2', N'BELLEVUE-VS84019-C2', 2),
    (N'68', N'TR055C3', N'LUNAVISION-TR055-C3', 1),
    (N'70', N'PZ22005DORADO', N'LEGIRO-PZ22005-DORADO', 1),
    (N'71', N'VS84008C2', N'BELLEVUE-VS84008-C2', 1),
    (N'77', N'28071C7', N'ROLETT-28071-C7', 1),
    (N'78', N'PZ22035C3', N'LEGIRO-PZ22035-C3', 1),
    (N'79', N'P8019C1', N'LUCERNA-P8019-C1', 1),
    (N'80', N'GE3552XC6', N'GEORGE-GE3552X-C6', 1),
    (N'85', N'MD201C4', N'BEVELLE-MD201-C4', 1),
    (N'88', N'FXN29314AC2', N'ANGELO FALCONI-FXN29314A-C2', 1),
    (N'91', N'FXN29314AC5', N'ANGELO FALCONI-FXN29314A-C5', 1),
    (N'93', N'ZN3510CAMUFLADO', N'ROLETT-ZN3510-CAMUFLADO', 1),
    (N'94', N'G60151XC5', N'GATTIZONI-G60151X-C5', 1),
    (N'96', N'FXN29314AC3', N'ANGELO FALCONI-FXN29314A-C3', 1),
    (N'98', N'AF2229C4', N'ANGELO FALCONI-AF2229-C4', 1),
    (N'99', N'AMAF2258C2', N'ANGELO FALCONI-AMAF2258-C2', 1),
    (N'102', N'AMAF2260C4', N'ANGELO FALCONI-AMAF2260-C4', 1),
    (N'105', N'AMAF2257C1', N'ANGELO FALCONI-AMAF2257-C1', 1),
    (N'106', N'AF049C1', N'ANGELO FALCONI-AF049-C1', 1),
    (N'108', N'VE21029C5', N'VESPA-VE21029-C5', 1),
    (N'111', N'AMAF2259C4', N'ANGELO FALCONI-AMAF2259-C4', 1),
    (N'112', N'FXN20343C5', N'ANGELO FALCONI-FXN20343-C5', 1),
    (N'113', N'KDAF2237C2', N'ANGELO FALCONI-KDAF2237-C2', 1),
    (N'128', N'VE21078C1', N'VESPA-VE21078-C1', 1),
    (N'130', N'METALAZUL', N'GENERICO-METAL-AZUL', 1),
    (N'158', N'WD12117C2', N'BL-WD12117-C2', 1),
    (N'189', N'G60125XC7', N'GATTIZONI-G60125X-C7', 1),
    (N'193', N'FXJO030C5', N'JORGIO-FXJO030-C5', 1),
    (N'195', N'GE3543XC16', N'GEORGE-GE3543X-C16', 1),
    (N'201', N'8827C1', N'LUCERNA-8827-C1', 1),
    (N'203', N'2288C7', N'ROLETT-2288-C7', 1),
    (N'205', N'A60151XC5', N'GATTIZONI-A60151X-C5', 1),
    (N'209', N'A17636C6', N'OCTAVIA-A17636-C6', 1),
    (N'226', N'MD294C30', N'BEVELLE-MD294-C30', 1),
    (N'227', N'FXJO001C2', N'JORGIO-FXJO001-C2', 1),
    (N'235', N'FXJO001C8', N'JORGIO-FXJO001-C8', 1),
    (N'236', N'JH496NEGRO', N'OSO EYEWEAR-JH496-NEGRO', 1),
    (N'240', N'VS83006C3', N'BELLEVUE-VS83006-C3', 1),
    (N'241', N'A1044BWHITE', N'TOOINCH-A1044B-WHITE', 1),
    (N'242', N'CO6C6', N'COOL-CO6-C6', 1),
    (N'243', N'CF5C4', N'COOL FLEX-CF5-C4', 1),
    (N'244', N'DS29300C5', N'LINEA VIGO-DS29300-C5', 1),
    (N'248', N'A1044BBROWNORANGE', N'TOOINCH-A1044B-BROWNORANGE', 1),
    (N'255', N'A10644BEIGE', N'TOOINCH-A10644-BEIGE', 1),
    (N'258', N'XO9CAFE', N'XION-XO9-CAFE', 1),
    (N'259', N'G60161XC8', N'GATTIZONI-G60161X-C8', 1),
    (N'260', N'GE3548XC2', N'GEORGE-GE3548X-C2', 1),
    (N'266', N'SS16C10', N'SELENE-SS16-C10', 1),
    (N'270', N'PK13C13', N'NOUVELLE-PK13-C13', 1),
    (N'275', N'K2008C6', N'COOL-K2008-C6', 1),
    (N'277,285', N'K2008C1', N'COOL-K2008-C1', 2),
    (N'280', N'A8882MC6', N'AKOKO-A8882M-C6', 1),
    (N'286', N'MW17316C1', N'BL-MW17316-C1', 1),
    (N'287', N'GH28259C3', N'GATO FELIX-GH28259-C3', 1);

/* ---------------------------------------------------------------------
   2. Validaciones previas (no escriben nada)
   --------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM #Nuevos n JOIN dbo.OPT_Producto p ON p.Codigo = n.Codigo)
BEGIN
    SELECT 'ERROR: el codigo nuevo ya existe' AS Problema, n.*, p.idProducto, p.Producto AS ProductoActual
    FROM #Nuevos n JOIN dbo.OPT_Producto p ON p.Codigo = n.Codigo;
    RAISERROR(N'Hay codigos nuevos que ya existen en OPT_Producto. Se aborta.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------
   3. Insertar producto + stock por sucursal + registro
   --------------------------------------------------------------------- */
DECLARE @Insertados TABLE (idProducto bigint NOT NULL, Codigo nvarchar(50) COLLATE DATABASE_DEFAULT NOT NULL);

INSERT INTO dbo.OPT_Producto (Codigo, Producto, FechaIngreso, ControlStock)
OUTPUT inserted.idProducto, inserted.Codigo INTO @Insertados (idProducto, Codigo)
SELECT n.Codigo, n.Producto, @Ahora, 1
FROM #Nuevos n;

-- Una fila por cada sucursal (como hace el sistema al crear un producto); stock solo en @idSucursal
INSERT INTO dbo.OPT_ProductoSucursal (idProducto, idSucursal, Entrada, Salida, Stock, StockMinimo, StockMaximo, PrecioVenta)
SELECT i.idProducto, s.idSucursal, 0, 0,
       CASE WHEN s.idSucursal = @idSucursal THEN n.Cantidad ELSE 0 END,
       0, 0, 0
FROM @Insertados i
JOIN #Nuevos n ON n.Codigo = i.Codigo
CROSS JOIN dbo.OPT_Sucursal s;

INSERT INTO dbo.OPT_RegularizaProducto (idProducto, StockAnterior, StockNuevo, Fecha, Responsable, Motivo, idSucursal, PrecioAnterior, PrecioNuevo)
SELECT i.idProducto, 0, n.Cantidad, @Ahora, @Responsable, @Motivo, @idSucursal, 0, 0
FROM @Insertados i
JOIN #Nuevos n ON n.Codigo = i.Codigo;

/* ---------------------------------------------------------------------
   4. Resumen / verificacion
   --------------------------------------------------------------------- */
SELECT 'Nuevos insertados' AS Tipo, COUNT(*) AS Productos, SUM(n.Cantidad) AS Unidades
FROM @Insertados i JOIN #Nuevos n ON n.Codigo = i.Codigo
JOIN dbo.OPT_ProductoSucursal ps ON ps.idProducto = i.idProducto AND ps.idSucursal = @idSucursal AND ps.Stock = n.Cantidad
UNION ALL
SELECT 'Regularizaciones registradas', COUNT(*), SUM(StockNuevo)
FROM dbo.OPT_RegularizaProducto WHERE Motivo = @Motivo;
-- Esperado: Nuevos 63 / 66 u. - Regularizaciones 63 / 66 u.

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'Productos nuevos AQUAMED insertados (COMMIT).';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'Modo prueba: se hizo ROLLBACK. Cambie @Confirmar = 1 para aplicar.';
END;
GO
