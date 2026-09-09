import { PesosPipe } from './pesos-pipe';

describe('PesosPipe', () => {
  const pipe = new PesosPipe();

  it('should create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('formatea el monto sin decimales', () => {
    // El separador de miles de es-CL es el punto; el espacio tras el símbolo puede ser
    // un espacio duro según la versión de ICU, por eso se compara sin espacios.
    expect(pipe.transform(12578).replace(/\s/g, '')).toBe('$12.578');
  });

  it('devuelve un guion largo cuando no hay monto', () => {
    expect(pipe.transform(null)).toBe('—');
  });

  it('formatea el cero como monto, no como vacío', () => {
    expect(pipe.transform(0).replace(/\s/g, '')).toBe('$0');
  });
});
