/**
 * Conversión de `Date` (lo que produce el datepicker de Material) al formato que espera
 * cada tipo del backend. Se centraliza acá porque el módulo Comercial mezcla los tres:
 * `DateOnly` (fecha de atención, vencimiento de cuota), `TimeOnly` (hora de entrega) y
 * `DateTimeOffset` (fecha de entrega, fecha de pago).
 *
 * Todas trabajan en hora local a propósito: `toISOString()` a secas corre la fecha un día
 * cuando el navegador está al oeste de UTC, que es el caso de Chile.
 */

/** `DateOnly` del backend — "yyyy-MM-dd" en hora local. */
export function aFechaIso(fecha: Date): string {
  const anio = fecha.getFullYear();
  const mes = String(fecha.getMonth() + 1).padStart(2, '0');
  const dia = String(fecha.getDate()).padStart(2, '0');
  return `${anio}-${mes}-${dia}`;
}

/**
 * `DateTimeOffset` del backend. El datepicker entrega la fecha a medianoche local; se
 * envía con el offset del navegador para que el instante sea el mismo que ve el operador.
 */
export function aFechaHoraIso(fecha: Date): string {
  return new Date(fecha.getTime() - fecha.getTimezoneOffset() * 60000)
    .toISOString()
    .replace('Z', aOffsetIso(fecha));
}

/** `TimeOnly` del backend — "HH:mm:ss" a partir del valor de un `<input type="time">`. */
export function aHoraIso(hora: string): string | null {
  const partes = /^(\d{2}):(\d{2})(?::(\d{2}))?$/.exec(hora.trim());
  if (!partes) {
    return null;
  }
  return `${partes[1]}:${partes[2]}:${partes[3] ?? '00'}`;
}

/** "HH:mm" para poblar un `<input type="time">` desde el "HH:mm:ss" del backend. */
export function aHoraCorta(hora: string | null): string {
  return hora ? hora.slice(0, 5) : '';
}

function aOffsetIso(fecha: Date): string {
  const minutos = -fecha.getTimezoneOffset();
  const signo = minutos >= 0 ? '+' : '-';
  const absoluto = Math.abs(minutos);
  const horas = String(Math.floor(absoluto / 60)).padStart(2, '0');
  const resto = String(absoluto % 60).padStart(2, '0');
  return `${signo}${horas}:${resto}`;
}
