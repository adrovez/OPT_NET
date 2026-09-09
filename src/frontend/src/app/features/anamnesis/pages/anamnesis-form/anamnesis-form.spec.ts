import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { AnamnesisForm, AnamnesisFormDialogData } from './anamnesis-form';

describe('AnamnesisForm', () => {
  let component: AnamnesisForm;
  let fixture: ComponentFixture<AnamnesisForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnamnesisForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        {
          provide: MAT_DIALOG_DATA,
          useValue: { clientePublicId: 'cliente-1' } satisfies AnamnesisFormDialogData,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AnamnesisForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
