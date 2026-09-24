/**
 * Excel del Reporte de Cristales del Operativo, para enviar al proveedor. Replica el
 * "Listado de cristales" del legacy (título, bloque TOTALES, bloque DETALLE con una fila por
 * receta y las medidas apiladas en líneas dentro de cada celda), pero SIN Empresa ni Sucursal
 * ni datos del cliente, ordenado por N° de OT y con la OT como primera columna.
 */
import { RecetaCristales } from '../../receta-cristales/models/receta-cristales.model';
import { ReporteCristalesItem } from '../models/operativo.model';

/**
 * Reglas del legacy (dsListaCristales.xsd): un bloque (Lejos/Cerca) solo cuenta si su check
 * "Incluir" está marcado; un ojo cuenta si tiene Esfera o Cilindro (el Eje no cuenta). La
 * "Cantidad" de una fila es la suma de esos ojos con datos, no un valor fijo.
 */
const tieneOjo = (esfera: number | null, cilindro: number | null): boolean =>
  esfera !== null || cilindro !== null;

const cuentaLejosOD = (r: RecetaCristales) =>
  r.incluirLejos && tieneOjo(r.odEsferaLejos, r.odCilindroLejos);
const cuentaLejosOI = (r: RecetaCristales) =>
  r.incluirLejos && tieneOjo(r.oiEsferaLejos, r.oiCilindroLejos);
const cuentaCercaOD = (r: RecetaCristales) =>
  r.incluirCerca && tieneOjo(r.odEsferaCerca, r.odCilindroCerca);
const cuentaCercaOI = (r: RecetaCristales) =>
  r.incluirCerca && tieneOjo(r.oiEsferaCerca, r.oiCilindroCerca);

const cantidad = (r: RecetaCristales): number =>
  [cuentaLejosOD, cuentaLejosOI, cuentaCercaOD, cuentaCercaOI].filter((f) => f(r)).length;

/** Como el legacy: si el bloque no está incluido, sus datos salen en blanco. */
function soloIncluido(r: RecetaCristales): RecetaCristales {
  const sinLejos = !r.incluirLejos;
  const sinCerca = !r.incluirCerca;
  return {
    ...r,
    odEsferaLejos: sinLejos ? null : r.odEsferaLejos,
    odCilindroLejos: sinLejos ? null : r.odCilindroLejos,
    odEjeLejos: sinLejos ? null : r.odEjeLejos,
    oiEsferaLejos: sinLejos ? null : r.oiEsferaLejos,
    oiCilindroLejos: sinLejos ? null : r.oiCilindroLejos,
    oiEjeLejos: sinLejos ? null : r.oiEjeLejos,
    observacionOdLejos: sinLejos ? null : r.observacionOdLejos,
    observacionOiLejos: sinLejos ? null : r.observacionOiLejos,
    observacionDpLejos: sinLejos ? null : r.observacionDpLejos,
    dpLejos: sinLejos ? null : r.dpLejos,
    addLejos: sinLejos ? null : r.addLejos,
    odEsferaCerca: sinCerca ? null : r.odEsferaCerca,
    odCilindroCerca: sinCerca ? null : r.odCilindroCerca,
    odEjeCerca: sinCerca ? null : r.odEjeCerca,
    oiEsferaCerca: sinCerca ? null : r.oiEsferaCerca,
    oiCilindroCerca: sinCerca ? null : r.oiCilindroCerca,
    oiEjeCerca: sinCerca ? null : r.oiEjeCerca,
    observacionOdCerca: sinCerca ? null : r.observacionOdCerca,
    observacionOiCerca: sinCerca ? null : r.observacionOiCerca,
    observacionDpCerca: sinCerca ? null : r.observacionDpCerca,
    dpCerca: sinCerca ? null : r.dpCerca,
  };
}

const medida = (v: number | null): string =>
  v === null ? '' : `${v > 0 ? '+' : ''}${v.toFixed(2)}`;
const eje = (v: number | null): string => (v === null ? '' : String(v));

const SALTO = String.fromCharCode(10);
const apilar = (lineas: (string | null)[]): string => lineas.map((l) => l ?? '').join(SALTO);

function filaReceta(ot: number, r: RecetaCristales, cant: number): (string | number)[] {
  return [
    ot,
    cant,
    r.urgente ? 'SI' : 'NO',
    r.requiereLab ? 'SI' : 'NO',
    apilar(['Lejos OD', 'Lejos OI', 'Cerca OD', 'Cerca OI']),
    apilar([
      medida(r.odEsferaLejos),
      medida(r.oiEsferaLejos),
      medida(r.odEsferaCerca),
      medida(r.oiEsferaCerca),
    ]),
    apilar([
      medida(r.odCilindroLejos),
      medida(r.oiCilindroLejos),
      medida(r.odCilindroCerca),
      medida(r.oiCilindroCerca),
    ]),
    apilar([eje(r.odEjeLejos), eje(r.oiEjeLejos), eje(r.odEjeCerca), eje(r.oiEjeCerca)]),
    apilar([
      r.observacionOdLejos,
      r.observacionOiLejos,
      r.observacionDpLejos,
      r.observacionOdCerca,
      r.observacionOiCerca,
      r.observacionDpCerca,
    ]),
    apilar([r.dpLejos, ' ', r.dpCerca]),
    r.addLejos ?? '',
  ];
}

export async function descargarExcelCristales(
  items: ReporteCristalesItem[],
  nombreArchivo: string,
): Promise<void> {
  // Carga diferida: la librería es pesada y solo se necesita al exportar.
  const { Workbook } = await import('exceljs');
  const libro = new Workbook();
  const hoja = libro.addWorksheet('rptListaCristales');
  const ordenados = [...items].sort((a, b) => a.numeroOT - b.numeroOT);
  // Como el legacy: solo recetas con "Incluir Lejos" o "Incluir Cerca" marcado.
  const recetas = ordenados.flatMap((i) =>
    i.recetas.filter((r) => r.incluirLejos || r.incluirCerca).map((r) => ({ ot: i.numeroOT, r })),
  );

  const borde = { style: 'thin' as const };
  const bordes = { top: borde, left: borde, bottom: borde, right: borde };
  const relleno = (argb: string) => ({
    type: 'pattern' as const,
    pattern: 'solid' as const,
    fgColor: { argb },
  });
  const centrado = { horizontal: 'center' as const, vertical: 'middle' as const, wrapText: true };

  [12, 10, 10, 20, 14, 10, 10, 8, 30, 8, 8].forEach((w, i) => (hoja.getColumn(i + 1).width = w));

  // Título
  hoja.mergeCells('A1:K1');
  const titulo = hoja.getCell('A1');
  titulo.value = 'LISTADO CRISTALES';
  titulo.font = { name: 'Arial', bold: true, size: 18 };
  titulo.alignment = centrado;
  hoja.getRow(1).height = 26;

  // TOTALES: cantidad de cristales con datos por ojo y distancia
  const cuenta = (f: (r: RecetaCristales) => boolean) => recetas.filter(({ r }) => f(r)).length;
  hoja.mergeCells('A3:D3');
  hoja.getCell('A3').value = 'TOTALES';
  hoja.getCell('B4').value = 'OD';
  hoja.getCell('C4').value = 'OI';
  hoja.getCell('A5').value = 'Lejos';
  hoja.getCell('B5').value = cuenta(cuentaLejosOD);
  hoja.getCell('C5').value = cuenta(cuentaLejosOI);
  hoja.getCell('A6').value = 'Cerca';
  hoja.getCell('B6').value = cuenta(cuentaCercaOD);
  hoja.getCell('C6').value = cuenta(cuentaCercaOI);
  for (const ref of ['A3', 'B4', 'C4']) {
    hoja.getCell(ref).font = { name: 'Arial', bold: true, size: 9 };
    hoja.getCell(ref).fill = relleno('FFD9D9D9');
  }
  for (const fila of [3, 4, 5, 6]) {
    for (let c = 1; c <= (fila === 3 ? 4 : 3); c++) {
      const cel = hoja.getCell(fila, c);
      cel.border = bordes;
      cel.alignment = centrado;
      cel.font = { name: 'Arial', size: 9, bold: fila <= 4 || c === 1 };
    }
  }

  // DETALLE
  hoja.mergeCells('A8:K8');
  const detalle = hoja.getCell('A8');
  detalle.value = 'DETALLE';
  detalle.font = { name: 'Arial', bold: true, size: 9 };
  detalle.fill = relleno('FFD9D9D9');
  detalle.alignment = centrado;
  detalle.border = bordes;

  const encabezado = hoja.addRow([
    'OT',
    'Cantidad',
    'Urgente',
    'Cristal Laboratorio',
    'OD / OI',
    'Esfera',
    'Cilindro',
    'Eje',
    'Observación',
    'DP',
    'ADD',
  ]);
  encabezado.height = 22;
  encabezado.eachCell((c) => {
    c.font = { name: 'Arial', bold: true, size: 9 };
    c.fill = relleno('FFBFBFBF');
    c.alignment = centrado;
    c.border = bordes;
  });

  for (const { ot, r } of recetas) {
    const fila = hoja.addRow(filaReceta(ot, soloIncluido(r), cantidad(r)));
    fila.height = 67.5;
    fila.eachCell({ includeEmpty: true }, (c, col) => {
      c.font = { name: 'Arial', size: 9 };
      c.border = bordes;
      c.alignment = { ...centrado, horizontal: col === 5 || col === 9 ? 'left' : 'center' };
    });
  }

  const buffer = await libro.xlsx.writeBuffer();
  const blob = new Blob([buffer], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  });
  const url = URL.createObjectURL(blob);
  const enlace = document.createElement('a');
  enlace.href = url;
  enlace.download = nombreArchivo;
  enlace.click();
  URL.revokeObjectURL(url);
}
