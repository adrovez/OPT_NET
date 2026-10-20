/* =====================================================================
   Carga de inventario de armazones BRILAZ  ->  db_a25cfd_opt2 (LEGACY / produccion)
   Origen : doc_Cliente/Inventario/Inventario01.xlsx (hoja Detalle: 305 filas, 329 unidades)
            Productos existentes tomados de ProductoProduccion.xlsx (4.874 productos).
            Inventario02.xlsx es la misma toma transcrita por segunda vez: NO se suma,
            solo se uso para contrastar (ver Revision_Carga_Inventario.xlsx).
   Generado: 2026-10-08

   Resultado del analisis (filas del Excel con mismo Modelo+Color se suman):
     - 167 productos EXISTEN  (184 u.)  -> se fija el stock (regularizacion)
     - 136 productos NO existen (145 u.) -> se insertan
   Criterio de coincidencia: la descripcion del producto (OPT_Producto.Producto) sin
   separadores termina en MODELO+COLOR del Excel (misma marca), o Codigo = MODELO+COLOR.
   Con varias coincidencias se prefiere ControlStock = 1 y luego el mayor idProducto.
   Unica correccion de lectura aplicada: Jorgio FXTO#### -> FXJO#### (error de transcripcion).
   Los demas codigos se toman tal cual vienen en el Excel.

   Reglas (heredadas del script del 28-sep, decididas con el usuario):
     - Sucursal: 1 (Casa Matriz)
     - Existentes: Stock = cantidad del Excel (conteo fisico). Queda registro en
       OPT_RegularizaProducto (StockAnterior -> StockNuevo), igual que la pantalla
       "Regularizar". Entrada/Salida/PrecioVenta/ControlStock NO se tocan.
     - Nuevos: OPT_Producto (ControlStock = 1) + una fila de OPT_ProductoSucursal por cada
       sucursal (stock solo en la sucursal 1, precio 0) + registro en OPT_RegularizaProducto.

   USO: ejecutar primero con @Confirmar = 0 (hace todo y hace ROLLBACK, muestra el resumen).
   Si el resumen coincide con lo esperado, cambiar a @Confirmar = 1 y re-ejecutar.
   Proteccion: aborta si ya existe una regularizacion con el mismo Motivo, si un producto
   existente cambio de nombre/no existe, o si algun codigo nuevo ya existe.
   ===================================================================== */
USE db_a25cfd_opt2;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Confirmar   bit           = 0;          -- 0 = prueba (ROLLBACK) | 1 = aplicar (COMMIT)
DECLARE @idSucursal  int           = 1;          -- Casa Matriz
DECLARE @Motivo      nvarchar(200) = N'Inventario armazones BRILAZ 2026-10-08';
DECLARE @Responsable nvarchar(100) = N'ALEXIS';
DECLARE @Ahora       datetime      = GETDATE();

IF EXISTS (SELECT 1 FROM dbo.OPT_RegularizaProducto WHERE Motivo = @Motivo)
BEGIN
    RAISERROR(N'Este inventario ya fue cargado (existe una regularizacion con el mismo Motivo). Se aborta.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.OPT_Sucursal WHERE idSucursal = @idSucursal)
BEGIN
    RAISERROR(N'La sucursal indicada no existe. Se aborta.', 16, 1);
    RETURN;
END;

/* ---------------------------------------------------------------------
   1. Productos EXISTENTES (167)
   --------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Existentes') IS NOT NULL DROP TABLE #Existentes;
CREATE TABLE #Existentes (
    FilaExcel        varchar(40)   NOT NULL,
    idProducto       bigint        NOT NULL PRIMARY KEY,
    ProductoEsperado nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL,
    Cantidad         int           NOT NULL
);
INSERT INTO #Existentes (FilaExcel, idProducto, ProductoEsperado, Cantidad) VALUES
    ('8', 3047, N'ANGELOFALCONI-DLAF02-C5', 1),
    ('11', 3403, N'ANGELO-DLAF26-C4', 1),
    ('13', 739, N'ANGELO-FALCONI--FXAF-037X-C4', 1),
    ('14', 197, N'ANGELO-FALCONI--FXAF-031X-C4', 1),
    ('15', 3083, N'ANGELOFALCONI-FXN20321-C2', 1),
    ('17', 3091, N'ANGELOFALCONI-FXN20340-C6', 1),
    ('19', 2307, N'ANGELO-FALCONI-FXN-20326-C7', 1),
    ('20,22', 4519, N'ANGELO-FALCONI-PZAF061-C6', 2),
    ('21', 3309, N'ANGELO-FALCONI-FXN20338-C4', 1),
    ('24', 4929, N'ANGELO FALCONI-FXN20350-C2', 1),
    ('26', 1493, N'ANGELO-FALCONI-DLAF-12-C6', 1),
    ('31', 3059, N'ANGELOFALCONI-DLAF10-C1', 1),
    ('32', 4420, N'ANGELOFALCONI-AMAF2245-C1', 1),
    ('34', 3402, N'ANGELO-DLAF28-C3', 1),
    ('35', 3579, N'ANGELO-FALCONI-FXN20346-C3', 1),
    ('36', 4098, N'ANGELO-FALCONI-FXAF051X-C3', 1),
    ('43', 3490, N'ANGELOFALCONI-PZAF064-C4', 1),
    ('45', 3423, N'ANGELO-DLAF09-C4', 1),
    ('46', 2228, N'ANGELO-FALCONI-FXN-29315-C3', 1),
    ('47', 4051, N'ANGELO-FALCONI-FXN20326-C1', 2),
    ('48', 1160, N'ANGELO-FALCONI-FXN-20329-C5', 1),
    ('49', 789, N'ANGELO-FALCONI-AF-057-C5', 1),
    ('50', 3488, N'ANGELOFALCONI-FXN20325-C3', 1),
    ('51', 1198, N'ANGELO-FALCONI--FXAF-037X-C2', 1),
    ('52', 4102, N'ANGELO-FALCONI-PZAF068-C4', 1),
    ('53', 3076, N'ANGELO-FALCONI-FXAF042X-C2', 1),
    ('55', 3080, N'ANGELOFALCONI-FXAF048X-C4', 1),
    ('56', 3950, N'ANGELO-FALCONI-FXN20325-C4', 1),
    ('57', 4517, N'ANEGELO-FALCONI-AF059-C5', 1),
    ('58', 3412, N'ANGELO-FXN20338-C2', 1),
    ('60', 1816, N'ANGELO-FALCONI--FXAF-042X-C5', 1),
    ('61', 3619, N'ANGELO-FALCONI-FXN20338-C3', 1),
    ('63', 3626, N'ANGELO-FALCONI-FXN20340-C1', 1),
    ('64', 4031, N'ANGELO-FALCONI-PZAF070-C1', 2),
    ('65', 636, N'ANGELO-FALCONI-AF-049-C3', 1),
    ('66', 3929, N'ANGELO FALCONI-PZAF066-C2', 1),
    ('68', 4141, N'ANGELOF-FXN20342-C3', 1),
    ('71', 1465, N'BL-WD-3119-C2', 1),
    ('72', 534, N'BL-WD-1217-C2', 1),
    ('73', 2726, N'BL-MW-17234-C1', 1),
    ('74', 1984, N'BL-MW-20006-C1', 1),
    ('76', 509, N'BL-MW-17054-C1', 2),
    ('77', 836, N'BL-MW-17316-C3', 1),
    ('78', 2049, N'BL-MW-17013-C1', 1),
    ('82', 4625, N'CLARITY-95589-C11', 1),
    ('89', 4644, N'CLARITY-95589-C3', 1),
    ('91', 4614, N'CLARITY-81059-C3', 1),
    ('92', 4691, N'CLARITY-81059-C5', 1),
    ('106', 4434, N'FOOSE-T1866-C3', 1),
    ('109', 2803, N'FOOSE-DS20061-C3', 1),
    ('112', 4287, N'FOOSE-GV1035-C6', 1),
    ('117', 1323, N'FORMOSA-TC-3-C5', 1),
    ('123', 4490, N'FRIDAKAKLO-FK1007M-BL', 1),
    ('124', 2544, N'GATO-FELIX-YS-26005-C3', 1),
    ('125', 323, N'GATO-FELIX-YS-26004-C3', 1),
    ('127', 4255, N'GATTIZONI-G60036X-C10', 1),
    ('128', 4883, N'GATTIZONI-G60164X-C16', 2),
    ('129', 2956, N'GATTIZONI-G60133X-C8', 1),
    ('134', 4891, N'GATTIZONI-G60173X-C3', 1),
    ('135', 4893, N'GATTIZONI-G60174X-C1', 1),
    ('136', 4871, N'GATTIZONI-G60058X-C6', 1),
    ('137', 4876, N'GATTIZONI-G60130X-C5', 1),
    ('138', 4886, N'GATTIZONI-G60169X-C16', 1),
    ('139', 3850, N'GATTIZONI-G60157X-C3', 1),
    ('141', 3849, N'GATTIZONI-G60157X-C2', 1),
    ('142', 4879, N'GATTIZONI-G60149X-C4', 1),
    ('144', 3503, N'GEORGE-GE3538X-C7', 1),
    ('146', 2953, N'GEORGE-GE3538X-C4', 1),
    ('148', 1973, N'GEORGE-GE-3531-X-C1', 1),
    ('152', 520, N'GEORGE-GE-3543-X-C5', 1),
    ('154', 280, N'JEAN-DE-PARIS-JP202001-C4', 1),
    ('155', 3117, N'JORGIO-FXJO031-C2', 2),
    ('156', 4806, N'JORGIO-DLJO066-C2', 1),
    ('158', 3511, N'JORGIO-FXJO010-C7', 1),
    ('159', 4819, N'JORGIO-FXJO029-C3', 1),
    ('160', 4797, N'JORGIO-FXJO067-C4', 1),
    ('161', 4809, N'JORGIO-FXJO001-C3', 1),
    ('162', 4833, N'JORGIO-DLJO040-C3', 1),
    ('163', 3521, N'JORGIO-FXJO038-C3', 2),
    ('165', 2966, N'JORGIO-FXJO025-C6', 1),
    ('166', 3777, N'JORGIO-FXJO021-C1', 2),
    ('167', 4315, N'JORGIO-FXJO038-C6', 1),
    ('168', 2957, N'JORGIO-DLJO015-C1', 1),
    ('169', 4831, N'JORGIO-DLJO040-C1', 1),
    ('170', 4795, N'JORGIO-FXJO039-C5', 1),
    ('171', 4818, N'JORGIO-FXJO029-C1', 1),
    ('172', 3893, N'JORGIO-DLJO042-C1', 1),
    ('173', 4813, N'JORGIO-FXJO022-C11', 1),
    ('175', 696, N'JORGIO-FXJO-023-C5', 1),
    ('176', 4313, N'JORGIO-FXJO037-C2', 1),
    ('177', 4219, N'JORGIO-DLJO051-C1', 1),
    ('179', 3506, N'JORGIO-DLJO016-C4', 1),
    ('184', 3161, N'LEGIRO-PZ22020-C2', 1),
    ('185', 3819, N'LEGIRO-PZ22016-C3', 1),
    ('186', 3148, N'LEGIRO-PZ22012-C3', 1),
    ('188', 3127, N'LEGIRO-PZ22004-C6', 2),
    ('189', 3821, N'LEGIRO-PZ22016-C5', 1),
    ('190', 3149, N'LEGIRO-22012-C4', 1),
    ('191', 3156, N'LEGIRO-PZ22017-C1', 1),
    ('192', 3157, N'LEGIRO-PZ22018-C1', 1),
    ('193', 4263, N'LEGIRO-PZ22015-C4', 1),
    ('194', 2919, N'LEGIRO-PZ22002-C5', 1),
    ('195', 3164, N'LEGIRO-PZ22023-C1', 1),
    ('196', 3797, N'LEGIRO-PZ22017-C4', 1),
    ('197', 4266, N'LEGIRO-PZ22024-C3', 1),
    ('198', 3150, N'LEGIRO-PZ22012-C5', 1),
    ('199', 3154, N'LEGIRO-PZ22013-C5', 1),
    ('200', 3805, N'LEGIRO-PZ22002-C2', 1),
    ('201', 4259, N'LEGIRO-PZ22027-C5', 1),
    ('202', 4325, N'LEGIRO-PZ22015-C2', 1),
    ('203', 3179, N'LEGIRO-PZ22036-C5', 1),
    ('204', 2874, N'LEGIRO-PZ22003-C3', 1),
    ('205', 2871, N'LEGIRO-PZ22003-C6', 1),
    ('206', 3130, N'LEGIRO-PZ22005-C5', 1),
    ('207', 3555, N'LEGIRO-PZ22005-C3', 1),
    ('209', 3166, N'LEGIRO-PZ22024-C4', 1),
    ('210', 3174, N'LEGIRO-PZ22034-C4', 1),
    ('211', 4318, N'LEGIRO-PZ22001-C4', 1),
    ('212', 4320, N'LEGIRO-PZ22003-C7', 1),
    ('213', 3175, N'LEGIRO-PZ22035-C1', 1),
    ('215', 3788, N'LEGIRO-PZ22025-C4', 1),
    ('216', 2938, N'LEGIRO-PZ22002-C3', 1),
    ('217', 4330, N'LEGIRO-PZ22021-C5', 1),
    ('219', 3218, N'LEGIRO-PZ22037-C5', 1),
    ('220', 3169, N'LEGIRO-PZ22027-C7', 1),
    ('221', 4319, N'LEGIRO-PZ22001-C5', 1),
    ('224', 3537, N'VIGO-FXN21130F-C4', 2),
    ('225', 3538, N'VIGO-FXN21130F-C5', 2),
    ('231', 4604, N'LUCERNA-8820-C3', 1),
    ('232', 4606, N'LUCERNA-8814-C6', 1),
    ('233', 4713, N'LUCERNA-LH2024-C2', 1),
    ('237', 4600, N'LUCERNA-8827-C2', 1),
    ('238', 4601, N'LUCERNA-8826-C3', 1),
    ('239', 4597, N'LUCERNA-8826-C1', 1),
    ('243', 2897, N'MAXS-A17359-C4', 1),
    ('247', 3392, N'MAXS-MW16243-C2', 1),
    ('248', 2829, N'MAXS-A17656-C2', 1),
    ('249', 2763, N'MAXS-A17114-C5', 1),
    ('250', 1805, N'MAXS-A-17510-C3', 1),
    ('251', 689, N'MAXS-A-17627-C3', 1),
    ('252', 4251, N'MAXS-A17227-C4', 1),
    ('253', 3704, N'MAXS-DR20027-C2', 1),
    ('254', 3368, N'MAXS-A17115-C12', 1),
    ('255', 4267, N'MAXS-A17228-C1', 2),
    ('256', 2899, N'MAXS-A17636-C4', 1),
    ('258', 2833, N'MAXS-DR20071-C5', 1),
    ('259', 521, N'MAXS-A17113-C2', 1),
    ('261', 2884, N'OCTAVIA-A16439-C5', 2),
    ('262', 4264, N'OCTAVIA-MW16061-C3', 1),
    ('263', 3439, N'OCTAVIA-DR20039-C4', 1),
    ('264', 784, N'OCTAVIA-A-17333-C1', 1),
    ('265', 3570, N'OCTAVIA-A15782-C4', 1),
    ('266', 2753, N'OCTAVIA-A-15736-C3', 1),
    ('267', 825, N'OCTAVIA-A-17112-C7', 1),
    ('268', 2888, N'OCTAVIA-A16307-C3', 2),
    ('269', 190, N'OCTAVIA-A-17655-C6', 1),
    ('270,281', 2825, N'OCTAVIA-A16330-C4', 2),
    ('272', 2915, N'OCTAVIA-A16254-C4', 1),
    ('273', 2823, N'OCTAVIA-A16257-C1', 1),
    ('274', 3378, N'OCTAVIA-A17644-C4', 2),
    ('275', 3379, N'OCTAVIA-ZL00809-C1', 1),
    ('277', 2821, N'OCTAVIA-A16307-C5', 1),
    ('278', 4463, N'OCTAVIA-A15736-C5', 2),
    ('280', 2812, N'OCTAVIA-A16040-C1', 1),
    ('297', 4611, N'ROLETT-LD2435-C4', 1),
    ('305', 2872, N'WIZZ-WO-12-C4', 1),
    ('307', 540, N'WIZZ-WO-17-C5', 1);

/* ---------------------------------------------------------------------
   2. Productos NUEVOS (136)
   --------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Nuevos') IS NOT NULL DROP TABLE #Nuevos;
CREATE TABLE #Nuevos (
    FilaExcel  varchar(40)   NOT NULL,
    Codigo     nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    Producto   nvarchar(50)  COLLATE DATABASE_DEFAULT NOT NULL,
    Cantidad   int           NOT NULL
);
INSERT INTO #Nuevos (FilaExcel, Codigo, Producto, Cantidad) VALUES
    ('6', N'SP3006091', N'ADIDAS-SP3006-091', 1),
    ('7', N'AMAF2259C1', N'ANGELO FALCONI-AMAF2259-C1', 1),
    ('9', N'KDAF2237C3', N'ANGELO FALCONI-KDAF2237-C3', 1),
    ('10', N'AF2233C2', N'ANGELO FALCONI-AF2233-C2', 1),
    ('12', N'AMAF2260C1', N'ANGELO FALCONI-AMAF2260-C1', 1),
    ('16', N'FXN20314C3A', N'ANGELO FALCONI-FXN20314-C3A', 1),
    ('18', N'FXN20314AC1', N'ANGELO FALCONI-FXN20314A-C1', 1),
    ('23', N'PZAF063C5', N'ANGELO FALCONI-PZAF063-C5', 1),
    ('25', N'AMAF2243C1', N'ANGELO FALCONI-AMAF2243-C1', 1),
    ('27', N'AMAF2258C4', N'ANGELO FALCONI-AMAF2258-C4', 1),
    ('28', N'FXAF2251C6', N'ANGELO FALCONI-FXAF2251-C6', 1),
    ('29', N'AMAF2242C1', N'ANGELO FALCONI-AMAF2242-C1', 1),
    ('30', N'AMAF2261C2', N'ANGELO FALCONI-AMAF2261-C2', 1),
    ('33', N'AMAF2259C3', N'ANGELO FALCONI-AMAF2259-C3', 1),
    ('37', N'AF058C5', N'ANGELO FALCONI-AF058-C5', 1),
    ('38', N'AF060C4', N'ANGELO FALCONI-AF060-C4', 1),
    ('39', N'FXN20345C3', N'ANGELO FALCONI-FXN20345-C3', 1),
    ('40', N'FXN20349C3', N'ANGELO FALCONI-FXN20349-C3', 1),
    ('41', N'AF058C3', N'ANGELO FALCONI-AF058-C3', 1),
    ('42', N'FXN29314AC6', N'ANGELO FALCONI-FXN29314A-C6', 1),
    ('44', N'DLAF24C3', N'ANGELO FALCONI-DLAF24-C3', 1),
    ('54', N'DLAF27C4', N'ANGELO FALCONI-DLAF27-C4', 1),
    ('59', N'AF059C3', N'ANGELO FALCONI-AF059-C3', 1),
    ('62', N'FXN20326C7A', N'ANGELO FALCONI-FXN20326-C7A', 1),
    ('67', N'AF060C6', N'ANGELO FALCONI-AF060-C6', 1),
    ('69', N'AMAF2236C2', N'ANGELO FALCONI-AMAF2236-C2', 1),
    ('70', N'IS3012C3', N'BELLEVE-IS3012-C3', 1),
    ('75', N'WD3015C7', N'BL-WD3015-C7', 1),
    ('79', N'MW16154C1', N'BL-MW16154-C1', 1),
    ('80', N'95589C12', N'CLARITY-95589-C12', 1),
    ('81', N'95589C8', N'CLARITY-95589-C8', 1),
    ('83', N'95589C10', N'CLARITY-95589-C10', 1),
    ('84', N'96021C3', N'CLARITY-96021-C3', 1),
    ('85', N'95270C10', N'CLARITY-95270-C10', 1),
    ('86', N'95296C6', N'CLARITY-95296-C6', 1),
    ('87', N'9768XC4', N'CLARITY-9768X-C4', 1),
    ('88', N'81059C4', N'CLARITY-81059-C4', 1),
    ('90', N'98970C6', N'CLARITY-98970-C6', 1),
    ('93', N'99976C13', N'CLARITY-99976-C13', 1),
    ('94', N'81525C6', N'CLARITY-81525-C6', 1),
    ('95', N'82142C8', N'CLARITY-82142-C8', 1),
    ('96', N'82142C7', N'CLARITY-82142-C7', 1),
    ('97', N'82033C1', N'CLARITY-82033-C1', 1),
    ('98', N'81053C6', N'CLARITY-81053-C6', 1),
    ('99', N'82108C7', N'CLARITY-82108-C7', 1),
    ('100', N'K2008C5', N'COOL-K2008-C5', 1),
    ('101', N'CFAC1', N'COOL FLEX-CF-A-C1', 1),
    ('102', N'LD90302C4', N'DIFF-LD90302-C4', 1),
    ('103', N'MASILMAT', N'DIVELLI-M-A-SIL/MAT', 1),
    ('104', N'91359C6', N'FANIA-91359-C6', 1),
    ('105', N'2112DORADO', N'FIRST-2112-DORADO', 1),
    ('107', N'XYFO106C5', N'FOOSE-XYFO106-C5', 1),
    ('108', N'XYFO104C1', N'FOOSE-XYFO104-C1', 1),
    ('110', N'XYFO101C3', N'FOOSE-XYFO101-C3', 1),
    ('111', N'GV1036C1', N'FOOSE-GV1036-C1', 1),
    ('113', N'XYFO065C3', N'FOOSE-XYFO065-C3', 1),
    ('114', N'XYFO098C2', N'FOOSE-XYFO098-C2', 1),
    ('115', N'FXFO116C2', N'FOOSE-FXFO116-C2', 1),
    ('116', N'GV1010C5', N'FOOSE-GV1010-C5', 1),
    ('118', N'F325BICOLOR', N'FRESH-F325-BICOLOR', 1),
    ('119', N'FK1021APO', N'FRIDA KAHLO-FK1021A-PO', 1),
    ('120', N'FK1004ABU', N'FRIDA KAHLO-FK1004A-BU', 1),
    ('121', N'FK1016MPI', N'FRIDA KAHLO-FK1016M-PI', 1),
    ('122', N'FK1020MPR', N'FRIDA KAHLO-FK1020M-PR', 1),
    ('126', N'GH2259C2', N'GATO FELIX-GH2259-C2', 1),
    ('130', N'G60152XC5', N'GATTIZONI-G60152X-C5', 1),
    ('131', N'G60161XC1', N'GATTIZONI-G60161X-C1', 1),
    ('132', N'G60161XC5', N'GATTIZONI-G60161X-C5', 1),
    ('133', N'G60125XC2', N'GATTIZONI-G60125X-C2', 1),
    ('140', N'G60164XC8', N'GATTIZONI-G60164X-C8', 1),
    ('143', N'11651C2', N'GAZAPO-11651-C2', 1),
    ('145', N'GE3563XC3', N'GEORGE-GE3563X-C3', 1),
    ('147', N'GE3512XC2', N'GEORGE-GE3512X-C2', 1),
    ('149', N'GE3547XC3A', N'GEORGE-GE3547X-C3A', 1),
    ('150', N'GE3748XC2', N'GEORGE-GE3748X-C2', 1),
    ('151', N'GE3748XC25', N'GEORGE-GE3748X-C25', 1),
    ('153', N'GV30059OS2', N'GUESS-GV30059-OS2', 1),
    ('157', N'FXJO032C5', N'JORGIO-FXTO032-C5', 1),
    ('164', N'FXJO030C3', N'JORGIO-FXTO030-C3', 2),
    ('174', N'FXJO022C13', N'JORGIO-FXTO022-C13', 1),
    ('178', N'DLJO011C3', N'JORGIO-DLJO011-C3', 1),
    ('180', N'KP312M232', N'KIPLING-KP312-M232', 1),
    ('181', N'RG24103C3', N'LAURA BAGIOTTI-RG24103-C3', 1),
    ('182', N'PD22006C6', N'LAURA BAGIOTTI-PD22006-C6', 1),
    ('183', N'PZ22030AC4', N'LEGIRO-PZ22030A-C4', 1),
    ('187', N'HLW20219C5', N'LEGIRO-HLW20219-C5', 1),
    ('208', N'PZ22029C6', N'LEGIRO-PZ22029-C6', 1),
    ('214', N'PZ22043C5', N'LEGIRO-PZ22043-C5', 1),
    ('218', N'PZ22029C3', N'LEGIRO-PZ22029-C3', 1),
    ('222', N'PZ22036C1', N'LEGIRO-PZ22036-C1', 4),
    ('223', N'FXN202075FC7', N'LINEA VIGO-FXN202075F-C7', 1),
    ('226', N'DS202012C1', N'LINEA VIGO-DS202012-C1', 1),
    ('227', N'LH2024C3', N'LUCERNA-LH2024-C3', 1),
    ('228', N'8824C5', N'LUCERNA-8824-C5', 1),
    ('229', N'LH3007C5', N'LUCERNA-LH3007-C5', 1),
    ('230', N'LH3007C2', N'LUCERNA-LH3007-C2', 2),
    ('234', N'10387CELESTETRANSPARENTE', N'LUCERNA-10387-CELESTE/TRANSPARENTE', 1),
    ('235', N'10387VERDETRANSPARENTE', N'LUCERNA-10387-VERDE/TRANSPARENTE', 2),
    ('236', N'8813C6', N'LUCERNA-8813-C6', 1),
    ('240', N'LU23036C1', N'LUXOR-LU23036-C1', 1),
    ('241', N'LU23031C1', N'LUXOR-LU23031-C1', 1),
    ('242', N'LU23027C1', N'LUXOR-LU23027-C1', 1),
    ('244', N'A17333C4', N'MAXS-A17333-C4', 2),
    ('245', N'MW16169C2', N'MAXS-MW16169-C2', 1),
    ('246', N'MW17165C2', N'MAXS-MW17165-C2', 1),
    ('257', N'MW17253C2', N'MAXS-MW17253-C2', 2),
    ('260', N'A17636C3', N'MAXS-A17636-C3', 1),
    ('271', N'MW15064C5', N'OCTAVIA-MW15064-C5', 2),
    ('276', N'MW16031C4', N'OCTAVIA-MW16031-C4', 1),
    ('279', N'MW16411C2', N'OCTAVIA-MW16411-C2', 1),
    ('282', N'AH996CARETRANSP', N'OSO EYEWEAR-AH996-CARE-TRANSP.', 1),
    ('283', N'M23388C3', N'OSO EYEWEAR-M23388-C3', 1),
    ('284', N'M23388C4', N'OSO EYEWEAR-M23388-C4', 1),
    ('285', N'5008DEMICAFE', N'OSO EYEWEAR-5008-DEMI CAFE', 1),
    ('286', N'81115C4', N'OSO EYEWEAR-81115-C4', 1),
    ('287', N'PL1912C2', N'POLO-PL1912-C2', 1),
    ('288', N'PL2155C1', N'POLO-PL2155-C1', 1),
    ('289', N'PL2165C4', N'POLO-PL2165-C4', 1),
    ('290', N'PL2161C1', N'POLO-PL2161-C1', 1),
    ('291', N'Z3337TRANSPARENTE', N'ROLETT-Z3337-TRANSPARENTE', 1),
    ('292', N'SCNEGROTRANSPARENTE', N'ROLETT-SC-NEGRO/TRANSPARENTE', 1),
    ('293', N'SCTRANSPARENTE', N'ROLETT-SC-TRANSPARENTE', 1),
    ('294', N'ZN3518NEGROCAFE', N'ROLETT-ZN3518-NEGRO/CAFE', 1),
    ('295', N'ZN3518BICOLOR', N'ROLETT-ZN3518-BICOLOR', 1),
    ('296', N'LH2120C1', N'ROLETT-LH2120-C1', 1),
    ('298', N'56167C1', N'VELTOM-56167-C1', 1),
    ('299', N'MF7755C3', N'VELTOM-MF7755-C3', 1),
    ('300', N'MY21409C1', N'VELTOM-MY21409-C1', 1),
    ('301', N'MG6308AC4', N'VELTOM-MG6308A-C4', 1),
    ('302', N'XC84031C4', N'VELTOM-XC8403-1-C4', 1),
    ('303', N'VE21029C1', N'VESPA-VE21029-C1', 1),
    ('304', N'VE21033FC4', N'VESPA-VE21033F-C4', 1),
    ('306', N'WO14C1', N'WIZZ-WO-14-C1', 1),
    ('308', N'XO4VERDE', N'XION-XO-4-VERDE', 1),
    ('309', N'YA14C5', N'YANGO-YA-14-C5', 1),
    ('310', N'YA16C5', N'YANGO-YA-16-C5', 1);

/* ---------------------------------------------------------------------
   3. Validaciones previas (no escriben nada)
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

IF EXISTS (SELECT 1 FROM #Nuevos n JOIN dbo.OPT_Producto p ON p.Codigo = n.Codigo)
BEGIN
    SELECT 'ERROR: el codigo nuevo ya existe' AS Problema, n.*, p.idProducto, p.Producto AS ProductoActual
    FROM #Nuevos n JOIN dbo.OPT_Producto p ON p.Codigo = n.Codigo;
    RAISERROR(N'Hay codigos nuevos que ya existen en OPT_Producto. Se aborta.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------
   4. EXISTENTES: fijar stock en la sucursal (con registro de regularizacion)
   --------------------------------------------------------------------- */
-- 4.1 Si algun producto no tiene fila en la sucursal, se crea en 0
INSERT INTO dbo.OPT_ProductoSucursal (idProducto, idSucursal, Entrada, Salida, Stock, StockMinimo, StockMaximo, PrecioVenta)
SELECT e.idProducto, @idSucursal, 0, 0, 0, 0, 0, 0
FROM #Existentes e
WHERE NOT EXISTS (SELECT 1 FROM dbo.OPT_ProductoSucursal ps
                  WHERE ps.idProducto = e.idProducto AND ps.idSucursal = @idSucursal);

-- 4.2 Registro historico (StockAnterior -> StockNuevo)
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
   6. Resumen / verificacion
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
-- Esperado: Existentes 167 / 184 u. - Nuevos 136 / 145 u. - Regularizaciones 303 / 329 u.

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
