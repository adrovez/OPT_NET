import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';

import { AsociarOrdenDialog } from './asociar-orden-dialog';

describe('AsociarOrdenDialog', () => {
  let component: AsociarOrdenDialog;
  let fixture: ComponentFixture<AsociarOrdenDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AsociarOrdenDialog],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close: () => undefined } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AsociarOrdenDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
