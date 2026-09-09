import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { RecetaCristalesForm, RecetaCristalesFormDialogData } from './receta-cristales-form';

describe('RecetaCristalesForm', () => {
  let component: RecetaCristalesForm;
  let fixture: ComponentFixture<RecetaCristalesForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecetaCristalesForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        {
          provide: MAT_DIALOG_DATA,
          useValue: { clientePublicId: 'cliente-1' } satisfies RecetaCristalesFormDialogData,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RecetaCristalesForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
