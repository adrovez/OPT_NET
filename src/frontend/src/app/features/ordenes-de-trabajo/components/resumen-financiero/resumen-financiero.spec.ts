import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResumenFinanciero } from './resumen-financiero';

describe('ResumenFinanciero', () => {
  let component: ResumenFinanciero;
  let fixture: ComponentFixture<ResumenFinanciero>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResumenFinanciero],
    }).compileComponents();

    fixture = TestBed.createComponent(ResumenFinanciero);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('precio', 10000);
    fixture.componentRef.setInput('totalAbonado', 10000);
    fixture.componentRef.setInput('saldo', 0);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('avisa cuando la orden ya no tiene saldo pendiente', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Sin saldo pendiente');
  });
});
