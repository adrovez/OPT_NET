import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EstadoOperativoChip } from './estado-operativo-chip';

describe('EstadoOperativoChip', () => {
  let component: EstadoOperativoChip;
  let fixture: ComponentFixture<EstadoOperativoChip>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EstadoOperativoChip],
    }).compileComponents();

    fixture = TestBed.createComponent(EstadoOperativoChip);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('estadoId', 1);
    fixture.componentRef.setInput('nombre', 'PROSPECTO');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('muestra el nombre del estado que envía el backend', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('PROSPECTO');
  });

  it('aplica el tono de alerta al estado ANULADO', async () => {
    fixture.componentRef.setInput('estadoId', 5);
    fixture.componentRef.setInput('nombre', 'ANULADO');
    await fixture.whenStable();

    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.opt-chip--alerta'),
    ).toBeTruthy();
  });
});
