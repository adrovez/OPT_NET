import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ORDEN_TICKET } from '../../models/orden-de-trabajo.fixture';
import { TicketOT } from './ticket-ot';

describe('TicketOT', () => {
  let component: TicketOT;
  let fixture: ComponentFixture<TicketOT>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [TicketOT] }).compileComponents();

    fixture = TestBed.createComponent(TicketOT);
    fixture.componentRef.setInput('orden', ORDEN_TICKET);
    await fixture.whenStable();
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('imprime el N° de OT, el cliente y el detalle', () => {
    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(texto).toContain('17067');
    expect(texto).toContain('JUANA PEREZ');
    expect(texto).toContain('FORMOSA F4 C2');
  });

  it('deja fuera las cuotas anuladas', () => {
    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(texto).toContain('1 ×');
  });
});
