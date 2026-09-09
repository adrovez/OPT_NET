import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { EmpresaForm, EmpresaFormDialogData } from './empresa-form';

describe('EmpresaForm', () => {
  let component: EmpresaForm;
  let fixture: ComponentFixture<EmpresaForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmpresaForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: {} satisfies EmpresaFormDialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EmpresaForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
