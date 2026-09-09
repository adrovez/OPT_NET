import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { SucursalForm, SucursalFormDialogData } from './sucursal-form';

describe('SucursalForm', () => {
  let component: SucursalForm;
  let fixture: ComponentFixture<SucursalForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SucursalForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: {} satisfies SucursalFormDialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SucursalForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
