import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RecetaCristales } from '../../models/receta-cristales.model';
import { RecetaGraduacion } from './receta-graduacion';

const RECETA: RecetaCristales = {
  publicId: '11111111-1111-1111-1111-111111111111',
  clientePublicId: '22222222-2222-2222-2222-222222222222',
  odEsferaLejos: -1.25,
  odCilindroLejos: null,
  odEjeLejos: null,
  odEsferaCerca: null,
  odCilindroCerca: null,
  odEjeCerca: null,
  oiEsferaLejos: -2,
  oiCilindroLejos: null,
  oiEjeLejos: null,
  oiEsferaCerca: null,
  oiCilindroCerca: null,
  oiEjeCerca: null,
  urgente: false,
  requiereLab: false,
  observaciones: null,
  dpLejos: '64',
  dpCerca: null,
  addLejos: null,
  incluirLejos: true,
  incluirCerca: false,
  observacionOdLejos: null,
  observacionOiLejos: null,
  observacionDpLejos: null,
  observacionOdCerca: null,
  observacionOiCerca: null,
  observacionDpCerca: null,
  fechaRegistro: '2018-09-25T00:00:00+00:00',
};

describe('RecetaGraduacion', () => {
  let component: RecetaGraduacion;
  let fixture: ComponentFixture<RecetaGraduacion>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecetaGraduacion],
    }).compileComponents();

    fixture = TestBed.createComponent(RecetaGraduacion);
    fixture.componentRef.setInput('receta', RECETA);
    await fixture.whenStable();
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('muestra la graduación de lejos y el DP', () => {
    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(texto).toContain('-1.25');
    expect(texto).toContain('-2');
    expect(texto).toContain('64');
  });

  it('marca "(no incluye)" el bloque cuyo check de inclusión está apagado', () => {
    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(texto).toContain('Cerca (no incluye)');
    expect(texto).not.toContain('Lejos (no incluye)');
  });

  it('omite la fila de DP cuando no hay dato ni observación', async () => {
    fixture.componentRef.setInput('receta', { ...RECETA, dpLejos: null, observacionDpLejos: null });
    await fixture.whenStable();
    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(texto).not.toContain('DP');
  });

  it('omite el párrafo de observaciones generales cuando no hay ninguna', () => {
    expect((fixture.nativeElement as HTMLElement).querySelector('.nota')).toBeNull();
  });
});
