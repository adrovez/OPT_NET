import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmptyState } from './empty-state';

describe('EmptyState', () => {
  let component: EmptyState;
  let fixture: ComponentFixture<EmptyState>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyState],
    }).compileComponents();

    fixture = TestBed.createComponent(EmptyState);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('message', 'No hay datos para mostrar.');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('muestra el mensaje recibido', () => {
    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(texto).toContain('No hay datos para mostrar.');
  });
});
