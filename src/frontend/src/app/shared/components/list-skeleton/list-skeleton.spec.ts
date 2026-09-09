import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListSkeleton } from './list-skeleton';

describe('ListSkeleton', () => {
  let fixture: ComponentFixture<ListSkeleton>;

  beforeEach(async () => {
    TestBed.configureTestingModule({ imports: [ListSkeleton] });
    fixture = TestBed.createComponent(ListSkeleton);
    await fixture.whenStable();
  });

  it('renderiza la cantidad de filas pedida más la cabecera', async () => {
    fixture.componentRef.setInput('rows', 3);
    await fixture.whenStable();
    const barras = fixture.nativeElement.querySelectorAll('.skeleton__barra');
    expect(barras.length).toBe(4); // 3 filas + 1 cabecera
  });

  it('anuncia el estado de carga a lectores de pantalla', () => {
    const status = fixture.nativeElement.querySelector('[role="status"]');
    expect(status.textContent).toContain('Cargando');
  });
});
