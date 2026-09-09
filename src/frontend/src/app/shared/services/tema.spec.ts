import { TestBed } from '@angular/core/testing';

import { Tema } from './tema';

describe('Tema', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-opt-theme');
    TestBed.configureTestingModule({});
  });

  it('parte en la preferencia "system" y sin atributo en <html>', () => {
    const tema = TestBed.inject(Tema);
    expect(tema.preferencia()).toBe('system');
    expect(document.documentElement.hasAttribute('data-opt-theme')).toBe(false);
  });

  it('fija el atributo data-opt-theme y lo persiste al elegir un tema explícito', () => {
    const tema = TestBed.inject(Tema);
    tema.fijar('dark');
    expect(document.documentElement.getAttribute('data-opt-theme')).toBe('dark');
    expect(localStorage.getItem('opt.tema')).toBe('dark');
    expect(tema.oscuro()).toBe(true);
  });

  it('quita el atributo al volver a "system"', () => {
    const tema = TestBed.inject(Tema);
    tema.fijar('dark');
    tema.fijar('system');
    expect(document.documentElement.hasAttribute('data-opt-theme')).toBe(false);
  });

  it('alterna a partir del tema efectivo actual', () => {
    const tema = TestBed.inject(Tema);
    tema.fijar('light');
    tema.alternar();
    expect(tema.preferencia()).toBe('dark');
  });
});
