import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { CuotaPagoDialog, CuotaPagoDialogData } from './cuota-pago-dialog';

const datos: CuotaPagoDialogData = {
  numero: 1,
  valorCuota: 10000,
  formasPago: [{ id: 1, nombre: 'EFECTIVO' }],
};

describe('CuotaPagoDialog', () => {
  let component: CuotaPagoDialog;
  let fixture: ComponentFixture<CuotaPagoDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CuotaPagoDialog, NoopAnimationsModule],
      providers: [
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: datos },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CuotaPagoDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('advierte que no descuenta el saldo de la orden', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('no registra un pago');
  });
});
