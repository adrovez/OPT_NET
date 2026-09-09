import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formatea un monto en pesos chilenos: sin decimales y con separador de miles local
 * ($ 12.578). Usa `Intl.NumberFormat` en vez del `CurrencyPipe` de Angular porque este
 * último exige registrar los datos de locale es-CL en el bundle inicial (y sin registrarlos
 * imprime "CLP12,578", con la separación equivocada para Chile).
 *
 * Todo monto del módulo Comercial (precio de OT, abonos, pagos, cuotas, saldos) se muestra
 * con este pipe — nunca con `number`/`currency` sueltos, para que la moneda se vea igual
 * en toda la aplicación.
 */
@Pipe({
  name: 'pesos',
})
export class PesosPipe implements PipeTransform {
  private static readonly formato = new Intl.NumberFormat('es-CL', {
    style: 'currency',
    currency: 'CLP',
    maximumFractionDigits: 0,
  });

  transform(valor: number | null | undefined): string {
    if (valor === null || valor === undefined || Number.isNaN(valor)) {
      return '—';
    }
    return PesosPipe.formato.format(valor);
  }
}
