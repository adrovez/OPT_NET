import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { OperativoForm, OperativoFormDialogData } from './operativo-form';

describe('OperativoForm', () => {
  let component: OperativoForm;
  let fixture: ComponentFixture<OperativoForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OperativoForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: {} as OperativoFormDialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OperativoForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('empieza en modo alta, sin empresa/sucursal deshabilitadas', () => {
    expect(component['esEdicion']).toBe(false);
    expect(component['form'].controls.sucursalId.disabled).toBe(false);
  });
});
