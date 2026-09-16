import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';

import { GastoOperativoDialog } from './gasto-operativo-dialog';

describe('GastoOperativoDialog', () => {
  let component: GastoOperativoDialog;
  let fixture: ComponentFixture<GastoOperativoDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GastoOperativoDialog],
      providers: [{ provide: MatDialogRef, useValue: { close: () => undefined } }],
    }).compileComponents();

    fixture = TestBed.createComponent(GastoOperativoDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('no permite guardar sin un monto válido', () => {
    expect(component['form'].invalid).toBe(true);
  });
});
