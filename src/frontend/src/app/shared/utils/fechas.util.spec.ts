import { aFechaIso, aHoraCorta, aHoraIso } from './fechas.util';

describe('fechas.util', () => {
  it('aFechaIso usa la fecha local, no la UTC', () => {
    // 23:30 local del 27 — en UTC-4 esto es el 28 en UTC; debe seguir siendo el 27.
    expect(aFechaIso(new Date(2026, 7, 27, 23, 30))).toBe('2026-08-27');
  });

  it('aHoraIso completa los segundos', () => {
    expect(aHoraIso('18:30')).toBe('18:30:00');
  });

  it('aHoraIso rechaza un texto que no es una hora', () => {
    expect(aHoraIso('tarde')).toBeNull();
  });

  it('aHoraCorta recorta los segundos del backend', () => {
    expect(aHoraCorta('18:30:00')).toBe('18:30');
    expect(aHoraCorta(null)).toBe('');
  });
});
