/* =====================================================================
   Alta de productos NUEVOS - Inventario de TERRENO (idSucursal = 4)
   Base   : db_a25cfd_opt2 (LEGACY / produccion)
   Origen : doc_Cliente/Inventario/Inventario_de_Terreno_20261012.xlsx (hoja Inventario, 182 filas, 193 unidades)
   Cruce  : contra doc_Cliente/Inventario/InventarioProduccion_20261012.xlsx
   Generado: 2026-10-19

   Resultado del analisis (filas con misma Marca+Codigo se suman):
     - 147 modelos, 193 u.: NINGUNO existe en produccion como producto sin color -> se insertan todos.
     - No hay productos existentes que actualizar (por eso no se genera script de actualizacion).

   IMPORTANTE - el Excel de terreno NO trae color. En produccion cada color es un producto distinto
   (MARCA-MODELO-Cn), por lo que no se puede asignar el stock a un color. Decision tomada con el usuario:
   un producto por modelo, sin color:
       Codigo   = MODELO + 'SC'        (SC = sin color; evita chocar con Codigos existentes)
       Producto = MARCA-MODELO-SC
   86 de los 147 modelos ya existen en produccion con colores (ver hoja "Nuevos" del Excel de revision);
   esos quedaran duplicados por modelo hasta que se cuente el color y se regularice.

   Reglas (mismas de las cargas BRILAZ / AQUAMED):
     - ControlStock = 1. Una fila de OPT_ProductoSucursal por cada sucursal (como hace el sistema al
       crear un producto); el stock queda SOLO en la sucursal 4, con precio 0 en todas.
     - Se registra la entrada en OPT_RegularizaProducto (StockAnterior 0 -> StockNuevo).
     - La sucursal 4 NO figura en el export de produccion del 12-oct (solo 1, 2 y 3): el script aborta
       si no existe en OPT_Sucursal. Crearla antes de ejecutar.
     - Normalizacion de marca: Legiro -> LEGIRO, Luna Vision -> LUNAVISION, Belle Vue -> BELLEVUE,
       TOINCH -> TOOINCH, Angelo Faloni -> ANGELO FALCONI, Fellis -> FEILLIS, Brilaz -> BL,
       Luwena -> LUCERNA (LH3007 y 8827 existen bajo LUCERNA).
     - Correcciones de lectura: Jorgio FXJ0034 / DLJ0047 / FXJO 001 ... -> FXJOnnn / DLJOnnn; Foose F0 -> FO
       (DSF0112 -> DSFO112, XAF0062 -> XAFO062, XYF0-098 -> XYFO098, DSF0-113 -> DSFO113);
       Gattizoni GE60151X -> G60151X; guiones y espacios se quitan del modelo (PZ-22036, FXLV-006, FXAF-2251).
     - Revisar: filas 160 y 161 (ANGELO FALCONI FXAF-2251, 5 u. cada una, iguales) y fila 162 (JORGIO FXJO 001, 3 u.)
       por si son una misma toma repetida.

   USO: ejecutar primero con @Confirmar = 0 (hace todo y hace ROLLBACK, muestra el resumen).
   Si el resumen coincide con lo esperado, cambiar a @Confirmar = 1 y re-ejecutar.
   Proteccion: aborta si ya existe una regularizacion con el mismo Motivo, si la sucursal no existe
   o si algun codigo nuevo ya existe en OPT_Producto.
   ===================================================================== */
USE db_a25cfd_opt2;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Confirmar   bit           = 0;          -- 0 = prueba (ROLLBACK) | 1 = aplicar (COMMIT)
DECLARE @idSucursal  int           = 4;          -- Terreno
DECLARE @Motivo      nvarchar(200) = N'Inventario de terreno 2026-10-12 - productos nuevos';
DECLARE @Responsable nvarchar(100) = N'ALEXIS';
DECLARE @Ahora       datetime      = GETDATE();

IF EXISTS (SELECT 1 FROM dbo.OPT_RegularizaProducto WHERE Motivo = @Motivo)
BEGIN
    RAISERROR(N'Estos productos ya fueron cargados (existe una regularizacion con el mismo Motivo). Se aborta.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.OPT_Sucursal WHERE idSucursal = @idSucursal)
BEGIN
    RAISERROR(N'La sucursal indicada no existe en OPT_Sucursal. Cree la sucursal antes de ejecutar. Se aborta.', 16, 1);
    RETURN;
END;

/* ---------------------------------------------------------------------
   1. Productos NUEVOS (147)
   --------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Nuevos') IS NOT NULL DROP TABLE #Nuevos;
CREATE TABLE #Nuevos (
    FilaExcel  varchar(60)   NOT NULL,
    Codigo     nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    Producto   nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL,
    Cantidad   int           NOT NULL
);
INSERT INTO #Nuevos (FilaExcel, Codigo, Producto, Cantidad) VALUES
    (N'5,10,14,72,163', N'PZ22036SC', N'LEGIRO-PZ22036-SC', 5),
    (N'6', N'PZ22006SC', N'LEGIRO-PZ22006-SC', 1),
    (N'7', N'PZ22002SC', N'LEGIRO-PZ22002-SC', 2),
    (N'8', N'PZ22039SC', N'LEGIRO-PZ22039-SC', 1),
    (N'9', N'PZ22011SC', N'LEGIRO-PZ22011-SC', 1),
    (N'11', N'PZ22003SC', N'LEGIRO-PZ22003-SC', 1),
    (N'12,39', N'PZ22020SC', N'LEGIRO-PZ22020-SC', 2),
    (N'13', N'PZ22001SC', N'LEGIRO-PZ22001-SC', 1),
    (N'15,81', N'PZ22004SC', N'LEGIRO-PZ22004-SC', 2),
    (N'16,37', N'PZ22022SC', N'LEGIRO-PZ22022-SC', 2),
    (N'17,164', N'PZ22015SC', N'LEGIRO-PZ22015-SC', 2),
    (N'18', N'JH331SC', N'OSO EYEWEAR-JH331-SC', 1),
    (N'19', N'WD1488SC', N'OSO EYEWEAR-WD1488-SC', 1),
    (N'20', N'5502SC', N'SELENE-5502-SC', 1),
    (N'21', N'DL21031XSC', N'GEORGE-DL21031X-SC', 1),
    (N'22', N'G60161XSC', N'GATTIZONI-G60161X-SC', 1),
    (N'23,48', N'DLJO047SC', N'JORGIO-DLJO047-SC', 2),
    (N'24', N'FXJO034SC', N'JORGIO-FXJO034-SC', 1),
    (N'25', N'FXJO038SC', N'JORGIO-FXJO038-SC', 1),
    (N'26', N'FXJO021SC', N'JORGIO-FXJO021-SC', 1),
    (N'27', N'DLJO050SC', N'JORGIO-DLJO050-SC', 1),
    (N'28', N'DLJO052SC', N'JORGIO-DLJO052-SC', 1),
    (N'29', N'FXJO030SC', N'JORGIO-FXJO030-SC', 1),
    (N'30,55', N'TR27100SC', N'LUNAVISION-TR27100-SC', 2),
    (N'31,36', N'PZ22005SC', N'LEGIRO-PZ22005-SC', 2),
    (N'32', N'HLW29339SC', N'LEGIRO-HLW29339-SC', 1),
    (N'33', N'HLW29347SC', N'LEGIRO-HLW29347-SC', 1),
    (N'34', N'PZ22030ASC', N'LEGIRO-PZ22030A-SC', 1),
    (N'35', N'PZ22023SC', N'LEGIRO-PZ22023-SC', 1),
    (N'38,86', N'PZ22037SC', N'LEGIRO-PZ22037-SC', 2),
    (N'40', N'G60171XSC', N'GATTIZONI-G60171X-SC', 1),
    (N'41', N'S502SC', N'SELENE-S502-SC', 1),
    (N'42', N'S132SC', N'SELENE-S132-SC', 1),
    (N'43', N'GE3570XSC', N'GEORGE-GE3570X-SC', 1),
    (N'44', N'LH3007SC', N'LUCERNA-LH3007-SC', 1),
    (N'45', N'24009SC', N'OSO EYEWEAR-24009-SC', 1),
    (N'46', N'FXJO012SC', N'JORGIO-FXJO012-SC', 1),
    (N'47', N'DLJO044SC', N'JORGIO-DLJO044-SC', 1),
    (N'49', N'FXJO035SC', N'JORGIO-FXJO035-SC', 1),
    (N'50,66,162', N'FXJO001SC', N'JORGIO-FXJO001-SC', 5),
    (N'51', N'3003SC', N'AMERICAN-3003-SC', 1),
    (N'52,53', N'G60157XSC', N'GATTIZONI-G60157X-SC', 2),
    (N'54,67', N'GE3555XSC', N'GEORGE-GE3555X-SC', 2),
    (N'56,59', N'DLJO042SC', N'JORGIO-DLJO042-SC', 2),
    (N'57,63', N'DLJO015SC', N'JORGIO-DLJO015-SC', 2),
    (N'58,64,87', N'DLJO041SC', N'JORGIO-DLJO041-SC', 3),
    (N'60', N'FXJO032SC', N'JORGIO-FXJO032-SC', 1),
    (N'61', N'FXJO037SC', N'JORGIO-FXJO037-SC', 1),
    (N'62', N'DLJO051SC', N'JORGIO-DLJO051-SC', 1),
    (N'65', N'DLJO040SC', N'JORGIO-DLJO040-SC', 1),
    (N'68', N'GE3565XSC', N'GEORGE-GE3565X-SC', 1),
    (N'69', N'PZ22029SC', N'LEGIRO-PZ22029-SC', 1),
    (N'70,71,165', N'PZ22010SC', N'LEGIRO-PZ22010-SC', 3),
    (N'73', N'GE3563XSC', N'GEORGE-GE3563X-SC', 1),
    (N'74', N'GE3531XSC', N'GEORGE-GE3531X-SC', 1),
    (N'75', N'G60151XSC', N'GATTIZONI-G60151X-SC', 1),
    (N'76', N'A10644SC', N'TOOINCH-A10644-SC', 1),
    (N'77', N'8827SC', N'LUCERNA-8827-SC', 1),
    (N'78', N'VS84012SC', N'BELLEVUE-VS84012-SC', 1),
    (N'79', N'TR27055SC', N'LUNAVISION-TR27055-SC', 1),
    (N'80', N'WD3095SC', N'BL-WD3095-SC', 1),
    (N'82', N'WPS2578SC', N'BELLEVUE-WPS2578-SC', 1),
    (N'83', N'169SC', N'SELENE-169-SC', 1),
    (N'84', N'DLAF18SC', N'ANGELO FALCONI-DLAF18-SC', 1),
    (N'85', N'DR20027SC', N'MAXS-DR20027-SC', 1),
    (N'88', N'PZ22033SC', N'LEGIRO-PZ22033-SC', 1),
    (N'89', N'GE3548XSC', N'GEORGE-GE3548X-SC', 1),
    (N'90', N'PZ22018SC', N'LEGIRO-PZ22018-SC', 1),
    (N'91', N'MD216SC', N'SELENE-MD216-SC', 1),
    (N'92', N'DLAF25SC', N'ANGELO FALCONI-DLAF25-SC', 1),
    (N'93', N'GV2914SC', N'GUESS-GV2914-SC', 1),
    (N'94', N'KP3183SC', N'KIPLING-KP3183-SC', 1),
    (N'95', N'FK1019AWSC', N'FRIDA KAHLO-FK1019AW-SC', 1),
    (N'96', N'FK1013MSC', N'FRIDA KAHLO-FK1013M-SC', 1),
    (N'97', N'DSFO112SC', N'FOOSE-DSFO112-SC', 1),
    (N'98', N'XAFO062SC', N'FOOSE-XAFO062-SC', 1),
    (N'99', N'5008SC', N'OSO EYEWEAR-5008-SC', 1),
    (N'100', N'S194SC', N'SELENE-S194-SC', 1),
    (N'101,102', N'82099SC', N'OSO EYEWEAR-82099-SC', 2),
    (N'103,173', N'81059SC', N'CLARITY-81059-SC', 2),
    (N'104,175', N'95589SC', N'CLARITY-95589-SC', 2),
    (N'105', N'A16369SC', N'OCTAVIA-A16369-SC', 1),
    (N'106', N'A16434SC', N'OCTAVIA-A16434-SC', 1),
    (N'107', N'A17112SC', N'OCTAVIA-A17112-SC', 1),
    (N'108', N'A16439SC', N'OCTAVIA-A16439-SC', 1),
    (N'109', N'ZL00817SC', N'OCTAVIA-ZL00817-SC', 1),
    (N'110', N'FXN20325SC', N'ANGELO FALCONI-FXN20325-SC', 1),
    (N'111', N'FXN20326SC', N'ANGELO FALCONI-FXN20326-SC', 1),
    (N'112', N'FXN20340SC', N'ANGELO FALCONI-FXN20340-SC', 1),
    (N'113', N'WD2158SC', N'BL-WD2158-SC', 1),
    (N'114', N'MW17292SC', N'BL-MW17292-SC', 1),
    (N'115', N'MW20003SC', N'BL-MW20003-SC', 1),
    (N'116', N'MW17293SC', N'MAXS-MW17293-SC', 1),
    (N'117', N'A1772BSC', N'MAXS-A1772B-SC', 1),
    (N'118', N'A17608SC', N'MAXS-A17608-SC', 1),
    (N'119', N'MW16169SC', N'MAXS-MW16169-SC', 1),
    (N'120,140,141', N'FXLV006SC', N'LINEA VIGO-FXLV006-SC', 3),
    (N'121', N'FXLV001SC', N'LINEA VIGO-FXLV001-SC', 1),
    (N'122', N'FXLV007SC', N'LINEA VIGO-FXLV007-SC', 1),
    (N'123,142', N'ZD2128WSC', N'SILMO-ZD2128W-SC', 2),
    (N'124,147', N'FXLV004SC', N'LINEA VIGO-FXLV004-SC', 2),
    (N'125', N'ZD21279WSC', N'SILMO-ZD21279W-SC', 1),
    (N'126,132', N'LK8836SC', N'LEKE-LK8836-SC', 2),
    (N'127', N'LK8840SC', N'LEKE-LK8840-SC', 1),
    (N'128', N'2256SC', N'OSO EYEWEAR-2256-SC', 1),
    (N'129', N'58018SC', N'OSO EYEWEAR-58018-SC', 1),
    (N'130', N'58019SC', N'OSO EYEWEAR-58019-SC', 1),
    (N'131', N'85002SC', N'OSO EYEWEAR-85002-SC', 1),
    (N'133', N'LK8824SC', N'LEKE-LK8824-SC', 1),
    (N'134', N'2302SC', N'OSO EYEWEAR-2302-SC', 1),
    (N'135', N'G60119XSC', N'GATTIZONI-G60119X-SC', 1),
    (N'136,180', N'M3204SC', N'OSO EYEWEAR-M3204-SC', 2),
    (N'137', N'RK8013SC', N'OSO EYEWEAR-RK8013-SC', 1),
    (N'138', N'FXLV008SC', N'LINEA VIGO-FXLV008-SC', 1),
    (N'139', N'TR2343SC', N'FEILLIS-TR2343-SC', 1),
    (N'143', N'ZD21234SC', N'SILMO-ZD21234-SC', 1),
    (N'144,146', N'TJ2284SC', N'OSO EYEWEAR-TJ2284-SC', 2),
    (N'145', N'JL21214WSC', N'SILMO-JL21214W-SC', 1),
    (N'148', N'JL21215WSC', N'SILMO-JL21215W-SC', 1),
    (N'149,150', N'ZD21260WSC', N'SILMO-ZD21260W-SC', 2),
    (N'151', N'85003SC', N'OSO EYEWEAR-85003-SC', 1),
    (N'152', N'2215SC', N'OSO EYEWEAR-2215-SC', 1),
    (N'153', N'58021SC', N'OSO EYEWEAR-58021-SC', 1),
    (N'154', N'2216SC', N'OSO EYEWEAR-2216-SC', 1),
    (N'155', N'58016SC', N'OSO EYEWEAR-58016-SC', 1),
    (N'156', N'TJ21388SC', N'OSO EYEWEAR-TJ21388-SC', 1),
    (N'157', N'58022SC', N'OSO EYEWEAR-58022-SC', 1),
    (N'158', N'58012SC', N'OSO EYEWEAR-58012-SC', 1),
    (N'159', N'2295SC', N'OSO EYEWEAR-2295-SC', 1),
    (N'160,161', N'FXAF2251SC', N'ANGELO FALCONI-FXAF2251-SC', 10),
    (N'166', N'AMAF2261SC', N'ANGELO FALCONI-AMAF2261-SC', 1),
    (N'167', N'XAFO121SC', N'FOOSE-XAFO121-SC', 1),
    (N'168', N'AF2231SC', N'ANGELO FALCONI-AF2231-SC', 1),
    (N'169', N'XYFO098SC', N'FOOSE-XYFO098-SC', 1),
    (N'170', N'DSFO113SC', N'FOOSE-DSFO113-SC', 1),
    (N'171', N'048008SC', N'OAKLEY-048008-SC', 1),
    (N'172', N'S202SC', N'SELENE-S202-SC', 1),
    (N'174', N'LA7003SC', N'BELLEVUE-LA7003-SC', 1),
    (N'176', N'82142SC', N'CLARITY-82142-SC', 1),
    (N'177', N'91759SC', N'FANDIA-91759-SC', 1),
    (N'178', N'RK8007SC', N'OSO EYEWEAR-RK8007-SC', 1),
    (N'179,186', N'TJ9617SC', N'BELLEVUE-TJ9617-SC', 2),
    (N'181', N'M22713SC', N'OSO EYEWEAR-M22713-SC', 1),
    (N'182', N'PG4003SC', N'PRETTY GIRL-PG4003-SC', 1),
    (N'183', N'MB3218SC', N'OSO EYEWEAR-MB3218-SC', 1),
    (N'184', N'TR3071SC', N'BELLEVUE-TR3071-SC', 1),
    (N'185', N'TJ32001SC', N'OSO EYEWEAR-TJ32001-SC', 1);

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
-- Esperado: Nuevos 147 / 193 u. - Regularizaciones 147 / 193 u.

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'Productos de terreno insertados (COMMIT).';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'Modo prueba: se hizo ROLLBACK. Cambie @Confirmar = 1 para aplicar.';
END;
GO
