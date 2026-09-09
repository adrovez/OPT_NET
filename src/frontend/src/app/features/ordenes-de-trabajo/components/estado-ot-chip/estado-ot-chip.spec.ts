import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EstadoOtChip } from './estado-ot-chip';

describe('EstadoOtChip', () => {
  let component: EstadoOtChip;
  let fixture: ComponentFixture<EstadoOtChip>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EstadoOtChip],
    }).compileComponents();

    fixture = TestBed.createComponent(EstadoOtChip);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('estadoId', 0);
    fixture.componentRef.setInput('nombre', 'INGRESADO');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('muestra el nombre del estado que envía el backend', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('INGRESADO');
  });

  it('aplica la clase del estado ANULADO', async () => {
    fixture.componentRef.setInput('estadoId', 7);
    fixture.componentRef.setInput('nombre', 'ANULADO');
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('.estado--anulado')).toBeTruthy();
  });
});
