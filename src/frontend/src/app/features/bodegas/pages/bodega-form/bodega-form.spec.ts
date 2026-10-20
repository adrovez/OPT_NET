import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { BodegaForm, BodegaFormDialogData } from './bodega-form';

describe('BodegaForm', () => {
  let component: BodegaForm;
  let fixture: ComponentFixture<BodegaForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BodegaForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: { sucursalId: 1 } satisfies BodegaFormDialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BodegaForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('exige código y nombre', () => {
    expect(fixture.nativeElement.querySelector('button[type=submit]').disabled).toBe(true);
  });
});
