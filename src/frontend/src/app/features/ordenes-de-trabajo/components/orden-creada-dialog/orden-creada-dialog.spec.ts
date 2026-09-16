import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { ORDEN_TICKET } from '../../models/orden-de-trabajo.fixture';
import { OrdenCreadaDialog } from './orden-creada-dialog';

describe('OrdenCreadaDialog', () => {
  let component: OrdenCreadaDialog;
  let fixture: ComponentFixture<OrdenCreadaDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdenCreadaDialog, NoopAnimationsModule],
      providers: [
        { provide: MatDialogRef, useValue: { close: () => undefined } },
        { provide: MAT_DIALOG_DATA, useValue: { orden: ORDEN_TICKET } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OrdenCreadaDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('anuncia el número de la orden recién creada', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('17067');
  });
});
