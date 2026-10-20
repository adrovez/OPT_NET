/* =====================================================================
   Carga de inventario de armazones BRILAZ  ->  db_a25cfd_opt2 (LEGACY)
   Origen : doc_Cliente/Inventario_armazones_BRILAZ.xlsx (311 filas, 332 unidades)
   Generado: 2026-09-28 contra la BD local (4.874 productos al momento del análisis)

   Resultado de la validación (filas del Excel con mismo Modelo+Color se suman):
     - 107 productos EXISTEN  -> se fija el stock (regularización)
     - 199 productos NO existen -> se insertan
   Criterio de coincidencia: descripción del producto (OPT_Producto.Producto) sin
   separadores termina en MODELO+COLOR del Excel, o Codigo = MODELO+COLOR.
   Si hay varias coincidencias se prefiere la de la misma marca y, entre ellas,
   la más reciente (mayor idProducto). Los códigos del Excel se toman tal cual
   (EX/EXN/EXTO NO se homologan a FX/FXN/FXJO, por decisión del usuario).

   Reglas aplicadas (decididas con el usuario):
     - Sucursal: 1 (Casa Matriz)
     - Existentes: Stock = cantidad del Excel (conteo físico). Se deja registro
       en OPT_RegularizaProducto (StockAnterior -> StockNuevo), igual que la
       pantalla "Regularizar" del sistema. Entrada/Salida/PrecioVenta no se tocan.
     - Nuevos: OPT_Producto (ControlStock = 1) + una fila de OPT_ProductoSucursal
       por cada sucursal (stock solo en la sucursal 1, precio 0) + registro en
       OPT_RegularizaProducto (0 -> cantidad).

   USO: ejecutar primero con @Confirmar = 0 (hace todo y hace ROLLBACK, muestra
   el resumen). Si el resumen está bien, cambiar a @Confirmar = 1 y re-ejecutar.
   Protección: aborta si ya existe una regularización con el mismo Motivo.
   ===================================================================== */
USE db_a25cfd_opt2;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Confirmar   bit           = 0;          -- 0 = prueba (ROLLBACK) | 1 = aplicar (COMMIT)
DECLARE @idSucursal  int           = 1;          -- Casa Matriz
DECLARE @Motivo      nvarchar(200) = N'Inventario armazones BRILAZ 2026-09-28';
DECLARE @Responsable nvarchar(100) = N'ALEXIS';
DECLARE @Ahora       datetime      = GETDATE();

IF EXISTS (SELECT 1 FROM dbo.OPT_RegularizaProducto WHERE Motivo = @Motivo)
BEGIN
    RAISERROR(N'Este inventario ya fue cargado (existe una regularización con el mismo Motivo). Se aborta.', 16, 1);
    RETURN;
END;

/* ---------------------------------------------------------------------
   1. Productos EXISTENTES (107)
   --------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Existentes') IS NOT NULL DROP TABLE #Existentes;
CREATE TABLE #Existentes (
    FilaExcel        varchar(20)   NOT NULL,
    idProducto       bigint        NOT NULL PRIMARY KEY,
    ProductoEsperado nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL,
    Cantidad         int           NOT NULL
);
INSERT INTO #Existentes (FilaExcel, idProducto, ProductoEsperado, Cantidad) VALUES
    ('7', 730, N'COOL-FLEX-CF-1-C1', 1),
    ('9', 1177, N'VESPA-VE-21035-F-C4', 1),
    ('22', 2544, N'GATO-FELIX-YS-26005-C3', 1),
    ('23', 323, N'GATO-FELIX-YS-26004-C3', 1),
    ('24', 1820, N'GATO-FELIX-GH-28259-C2', 1),
    ('25', 3202, N'JEAN-DE-PARIS-JP202004-C4', 1),
    ('26', 1323, N'FORMOSA-TC-3-C5', 1),
    ('28', 2245, N'LAURA-BAGIOTTI-JO-22006-C6', 1),
    ('29', 2872, N'WIZZ-WO-12-C4', 1),
    ('31', 540, N'WIZZ-WO-17-C5', 1),
    ('32', 2617, N'GAZAPO-MG51--C2', 1),
    ('34', 4883, N'GATTIZONI-G60164X-C16', 2),
    ('35', 2956, N'GATTIZONI-G60133X-C8', 1),
    ('36', 3852, N'GATTIZONI-G60157X-C5', 1),
    ('40', 4891, N'GATTIZONI-G60173X-C3', 1),
    ('85', 3161, N'LEGIRO-PZ22020-C2', 1),
    ('86', 3819, N'LEGIRO-PZ22016-C3', 1),
    ('88', 3148, N'LEGIRO-PZ22012-C3', 1),
    ('89', 2436, N'LEGIRO-HLW-202119-C5', 1),
    ('90', 3127, N'LEGIRO-PZ22004-C6', 2),
    ('92', 3149, N'LEGIRO-22012-C4', 1),
    ('93', 3156, N'LEGIRO-PZ22017-C1', 1),
    ('94', 3157, N'LEGIRO-PZ22018-C1', 1),
    ('95', 4263, N'LEGIRO-PZ22015-C4', 1),
    ('96', 2919, N'LEGIRO-PZ22002-C5', 1),
    ('97', 3164, N'LEGIRO-PZ22023-C1', 1),
    ('98', 3797, N'LEGIRO-PZ22017-C4', 1),
    ('99', 4266, N'LEGIRO-PZ22024-C3', 1),
    ('100', 3150, N'LEGIRO-PZ22012-C5', 1),
    ('101', 3154, N'LEGIRO-PZ22013-C5', 1),
    ('102', 3805, N'LEGIRO-PZ22002-C2', 1),
    ('103,121', 4259, N'LEGIRO-PZ22027-C5', 2),
    ('104', 4325, N'LEGIRO-PZ22015-C2', 1),
    ('105', 3179, N'LEGIRO-PZ22036-C5', 1),
    ('106', 2874, N'LEGIRO-PZ22003-C3', 1),
    ('107', 2871, N'LEGIRO-PZ22003-C6', 1),
    ('108', 3130, N'LEGIRO-PZ22005-C5', 1),
    ('111', 3166, N'LEGIRO-PZ22024-C4', 1),
    ('112', 3174, N'LEGIRO-PZ22034-C4', 1),
    ('113', 4318, N'LEGIRO-PZ22001-C4', 1),
    ('114', 4320, N'LEGIRO-PZ22003-C7', 1),
    ('115', 3175, N'LEGIRO-PZ22035-C1', 1),
    ('117', 3788, N'LEGIRO-PZ22025-C4', 1),
    ('118', 2938, N'LEGIRO-PZ22002-C3', 1),
    ('119', 4330, N'LEGIRO-PZ22021-C5', 1),
    ('122', 3169, N'LEGIRO-PZ22027-C7', 1),
    ('123', 4319, N'LEGIRO-PZ22001-C5', 1),
    ('133', 4604, N'LUCERNA-8820-C3', 1),
    ('134', 4606, N'LUCERNA-8814-C6', 1),
    ('135', 4713, N'LUCERNA-LH2024-C2', 1),
    ('140', 4600, N'LUCERNA-8827-C2', 1),
    ('141', 4601, N'LUCERNA-8826-C3', 1),
    ('142', 4597, N'LUCERNA-8826-C1', 1),
    ('145', 4625, N'CLARITY-95589-C11', 1),
    ('150', 4687, N'CLARITY-97685-C4', 1),
    ('152', 4644, N'CLARITY-95589-C3', 1),
    ('154', 4614, N'CLARITY-81059-C3', 1),
    ('155', 4691, N'CLARITY-81059-C5', 1),
    ('170', 3047, N'ANGELOFALCONI-DLAF02-C5', 1),
    ('173', 3403, N'ANGELO-DLAF26-C4', 1),
    ('182,184', 4519, N'ANGELO-FALCONI-PZAF061-C6', 2),
    ('188', 1493, N'ANGELO-FALCONI-DLAF-12-C6', 1),
    ('196', 3402, N'ANGELO-DLAF28-C3', 1),
    ('205', 3490, N'ANGELOFALCONI-PZAF064-C4', 1),
    ('207', 3423, N'ANGELO-DLAF09-C4', 1),
    ('211', 789, N'ANGELO-FALCONI-AF-057-C5', 1),
    ('214', 4102, N'ANGELO-FALCONI-PZAF068-C4', 1),
    ('219', 4517, N'ANEGELO-FALCONI-AF059-C5', 1),
    ('226', 4031, N'ANGELO-FALCONI-PZAF070-C1', 2),
    ('227', 2633, N'ANGELO-FALCONI-AF-055-C3', 1),
    ('228', 3929, N'ANGELO FALCONI-PZAF066-C2', 1),
    ('233', 2884, N'OCTAVIA-A16439-C5', 2),
    ('234', 4264, N'OCTAVIA-MW16061-C3', 1),
    ('235', 3439, N'OCTAVIA-DR20039-C4', 1),
    ('239', 825, N'OCTAVIA-A-17112-C7', 1),
    ('240', 2888, N'OCTAVIA-A16307-C3', 2),
    ('241', 190, N'OCTAVIA-A-17655-C6', 1),
    ('242,253', 2825, N'OCTAVIA-A16330-C4', 2),
    ('243', 3373, N'OCTAVIA-MW15064-C2', 2),
    ('244', 2915, N'OCTAVIA-A16254-C4', 1),
    ('245', 2823, N'OCTAVIA-A16257-C1', 1),
    ('246', 3378, N'OCTAVIA-A17644-C4', 2),
    ('247', 3379, N'OCTAVIA-ZL00809-C1', 1),
    ('248', 4461, N'OCTAVIA-MW-16039-C4', 1),
    ('249', 2821, N'OCTAVIA-A16307-C5', 1),
    ('252', 2812, N'OCTAVIA-A16040-C1', 1),
    ('254', 2897, N'MAXS-A17359-C4', 1),
    ('258', 3392, N'MAXS-MW16243-C2', 1),
    ('259', 2829, N'MAXS-A17656-C2', 1),
    ('260', 2763, N'MAXS-A17114-C5', 1),
    ('261', 1805, N'MAXS-A-17510-C3', 1),
    ('262', 689, N'MAXS-A-17627-C3', 1),
    ('263', 4251, N'MAXS-A17227-C4', 1),
    ('265', 3368, N'MAXS-A17115-C12', 1),
    ('266', 4267, N'MAXS-A17228-C1', 2),
    ('267', 2899, N'MAXS-A17636-C4', 1),
    ('269', 2833, N'MAXS-DR20071-C5', 1),
    ('271', 958, N'MAXS-A-17656-C3', 1),
    ('286', 2803, N'FOOSE-DS20061-C3', 1),
    ('289', 4287, N'FOOSE-GV1035-C6', 1),
    ('299', 4490, N'FRIDAKAKLO-FK1007M-BL', 1),
    ('307', 1465, N'BL-WD-3119-C2', 1),
    ('308', 534, N'BL-WD-1217-C2', 1),
    ('309', 2726, N'BL-MW-17234-C1', 1),
    ('311', 2138, N'BL-WD-3095-C7', 1),
    ('313', 836, N'BL-MW-17316-C3', 1),
    ('314', 2049, N'BL-MW-17013-C1', 1);

/* ---------------------------------------------------------------------
   2. Productos NUEVOS (199)
   --------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Nuevos') IS NOT NULL DROP TABLE #Nuevos;
CREATE TABLE #Nuevos (
    FilaExcel  varchar(20)   NOT NULL,
    Codigo     nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    Producto   nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL,
    Cantidad   int           NOT NULL
);
INSERT INTO #Nuevos (FilaExcel, Codigo, Producto, Cantidad) VALUES
    ('5', N'LD90302C4', N'DIFF-LD90302-C4', 1),
    ('6', N'F325BICOLOR', N'FRESH-F325-BICOLOR', 1),
    ('8', N'VE21029C1', N'VESPA-VE21029-C1', 1),
    ('10', N'91759C6', N'FANIA-91759-C6', 1),
    ('11', N'M1SILVERMATTE', N'DIN CILI-M-1-SILVER MATTE', 1),
    ('12', N'K2008C5', N'COOL-K2008-C5', 1),
    ('13', N'JH496CAFETRANSPARENTE', N'OS EYEWEAR-JH496-CAFE/TRANSPARENTE', 1),
    ('14', N'M25388C3', N'OS EYEWEAR-M25388-C3', 1),
    ('15', N'M25388C4', N'OS EYEWEAR-M25388-C4', 1),
    ('16', N'5008NEGROCAFE', N'OS EYEWEAR-5008-NEGRO/CAFE', 1),
    ('17', N'81115C4', N'OS EYEWEAR-81115-C4', 1),
    ('18', N'2112DORADO', N'FIRST-2112-DORADO', 1),
    ('19', N'YA14C5', N'YANGO-YA-14-C5', 1),
    ('20', N'YA16C5', N'YANGO-YA-16-C5', 1),
    ('21', N'XO4VERDE', N'XION-XO-4-VERDE', 1),
    ('27', N'JO24103C3', N'HARA BAGOTTI-JO24103-C3', 1),
    ('30', N'WO14C1', N'WRZZ-WO-14-C1', 1),
    ('33', N'G6036XC10', N'GATTIZONI-G6036X-C10', 1),
    ('37', N'G60161XC1', N'GATTIZONI-G60161X-C1', 1),
    ('38', N'G60160XC5', N'GATTIZONI-G60160X-C5', 1),
    ('39', N'G60178XC2', N'GATTIZONI-G60178X-C2', 1),
    ('41', N'G60177XC1', N'GATTIZONI-G60177X-C1', 1),
    ('42', N'G60088XC6', N'GATTIZONI-G60088X-C6', 1),
    ('43', N'G60159XC5', N'GATTIZONI-G60159X-C5', 1),
    ('44', N'G60187XC10', N'GATTIZONI-G60187X-C10', 1),
    ('45', N'G60151XC3', N'GATTIZONI-G60151X-C3', 1),
    ('46', N'G60161XC8', N'GATTIZONI-G60161X-C8', 1),
    ('47', N'G60197XC2', N'GATTIZONI-G60197X-C2', 1),
    ('48', N'G60199XC4', N'GATTIZONI-G60199X-C4', 1),
    ('49', N'LU23036C1', N'LUXOR-LU23036-C1', 1),
    ('50', N'LU23031C1', N'LUXOR-LU23031-C1', 1),
    ('51', N'LU23027C1', N'LUXOR-LU23027-C1', 1),
    ('52', N'SG167C1', N'VELTOM-SG167-C1', 1),
    ('53', N'MF7755C3', N'VELTOM-MF7755-C3', 1),
    ('54', N'MY21409C1', N'VELTOM-MY21409-C1', 1),
    ('55', N'MG6308AC4', N'VELTOM-MG6308A-C4', 1),
    ('56', N'XC840431C4', N'VELTOM-XC84043-1-C4', 1),
    ('57', N'EXTO031C2', N'JORGIO-EXTO031-C2', 2),
    ('58', N'DLTO066C2', N'JORGIO-DLTO066-C2', 1),
    ('59', N'EXTO032C5', N'JORGIO-EXTO032-C5', 1),
    ('60', N'EXTO010C7', N'JORGIO-EXTO010-C7', 1),
    ('61', N'EXTO027C3', N'JORGIO-EXTO027-C3', 1),
    ('62', N'EXTO023C2', N'JORGIO-EXTO023-C2', 1),
    ('63', N'EXTO032C3', N'JORGIO-EXTO032-C3', 1),
    ('64', N'EXTO067C4', N'JORGIO-EXTO067-C4', 1),
    ('65', N'EXTO001C3', N'JORGIO-EXTO001-C3', 1),
    ('66', N'DLTO040C3', N'JORGIO-DLTO040-C3', 1),
    ('67', N'EXTO033C3', N'JORGIO-EXTO033-C3', 2),
    ('68', N'EXTO030C3', N'JORGIO-EXTO030-C3', 2),
    ('69', N'EXTO025C6', N'JORGIO-EXTO025-C6', 1),
    ('70', N'EXTO021C1', N'JORGIO-EXTO021-C1', 2),
    ('71', N'EXTO033C6', N'JORGIO-EXTO033-C6', 1),
    ('72', N'DLTO015C1', N'JORGIO-DLTO015-C1', 1),
    ('73', N'DLTO040C1', N'JORGIO-DLTO040-C1', 1),
    ('74', N'EXTO034C5', N'JORGIO-EXTO034-C5', 1),
    ('75', N'EXTO029C1', N'JORGIO-EXTO029-C1', 1),
    ('76', N'DLTO042C1', N'JORGIO-DLTO042-C1', 1),
    ('77', N'EXTO022C11', N'JORGIO-EXTO022-C11', 1),
    ('78', N'EXTO022C13', N'JORGIO-EXTO022-C13', 1),
    ('79', N'EXTO025C5', N'JORGIO-EXTO025-C5', 1),
    ('80', N'EXTO037C7', N'JORGIO-EXTO037-C7', 1),
    ('81', N'DLTO054C1', N'JORGIO-DLTO054-C1', 1),
    ('82', N'DLTO051C3', N'JORGIO-DLTO051-C3', 1),
    ('83', N'DLTO016C4', N'JORGIO-DLTO016-C4', 1),
    ('84', N'PZ22030AC4', N'LEGIRO-PZ22030A-C4', 1),
    ('87', N'PZ22003C1', N'LEGIRO-PZ22003-C1', 1),
    ('91', N'PZ22046C5', N'LEGIRO-PZ22046-C5', 1),
    ('109', N'PZ22008C3', N'LEGIRO-PZ22008-C3', 1),
    ('110', N'PZ22028C6', N'LEGIRO-PZ22028-C6', 1),
    ('116', N'PZ22043C5', N'LEGIRO-PZ22043-C5', 1),
    ('120', N'PZ22029C5', N'LEGIRO-PZ22029-C5', 1),
    ('124', N'PZ22036C1', N'LEGIRO-PZ22036-C1', 1),
    ('125', N'EXN202015FC7', N'LINEA VIGO-EXN202015F-C7', 1),
    ('126', N'EXN21130FC4', N'LINEA VIGO-EXN21130F-C4', 2),
    ('127', N'EXN21130FC5', N'LINEA VIGO-EXN21130F-C5', 2),
    ('128', N'DS202012C1', N'LINEA VIGO-DS202012-C1', 1),
    ('129', N'LH2024C3', N'LUCERNA-LH2024-C3', 1),
    ('130', N'8824C5', N'LUCERNA-8824-C5', 1),
    ('131', N'LH3007C5', N'LUCERNA-LH3007-C5', 1),
    ('132', N'LH3007C2', N'LUCERNA-LH3007-C2', 2),
    ('136', N'LH3004C4', N'LUCERNA-LH3004-C4', 1),
    ('137', N'LO387CELESTETRANSPARENTE', N'LUCERNA-LO387-CELESTE/TRANSPARENTE', 1),
    ('138', N'LO387VERDETRANSPARENTE', N'LUCERNA-LO387-VERDE/TRANSPARENTE', 2),
    ('139', N'8813C6', N'LUCERNA-8813-C6', 1),
    ('143', N'95589C12', N'CLARITY-95589-C12', 1),
    ('144', N'95589C8', N'CLARITY-95589-C8', 1),
    ('146', N'95589C10', N'CLARITY-95589-C10', 1),
    ('147', N'96021C3', N'CLARITY-96021-C3', 1),
    ('148', N'95770C10', N'CLARITY-95770-C10', 1),
    ('149', N'95296C6', N'CLARITY-95296-C6', 1),
    ('151', N'81059C4', N'CLARITY-81059-C4', 1),
    ('153', N'95970C6', N'CLARITY-95970-C6', 1),
    ('156', N'95556C13', N'CLARITY-95556-C13', 1),
    ('157', N'81525C6', N'CLARITY-81525-C6', 1),
    ('158', N'82142C8', N'CLARITY-82142-C8', 1),
    ('159', N'82142C7', N'CLARITY-82142-C7', 1),
    ('160', N'82033C1', N'CLARITY-82033-C1', 1),
    ('161', N'81053C6', N'CLARITY-81053-C6', 1),
    ('162', N'Z3337TRANSPARENTENEGRO', N'ROLETT-Z3337-TRANSPARENTE/NEGRO', 1),
    ('163', N'ROLETTSCNEGROTRANSPARENTE', N'ROLETT-S/C-NEGRO/TRANSPARENTE', 1),
    ('164', N'ROLETTSCTRANSPARENTE', N'ROLETT-S/C-TRANSPARENTE', 1),
    ('165', N'ZN3518NEGROCAFE', N'ROLETT-ZN3518-NEGRO/CAFE', 1),
    ('166', N'ZN3518BICOLOR', N'ROLETT-ZN3518-BICOLOR', 1),
    ('167', N'LH2120C1', N'ROLETT-LH2120-C1', 1),
    ('168', N'LD2485C4', N'ROLETT-LD2485-C4', 1),
    ('169', N'ANAF2239C1', N'ANGELO FALCONI-ANAF2239-C1', 1),
    ('171', N'ANAF2231C3', N'ANGELO FALCONI-ANAF2231-C3', 1),
    ('172', N'AF2233C2', N'ANGELO FALCONI-AF2233-C2', 1),
    ('174', N'ANAF2260C1', N'ANGELO FALCONI-ANAF2260-C1', 1),
    ('175', N'EXAF037XC4', N'ANGELO FALCONI-EX-AF037X-C4', 1),
    ('176', N'EXAF031XC4', N'ANGELO FALCONI-EX-AF031X-C4', 1),
    ('177', N'EXN20321C2', N'ANGELO FALCONI-EXN20321-C2', 1),
    ('178', N'EXN27314C3A', N'ANGELO FALCONI-EXN27314-C3A', 1),
    ('179', N'EXN20314C6', N'ANGELO FALCONI-EXN20314-C6', 1),
    ('180', N'EXN27314C1', N'ANGELO FALCONI-EXN27314-C1', 1),
    ('181,209', N'EXN20326C1', N'ANGELO FALCONI-EXN20326-C1', 3),
    ('183', N'EXN20338C11', N'ANGELO FALCONI-EXN20338-C11', 1),
    ('185', N'PZAF063C5', N'ANGELO FALCONI-PZAF063-C5', 1),
    ('186', N'EXN20350C2', N'ANGELO FALCONI-EXN20350-C2', 1),
    ('187', N'EXN20348C1', N'ANGELO FALCONI-EXN20348-C1', 1),
    ('189', N'ANAF2258C4', N'ANGELO FALCONI-ANAF2258-C4', 1),
    ('190', N'EXAF2251C6', N'ANGELO FALCONI-EXAF2251-C6', 1),
    ('191', N'ANAF2242C1', N'ANGELO FALCONI-ANAF2242-C1', 1),
    ('192', N'ANAF2261C2', N'ANGELO FALCONI-ANAF2261-C2', 1),
    ('193', N'DLAF20C1', N'ANGELO FALCONI-DLAF20-C1', 1),
    ('194', N'ANAF2245C1', N'ANGELO FALCONI-ANAF2245-C1', 1),
    ('195', N'ANAF2259C3', N'ANGELO FALCONI-ANAF2259-C3', 1),
    ('197', N'EXN20346C3', N'ANGELO FALCONI-EXN20346-C3', 1),
    ('198', N'EXAF09XC3', N'ANGELO FALCONI-EX-AF09X-C3', 1),
    ('199', N'AF058C5', N'ANGELO FALCONI-AF058-C5', 1),
    ('200', N'AF060C4', N'ANGELO FALCONI-AF060-C4', 1),
    ('201', N'EXN20345C3', N'ANGELO FALCONI-EXN20345-C3', 1),
    ('202', N'EXN20349C3', N'ANGELO FALCONI-EXN20349-C3', 1),
    ('203', N'AF058C3', N'ANGELO FALCONI-AF058-C3', 1),
    ('204', N'EXN24315AC6', N'ANGELO FALCONI-EXN24315A-C6', 1),
    ('206', N'DLAF24C3', N'ANGELO FALCONI-DLAF24-C3', 1),
    ('208', N'EXN26315C3', N'ANGELO FALCONI-EXN26315-C3', 1),
    ('210', N'EXN20329C5', N'ANGELO FALCONI-EXN20329-C5', 1),
    ('212', N'EXN20325C3', N'ANGELO FALCONI-EXN20325-C3', 1),
    ('213', N'EXAF08XC2', N'ANGELO FALCONI-EX-AF08X-C2', 1),
    ('215', N'EXAF042XC2', N'ANGELO FALCONI-EX-AF042X-C2', 1),
    ('216', N'DLAF27C4', N'ANGELO FALCONI-DLAF27-C4', 1),
    ('217', N'EXAF048XC4', N'ANGELO FALCONI-EX-AF048X-C4', 1),
    ('218', N'EXN20325C4', N'ANGELO FALCONI-EXN20325-C4', 1),
    ('220', N'EXN20338C2', N'ANGELO FALCONI-EXN20338-C2', 1),
    ('221', N'AF059C3', N'ANGELO FALCONI-AF059-C3', 1),
    ('222', N'EXAF042XC5', N'ANGELO FALCONI-EX-AF042X-C5', 1),
    ('223', N'EXN20338C3', N'ANGELO FALCONI-EXN20338-C3', 1),
    ('224', N'EXN20326C7A', N'ANGELO FALCONI-EXN20326-C7A', 1),
    ('225', N'EXN20340C1', N'ANGELO FALCONI-EXN20340-C1', 1),
    ('229', N'AF060C6', N'ANGELO FALCONI-AF060-C6', 1),
    ('230', N'EXN20342C3', N'ANGELO FALCONI-EXN20342-C3', 1),
    ('231', N'ANAF2256C2', N'ANGELO FALCONI-ANAF2256-C2', 1),
    ('232', N'ANAF2264C1', N'ANGELO FALCONI-ANAF2264-C1', 1),
    ('236', N'A17332C1', N'OCTAVIA-A17332-C1', 1),
    ('237', N'A15187C4', N'OCTAVIA-A15187-C4', 1),
    ('238', N'A15136C3', N'OCTAVIA-A15136-C3', 1),
    ('250', N'A15136C5', N'OCTAVIA-A15136-C5', 2),
    ('251', N'MW16411C2', N'OCTAVIA-MW16411-C2', 1),
    ('255', N'A17333C4', N'MAXS-A17333-C4', 2),
    ('256', N'MW16169C2', N'MAXS-MW16169-C2', 1),
    ('257', N'MW17161C2', N'MAXS-MW17161-C2', 1),
    ('264', N'DR20071C2', N'MAXS-DR20071-C2', 1),
    ('268', N'MW17253C2', N'MAXS-MW17253-C2', 2),
    ('270', N'A17143C2', N'MAXS-A17143-C2', 1),
    ('272', N'GE3253XC7', N'GEORGE-GE3253X-C7', 1),
    ('273', N'GE3463XC3', N'GEORGE-GE3463X-C3', 1),
    ('274', N'GE3556XC4', N'GEORGE-GE3556X-C4', 1),
    ('275', N'GE3516XC1', N'GEORGE-GE3516X-C1', 1),
    ('276', N'GE3559XC7', N'GEORGE-GE3559X-C7', 1),
    ('277', N'GE3511XC2', N'GEORGE-GE3511X-C2', 1),
    ('278', N'GE3445XC3A', N'GEORGE-GE3445X-C3A', 1),
    ('279', N'GE3511XC1', N'GEORGE-GE3511X-C1', 1),
    ('280', N'GE3440XC7', N'GEORGE-GE3440X-C7', 1),
    ('281,282', N'GE3443XC5', N'GEORGE-GE3443X-C5', 2),
    ('283', N'TI866C3', N'FOOSE-TI866-C3', 1),
    ('284', N'XYFO106C5', N'FOOSE-XYFO106-C5', 1),
    ('285', N'XYFO104C1', N'FOOSE-XYFO104-C1', 1),
    ('287', N'XYFO101C3', N'FOOSE-XYFO101-C3', 1),
    ('288', N'GV1036C1', N'FOOSE-GV1036-C1', 1),
    ('290', N'XYFO065C3', N'FOOSE-XYFO065-C3', 1),
    ('291', N'XYFO018C2', N'FOOSE-XYFO018-C2', 1),
    ('292', N'EXFO116C2', N'FOOSE-EXFO116-C2', 1),
    ('293', N'GV1010C5', N'FOOSE-GV1010-C5', 1),
    ('294', N'TS3012C3', N'BETEVUE-TS3012-C3', 1),
    ('295', N'FK1012RRO', N'FRIDA KAHLO-FK1012R-RO', 1),
    ('296', N'FK1004HBL', N'FRIDA KAHLO-FK1004H-BL', 1),
    ('297', N'FK1016MPI', N'FRIDA KAHLO-FK1016M-PI', 1),
    ('298', N'FK1020MPR', N'FRIDA KAHLO-FK1020M-PR', 1),
    ('300', N'PL1912C2', N'POLO-PL1912-C2', 1),
    ('301', N'PL2155C1', N'POLO-PL2155-C1', 1),
    ('302', N'PL2165C4', N'POLO-PL2165-C4', 1),
    ('303', N'PL2161C1', N'POLO-PL2161-C1', 1),
    ('304', N'GU50059052', N'GUESS-GU50059-052', 1),
    ('305', N'SP5006091', N'ADIDAS-SP5006-091', 1),
    ('306', N'KP3172M232', N'KIPLING-KP3172-M232', 1),
    ('310', N'MW20096C1', N'BRILAZ-MW20096-C1', 1),
    ('312', N'MW17014C1', N'BRILAZ-MW17014-C1', 2),
    ('315', N'MW16154C1', N'BRILAZ-MW16154-C1', 1);

/* ---------------------------------------------------------------------
   3. Validaciones previas (no escriben nada)
   --------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM #Existentes e
           LEFT JOIN dbo.OPT_Producto p ON p.idProducto = e.idProducto
           WHERE p.idProducto IS NULL OR p.Producto <> e.ProductoEsperado)
BEGIN
    SELECT 'ERROR: producto existente cambió o no existe' AS Problema, e.*, p.Producto AS ProductoActual
    FROM #Existentes e LEFT JOIN dbo.OPT_Producto p ON p.idProducto = e.idProducto
    WHERE p.idProducto IS NULL OR p.Producto <> e.ProductoEsperado;
    RAISERROR(N'Hay productos existentes que ya no coinciden con el análisis. Se aborta.', 16, 1);
    RETURN;
END;

IF EXISTS (SELECT 1 FROM #Nuevos n JOIN dbo.OPT_Producto p ON p.Codigo = n.Codigo)
BEGIN
    SELECT 'ERROR: el código nuevo ya existe' AS Problema, n.*, p.idProducto, p.Producto AS ProductoActual
    FROM #Nuevos n JOIN dbo.OPT_Producto p ON p.Codigo = n.Codigo;
    RAISERROR(N'Hay códigos nuevos que ya existen en OPT_Producto. Se aborta.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.OPT_Sucursal WHERE idSucursal = @idSucursal)
BEGIN
    RAISERROR(N'La sucursal indicada no existe. Se aborta.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------
   4. EXISTENTES: fijar stock en la sucursal (con registro de regularización)
   --------------------------------------------------------------------- */
-- 4.1 Si algún producto no tiene fila en la sucursal, se crea en 0
INSERT INTO dbo.OPT_ProductoSucursal (idProducto, idSucursal, Entrada, Salida, Stock, StockMinimo, StockMaximo, PrecioVenta)
SELECT e.idProducto, @idSucursal, 0, 0, 0, 0, 0, 0
FROM #Existentes e
WHERE NOT EXISTS (SELECT 1 FROM dbo.OPT_ProductoSucursal ps
                  WHERE ps.idProducto = e.idProducto AND ps.idSucursal = @idSucursal);

-- 4.2 Registro histórico (StockAnterior -> StockNuevo)
INSERT INTO dbo.OPT_RegularizaProducto (idProducto, StockAnterior, StockNuevo, Fecha, Responsable, Motivo, idSucursal, PrecioAnterior, PrecioNuevo)
SELECT e.idProducto, ps.Stock, e.Cantidad, @Ahora, @Responsable, @Motivo, @idSucursal, ps.PrecioVenta, ps.PrecioVenta
FROM #Existentes e
JOIN dbo.OPT_ProductoSucursal ps ON ps.idProducto = e.idProducto AND ps.idSucursal = @idSucursal;

-- 4.3 Nuevo stock
UPDATE ps
SET    ps.Stock = e.Cantidad
FROM   dbo.OPT_ProductoSucursal ps
JOIN   #Existentes e ON e.idProducto = ps.idProducto
WHERE  ps.idSucursal = @idSucursal;

/* ---------------------------------------------------------------------
   5. NUEVOS: insertar producto + stock por sucursal + registro
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
   6. Resumen / verificación
   --------------------------------------------------------------------- */
SELECT 'Existentes actualizados' AS Tipo, COUNT(*) AS Productos, SUM(e.Cantidad) AS Unidades
FROM #Existentes e
JOIN dbo.OPT_ProductoSucursal ps ON ps.idProducto = e.idProducto AND ps.idSucursal = @idSucursal AND ps.Stock = e.Cantidad
UNION ALL
SELECT 'Nuevos insertados', COUNT(*), SUM(n.Cantidad)
FROM @Insertados i JOIN #Nuevos n ON n.Codigo = i.Codigo
JOIN dbo.OPT_ProductoSucursal ps ON ps.idProducto = i.idProducto AND ps.idSucursal = @idSucursal AND ps.Stock = n.Cantidad
UNION ALL
SELECT 'Regularizaciones registradas', COUNT(*), SUM(StockNuevo)
FROM dbo.OPT_RegularizaProducto WHERE Motivo = @Motivo;
-- Esperado: Existentes 107 / 118 u. · Nuevos 199 / 214 u. · Regularizaciones 306 / 332 u.

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'Inventario BRILAZ aplicado (COMMIT).';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'Modo prueba: se hizo ROLLBACK. Cambie @Confirmar = 1 para aplicar.';
END;
GO
