import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

import { imprimirConClaseBody } from '../../../../shared/utils/impresion.util';
import { RecetaGraduacion } from '../../../receta-cristales/components/receta-graduacion/receta-graduacion';
import { EstadoOtChip } from '../../../ordenes-de-trabajo/components/estado-ot-chip/estado-ot-chip';
import { ReporteCristalesItem } from '../../models/operativo.model';

export interface ReporteCristalesImprimibleData {
  operativoNombre: string;
  items: ReporteCristalesItem[];
}

/**
 * HU-OP-10: versión imprimible del Reporte de Cristales de un Operativo — la "exportación a
 * PDF" que pide el criterio de aceptación se resuelve con Imprimir → Guardar como PDF del
 * navegador, mismo mecanismo que ya usa el ticket de OT (`shared/utils/impresion.util.ts`), en
 * vez de agregar una librería de generación de PDF nueva al proyecto.
 */
@Component({
  selector: 'app-reporte-cristales-imprimible',
  imports: [MatButtonModule, MatDialogModule, MatIconModule, EstadoOtChip, RecetaGraduacion],
  templateUrl: './reporte-cristales-imprimible.html',
  styleUrl: './reporte-cristales-imprimible.scss',
})
export class ReporteCristalesImprimible {
  protected readonly data = inject<ReporteCristalesImprimibleData>(MAT_DIALOG_DATA);

  protected imprimir(): void {
    imprimirConClaseBody();
  }
}
