/* =====================================================================
   Actualizacion de stock de productos EXISTENTES - AQUAMED (idSucursal = 2)
   Base   : db_a25cfd_opt2 (LEGACY / produccion)
   Origen : doc_Cliente/Inventario/INVENTARIO_AQUAMED_16102026_actualizado.xlsx (Hoja1)
   Cruce  : contra doc_Cliente/Inventario/InventarioProduccion_20261019.xlsx
   Generado: 2026-10-19

   Resultado del analisis (filas del Excel con mismo Marca+Modelo+Color se suman):
     - 189 productos EXISTEN (226 u.)  -> se fija el stock en la sucursal 2 (conteo fisico)
     - ver script *_Productos_Nuevos* para los 63 productos que NO existen (66 u.)

   Reglas (mismas del inventario BRILAZ del 2026-10-08):
     - Sucursal: 2 (AQUAMED).
     - Stock = cantidad contada en el Excel (regularizacion). Queda registro en
       OPT_RegularizaProducto (StockAnterior -> StockNuevo), igual que la pantalla "Regularizar".
       Entrada / Salida / PrecioVenta / StockMinimo / StockMaximo / ControlStock NO se tocan.
     - Productos de la sucursal 2 que NO aparecen en el Excel no se modifican.
     - Si el producto no tiene fila en OPT_ProductoSucursal de la sucursal 2, se crea en 0 antes.
     - Con varias coincidencias en produccion se prefirio ControlStock = 1 y luego el mayor idProducto.
     - Criterio de coincidencia: la descripcion (OPT_Producto.Producto) termina en MODELO+COLOR con la
       misma marca, o Codigo = MODELO+COLOR. Correcciones de lectura aplicadas al Excel:
         Jorgio: FXJ0014 / FXJOO28 / FXJO0039 / DLJ0051 / FXJO-037 / DLJO-50 ... -> FXJOnnn / DLJOnnn
         Foose : FXF0087T / FXF0080T -> FXFO087T / FXFO080T
         Marcas: ROLLET -> ROLETT, LE GIRO -> LEGIRO, GATO FELIX / GATOFELIX, MOOD KIDS -> MOODKIDS, FOOSE KIDS -> FOOSE
     - Coincidencias por supuesto (8), revisar en Revision_Carga_Inventario_AQUAMED.xlsx:
         variante con/sin X en el modelo, alias de color (BROWN=BRWN, BELGE=BEIGE, TRANSP=TRANS),
         FX0039 leido como FXJO039, y Codigo coincidente con descripcion distinta (fila 31).
     - 67 de los 189 productos tienen ControlStock = 0 en produccion; igual que en la carga
       BRILAZ, no se modifica ese indicador.

   USO: ejecutar primero con @Confirmar = 0 (hace todo y hace ROLLBACK, muestra el resumen).
   Si el resumen coincide con lo esperado, cambiar a @Confirmar = 1 y re-ejecutar.
   Proteccion: aborta si ya existe una regularizacion con el mismo Motivo o si algun producto
   existente cambio de nombre / no existe.
   ===================================================================== */
USE db_a25cfd_opt2;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Confirmar   bit           = 0;          -- 0 = prueba (ROLLBACK) | 1 = aplicar (COMMIT)
DECLARE @idSucursal  int           = 2;          -- AQUAMED
DECLARE @Motivo      nvarchar(200) = N'Inventario armazones AQUAMED 2026-10-16 - actualizacion existentes';
DECLARE @Responsable nvarchar(100) = N'ALEXIS';
DECLARE @Ahora       datetime      = GETDATE();

IF EXISTS (SELECT 1 FROM dbo.OPT_RegularizaProducto WHERE Motivo = @Motivo)
BEGIN
    RAISERROR(N'Esta actualizacion ya fue cargada (existe una regularizacion con el mismo Motivo). Se aborta.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.OPT_Sucursal WHERE idSucursal = @idSucursal)
BEGIN
    RAISERROR(N'La sucursal indicada no existe. Se aborta.', 16, 1);
    RETURN;
END;

/* ---------------------------------------------------------------------
   1. Productos EXISTENTES (189)
   --------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Existentes') IS NOT NULL DROP TABLE #Existentes;
CREATE TABLE #Existentes (
    FilaExcel        varchar(60)   NOT NULL,
    idProducto       bigint        NOT NULL PRIMARY KEY,
    ProductoEsperado nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL,
    Cantidad         int           NOT NULL
);
INSERT INTO #Existentes (FilaExcel, idProducto, ProductoEsperado, Cantidad) VALUES
    (N'3', 3610, N'JORGIO-FXJO014-C7', 1),
    (N'4', 4817, N'JORGIO-FXJO028-C1', 1),
    (N'5,33,200', 3112, N'JORGIO-FXJO028-C3', 3),
    (N'7', 3317, N'JORGIO-FXJO012-C6', 1),
    (N'8', 4303, N'JORGIO-DLJO051-C3', 1),
    (N'9', 3904, N'JORGIO-DLJO053-C3', 1),
    (N'10', 4836, N'JORGIO-DLJO043-C4', 1),
    (N'11', 4312, N'JORGIO-FXJO037-C1', 1),
    (N'12', 4832, N'JORGIO-DLJO040-C2', 1),
    (N'13', 3871, N'JORGIO-FXJO030-C2', 1),
    (N'14,39,230', 4830, N'JORGIO-DLJO016-C3', 3),
    (N'15', 3113, N'JORGIO-FXJO028-C6', 1),
    (N'16,24,28', 696, N'JORGIO-FXJO-023-C5', 3),
    (N'18', 3868, N'JORGIO-FXJO029-C4', 1),
    (N'19', 3521, N'JORGIO-FXJO038-C3', 1),
    (N'20,34', 4841, N'JORGIO-DLJO050-C4', 2),
    (N'21', 4315, N'JORGIO-FXJO038-C6', 1),
    (N'22', 3908, N'JORGIO-FXJO020-C1', 1),
    (N'23', 3902, N'JORGIO-DLJO051-C5', 1),
    (N'25', 4931, N'JORGIO-DLJO045-C2', 1),
    (N'29', 4807, N'JORGIO-DLJO066-C3', 1),
    (N'30', 3897, N'JORGIO-DLJO047-C4', 1),
    (N'31', 5125, N'JORGIO-FXTO022-C13', 1),
    (N'32', 754, N'JORGIO-FXJO-003-C5', 1),
    (N'35', 3933, N'JORGIO-FXJO025-C2', 1),
    (N'36', 3893, N'JORGIO-DLJO042-C1', 1),
    (N'37', 4795, N'JORGIO-FXJO039-C5', 1),
    (N'38', 4831, N'JORGIO-DLJO040-C1', 1),
    (N'41,60,188', 3555, N'LEGIRO-PZ22005-C3', 3),
    (N'42', 4598, N'LUCERNA-50387-CELESTE', 1),
    (N'44', 2872, N'WIZZ-WO-12-C4', 1),
    (N'45', 2871, N'LEGIRO-PZ22003-C6', 1),
    (N'46', 3441, N'OCTAVIA-MW15281-C3', 1),
    (N'47', 819, N'TOOINCH-A10630--BRWN', 1),
    (N'48,75', 5203, N'ROLETT-Z3337-TRANSPARENTE', 2),
    (N'49', 4842, N'GATTIZONI-G60174X-C2', 1),
    (N'50', 3126, N'LEGIRO-PZ22004-C4', 1),
    (N'51', 3805, N'LEGIRO-PZ22002-C2', 1),
    (N'52', 2938, N'LEGIRO-PZ22002-C3', 1),
    (N'53', 4880, N'GATTIZONI-G60151X-C2', 1),
    (N'58', 4892, N'GATTIZONI-G60173X-C4', 1),
    (N'59,74', 3158, N'LEGIRO-PZ22018-C3', 2),
    (N'61', 4214, N'LEGIRO-PZ22020-C4', 1),
    (N'62', 3163, N'LEGIRO-PZ22021-C4', 1),
    (N'63,82', 3608, N'LEGIRO-PZ22001-C3', 2),
    (N'64', 2538, N'TOOINCH-A10615-BEIGE', 1),
    (N'65,67', 3850, N'GATTIZONI-G60157X-C3', 2),
    (N'73', 3156, N'LEGIRO-PZ22017-C1', 1),
    (N'76', 2140, N'NOUVELLE-163--176-H', 1),
    (N'81', 3148, N'LEGIRO-PZ22012-C3', 1),
    (N'83,261', 3755, N'LEGIRO-PZ22011-C3', 2),
    (N'84', 3593, N'LEGIRO-PZ22006-C8', 1),
    (N'86', 5183, N'LEGIRO-PZ22036-C1', 1),
    (N'87,92', 3073, N'ANGELOFALCONI-FXAF037-C3', 2),
    (N'89', 4895, N'ANGELO FALCONI-FXN20349-C4', 1),
    (N'90', 3487, N'ANGELOFALCONI-FXN20325-C2', 1),
    (N'95', 4929, N'ANGELO FALCONI-FXN20350-C2', 1),
    (N'97', 3077, N'ANGELOFALCONI-FXAF042-C5', 1),
    (N'100', 3062, N'ANGELOFALCONI-DLAF13-C3', 1),
    (N'101', 5112, N'ANGELO FALCONI-AMAF2261-C2', 1),
    (N'103', 3791, N'ANGELOF-FXN20346-C4', 1),
    (N'104', 4674, N'ANGELO-AMAF2257-C2', 1),
    (N'107', 3079, N'ANGELOFALCONI-FXAF043-C6', 1),
    (N'109', 4684, N'ANGELOFALCONI-AMAF2259-C2', 1),
    (N'110', 4650, N'ANGELO-AMAF2260-C3', 1),
    (N'114', 1160, N'ANGELO-FALCONI-FXN-20329-C5', 1),
    (N'116', 4528, N'GATO-FELIX-YS-24114-C3', 1),
    (N'117,272', 4278, N'FOOSE-FXFO087T-C1', 2),
    (N'118', 4276, N'FOOSE-FXFO080T-C5', 1),
    (N'119', 578, N'VESPA-VE-21036-F-C2', 1),
    (N'120', 1302, N'VESPA-VE-21036-F-C4', 1),
    (N'121,281', 1177, N'VESPA-VE-21035-F-C4', 2),
    (N'122', 2606, N'MOOD-KIDS-MK-202046-C10', 1),
    (N'123', 1985, N'WIZZ-KID-WK-3-C3', 1),
    (N'124', 3758, N'MAXS-DR20027-C5', 1),
    (N'125,273', 4280, N'FOOSE-FXFO087T-C4', 2),
    (N'126', 571, N'GEORGE-GE-3532-X-C3', 1),
    (N'127', 4425, N'COOL-K-2003-C2', 1),
    (N'129', 1084, N'WIZZ-KIDS-WK-3-C6', 1),
    (N'131', 1565, N'BL-MW-17227-C3', 1),
    (N'132', 4267, N'MAXS-A17228-C1', 1),
    (N'133', 2812, N'OCTAVIA-A16040-C1', 1),
    (N'134', 2830, N'MAXS-A17205-C6', 2),
    (N'135', 2753, N'OCTAVIA-A-15736-C3', 1),
    (N'136', 2818, N'MAXS-A17681-C2', 1),
    (N'137', 2891, N'OCTAVIA-A16427-C4', 2),
    (N'138', 2907, N'MAXS-A17598-C6', 1),
    (N'139', 1121, N'OCTAVIA-A-15912-C1', 1),
    (N'140', 2845, N'OCTAVIA-A15782-C1', 1),
    (N'141,171', 2885, N'OCTAVIA-A15736-C2', 2),
    (N'142', 3433, N'MAXS-MW16162-C3', 1),
    (N'143', 3436, N'MAXS-MW17293-C2', 1),
    (N'144', 3704, N'MAXS-DR20027-C2', 1),
    (N'145', 2153, N'BL-MW-1008-C1', 1),
    (N'146', 3377, N'OCTAVIA-ZL00817-C3', 1),
    (N'147', 2829, N'MAXS-A17656-C2', 1),
    (N'148', 1392, N'MAXS-A17115-C8', 1),
    (N'149', 2886, N'OCTAVIA-A17042-C4', 1),
    (N'150', 586, N'OCTAVIA-A-17514-C2', 1),
    (N'151', 3375, N'OCTAVIA-DR30022-C2', 2),
    (N'152,167', 958, N'MAXS-A-17656-C3', 2),
    (N'153,175', 689, N'MAXS-A-17627-C3', 2),
    (N'154', 2825, N'OCTAVIA-A16330-C4', 1),
    (N'155', 2893, N'MAXS-A17333-C2', 2),
    (N'156', 3681, N'MAXS-MW17292-C1', 2),
    (N'157', 3424, N'MAXS-MX17165-C2', 1),
    (N'159', 752, N'BL-MW-18113-C1', 1),
    (N'160', 2833, N'MAXS-DR20071-C5', 1),
    (N'161', 3398, N'OCTAVIA-MW16117-C3', 1),
    (N'162', 2877, N'OCTAVIA-A16400-C2', 1),
    (N'163', 521, N'MAXS-A17113-C2', 1),
    (N'164', 3363, N'MAXS-A17649-C4', 1),
    (N'165', 3368, N'MAXS-A17115-C12', 2),
    (N'166', 3370, N'OCTAVIA-A15457-C8', 1),
    (N'168,213', 2905, N'OCTAVIA-A16424-C8', 2),
    (N'169', 3374, N'OCTAVIA-MW16039-C1', 1),
    (N'170', 3879, N'MAXCOME-XAP21186-C3', 1),
    (N'172', 3367, N'MAXS-MW18246-C4', 1),
    (N'173', 4461, N'OCTAVIA-MW-16039-C4', 1),
    (N'174,212', 4459, N'MAXS-A17257-C2', 2),
    (N'176', 3584, N'MAXS-A17655-C6', 1),
    (N'177', 2822, N'OCTAVIA-A15290-C4', 1),
    (N'178', 3371, N'OCTAVIA-MW15457-C1', 1),
    (N'179', 1195, N'BL-WD-3119-C1', 1),
    (N'180', 1564, N'BL-MW-17124-C2', 1),
    (N'181', 3391, N'MAXS-MW17252-C5', 2),
    (N'182', 2888, N'OCTAVIA-A16307-C3', 1),
    (N'183', 3185, N'FOOSE-SSFO025-C3', 1),
    (N'184', 3427, N'OCTAVIA-MW16092-C3', 1),
    (N'185', 3796, N'LEGIRO-PZ22017-C3', 1),
    (N'186', 2887, N'OCTAVIA-A15966-C4', 1),
    (N'187', 694, N'NOUVELLE-176-176-B', 1),
    (N'190', 4314, N'JORGIO-FXJO037-C4', 1),
    (N'191', 4860, N'GEORGE-GE3558X-C5', 1),
    (N'192', 4861, N'GEORGE-GE3558X-C10', 1),
    (N'194', 4600, N'LUCERNA-8827-C2', 1),
    (N'196', 3159, N'LEGIRO-PZ22018-C6', 1),
    (N'197', 4327, N'LEGIRO-PZ22015-C6', 1),
    (N'198', 4302, N'JORGIO-DLJO050-C3', 1),
    (N'199', 3777, N'JORGIO-FXJO021-C1', 1),
    (N'204', 4255, N'GATTIZONI-G60036X-C10', 1),
    (N'206', 4862, N'GEORGE-GE3558X-C6', 1),
    (N'207', 577, N'BL-WD-3121-C2', 1),
    (N'208,224', 2911, N'MAXS-A17608-C6', 3),
    (N'210', 3709, N'MAXS-MW16242-C2', 1),
    (N'211', 1753, N'BL-WD-1043-C3', 1),
    (N'214', 2901, N'OCTAVIA-A16273-C2', 1),
    (N'215', 2914, N'OCTAVIA-A16119-C4', 1),
    (N'216', 2884, N'OCTAVIA-A16439-C5', 1),
    (N'217', 2828, N'MAXS-DR20009-C4', 1),
    (N'218', 1815, N'MAXS-DR-20036-C2', 1),
    (N'219', 3916, N'MAXS-A17636-C6', 2),
    (N'220', 2889, N'OCTAVIA-A16169-C1', 1),
    (N'221', 2741, N'MAXS-DR-20071-C6', 1),
    (N'222', 2819, N'MAXS-A17591-C7', 1),
    (N'223', 2904, N'OCTAVIA-A16434-C1', 1),
    (N'225', 4852, N'GEORGE-GE3551X-C3', 1),
    (N'228', 4844, N'GATTIZONI-G60175X-C1', 1),
    (N'229', 4802, N'JORGIO-DLJO053-C4', 1),
    (N'231', 3365, N'MAXS-A17510-C4', 1),
    (N'232,237', 4098, N'ANGELO-FALCONI-FXAF051X-C3', 2),
    (N'233', 4902, N'ANGELO FALCONI-PZAF066-C4', 1),
    (N'234,254', 4823, N'GEORGE-GE3570X-C4', 2),
    (N'238', 4324, N'LEGIRO-PZ22011-C1', 1),
    (N'239,265', 3114, N'JORGIO-FXJO029-C6', 2),
    (N'245', 4693, N'ANGELO-AF2229-C3', 1),
    (N'246', 3582, N'LINEAVIGO-FXN202073F-C2', 1),
    (N'247', 3865, N'JORGIO-FXJO021-C8', 1),
    (N'249', 2138, N'BL-WD-3095-C7', 1),
    (N'250', 4689, N'ROLETT-ZN3510-TRANS', 2),
    (N'251', 540, N'WIZZ-WO-17-C5', 1),
    (N'252', 3121, N'LEGIRO-PZ2202-C1', 1),
    (N'253', 3145, N'LEGIRO-PZ22011-C6', 1),
    (N'256', 4893, N'GATTIZONI-G60174X-C1', 1),
    (N'262', 3900, N'JORGIO-DLJO051-C2', 1),
    (N'263', 3326, N'JORGIO-FXJO036-C5', 1),
    (N'264', 3969, N'LEGIRO-PZ22019-C1', 1),
    (N'267', 4454, N'LEGIRO-PZ22029-C2', 1),
    (N'268', 4800, N'JORGIO-FXJO067-C7', 1),
    (N'269', 2528, N'NOUVELLE-173-173-A', 1),
    (N'271', 2207, N'YANGO-YA-12-C3', 1),
    (N'274', 4275, N'FOOSE-FXFO080T-C2', 1),
    (N'276', 509, N'BL-MW-17054-C1', 1),
    (N'278', 1973, N'GEORGE-GE-3531-X-C1', 1),
    (N'279', 3531, N'MOODKIDS-MK202052-C35', 1),
    (N'282', 1634, N'WIZZ-KID-WK-3-C1', 1),
    (N'283', 4460, N'MOOD-KIDS-MK-29283-C3', 1),
    (N'284', 2669, N'BL-MM-17054-C1', 1),
    (N'288', 2399, N'MOODKIDS-MK-202156-C2', 1);

/* ---------------------------------------------------------------------
   2. Validaciones previas (no escriben nada)
   --------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM #Existentes e
           LEFT JOIN dbo.OPT_Producto p ON p.idProducto = e.idProducto
           WHERE p.idProducto IS NULL OR p.Producto <> e.ProductoEsperado)
BEGIN
    SELECT 'ERROR: producto existente cambio o no existe' AS Problema, e.*, p.Producto AS ProductoActual
    FROM #Existentes e LEFT JOIN dbo.OPT_Producto p ON p.idProducto = e.idProducto
    WHERE p.idProducto IS NULL OR p.Producto <> e.ProductoEsperado;
    RAISERROR(N'Hay productos existentes que ya no coinciden con el analisis. Se aborta.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------
   3. Fijar stock en la sucursal (con registro de regularizacion)
   --------------------------------------------------------------------- */
-- 3.1 Si algun producto no tiene fila en la sucursal, se crea en 0
INSERT INTO dbo.OPT_ProductoSucursal (idProducto, idSucursal, Entrada, Salida, Stock, StockMinimo, StockMaximo, PrecioVenta)
SELECT e.idProducto, @idSucursal, 0, 0, 0, 0, 0, 0
FROM #Existentes e
WHERE NOT EXISTS (SELECT 1 FROM dbo.OPT_ProductoSucursal ps
                  WHERE ps.idProducto = e.idProducto AND ps.idSucursal = @idSucursal);

-- 3.2 Registro historico (StockAnterior -> StockNuevo)
INSERT INTO dbo.OPT_RegularizaProducto (idProducto, StockAnterior, StockNuevo, Fecha, Responsable, Motivo, idSucursal, PrecioAnterior, PrecioNuevo)
SELECT e.idProducto, ps.Stock, e.Cantidad, @Ahora, @Responsable, @Motivo, @idSucursal, ps.PrecioVenta, ps.PrecioVenta
FROM #Existentes e
JOIN dbo.OPT_ProductoSucursal ps ON ps.idProducto = e.idProducto AND ps.idSucursal = @idSucursal;

-- 3.3 Nuevo stock
UPDATE ps
SET    ps.Stock = e.Cantidad
FROM   dbo.OPT_ProductoSucursal ps
JOIN   #Existentes e ON e.idProducto = ps.idProducto
WHERE  ps.idSucursal = @idSucursal;

/* ---------------------------------------------------------------------
   4. Resumen / verificacion
   --------------------------------------------------------------------- */
SELECT 'Existentes actualizados' AS Tipo, COUNT(*) AS Productos, SUM(e.Cantidad) AS Unidades
FROM #Existentes e
JOIN dbo.OPT_ProductoSucursal ps ON ps.idProducto = e.idProducto AND ps.idSucursal = @idSucursal AND ps.Stock = e.Cantidad
UNION ALL
SELECT 'Regularizaciones registradas', COUNT(*), SUM(StockNuevo)
FROM dbo.OPT_RegularizaProducto WHERE Motivo = @Motivo;
-- Esperado: Existentes 189 / 226 u. - Regularizaciones 189 / 226 u.

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'Stock AQUAMED de productos existentes actualizado (COMMIT).';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'Modo prueba: se hizo ROLLBACK. Cambie @Confirmar = 1 para aplicar.';
END;
GO
